﻿
// Phelix.NET - Copyright 2006 Markus Hahn - All rights reserved.

// experimental switch to see the performance improvement of local Z[] caching
// (measured to gain 25% speed) - NOT RECOMMENDED TO USE EXCEPT FOR SUCH TESTS!
//#define NO_Z_LOCALS

/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)
 Copyright (C) 2019 Simon Dudley

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace Highlander.Utilities.Encryption
{
    /// <summary>
    /// Phelix error container. Expect such an expectation for any class and
    /// method of this namespace to be on the safe side. 
    /// </summary>
    public class PhelixException : Exception
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        /// <param name="message">textual error message</param>
        public PhelixException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// The Phelix implementation. Optimized for re-usability, which makes it
    /// not thread safe, thus you should use one instance per thread to avoid
    /// trouble. Note that using this class directly is not really recommend,
    /// since certain knowledge about handling the nonce and storing the MAC
    /// is required.
    /// </summary>
    public sealed class Phelix
    {
        #region Public Constants

        /// <summary>
        /// Size of a nonce value in bits.
        /// </summary>
        public const int PhelixNonceSize = 128;
        /// <summary>
        /// Size of a standard Phelix MAC (128bit) in bits.
        /// </summary>
        public const int PhelixMacSize = 128;
        /// <summary>
        /// Size of a compact Phelix MAC (96bit) in bits.
        /// </summary>
        public const int PhelixMacSize96 = 96;
        /// <summary>
        /// Maximum size of a key for Phelix in bits.
        /// </summary>
        public const int PhelixKeySize = 256;
        /// <summary>
        /// Block size (in bytes) for continuous data passed for encryption and
        /// decryption.
        /// </summary>
        public const int PhelixDataAlign = 4;

        #endregion

        #region Private Constants

        const int OldZReg = 4;
        const int ZeroInitCnt = 8;
        const int MacInitCnt = 8;
        const int MacWordCnt = PhelixMacSize / 32;
        const int Rot0A = 9;
        const int Rot1A = 10;
        const int Rot2A = 17;
        const int Rot3A = 30;
        const int Rot4A = 13;
        const int Rot0B = 20;
        const int Rot1B = 11;
        const int Rot2B = 5;
        const int Rot3B = 15;
        const int Rot4B = 25;
        const uint MacMagicXOR = 0x912d94f1;
        const uint AadMagicXOR = 0xaadaadaa;

        static readonly uint[] MaskLeftover = { 0, 0x00ff, 0x00ffff, 0x00ffffff };

        #endregion

        #region State

        // key setup state
        uint _ksMacSize;
        uint _ksX1Bump;
        readonly uint[] _ksX0 = new uint[8];

        readonly uint[] _ksX1 = new uint[8];
        // cipher state
        readonly uint[] _csOldZ = new uint[4];
        readonly uint[] _csZ = new uint[5];
        uint _csI;
        ulong _csAadLen;
        ulong _csMsgLen;
        uint _csAadXor;

        #endregion

        #region Local Caches

        readonly uint[] _cacheFinalizeTmp = new uint[MacInitCnt + MacWordCnt];
        readonly uint[] _cacheProcessbytesBuckets = new uint[2];

        #endregion

        /// <summary>
        /// Default ctor. Notice that a new instance is not usable without
        /// consecutive and proper setup calls!
        /// </summary>
        public Phelix()
        {
            // this is a matter of discussion: should Init() always be called explicitly?
            Init();
        }

        #region API

        /// <summary>
        /// Resets the internal state. To be called before the actual setup.
        /// </summary>
        public void Init()
        {
            _csI = 0;
            _csMsgLen = 0;
        }

        /// <summary>
        /// Sets up the instance with a key.
        /// </summary>
        /// <param name="key">buffer containing the key material</param>
        /// <param name="keyOfs">offset where the key material starts</param>
        /// <param name="keySize">size of the key in bits, up to PHELIX_KEY_SIZE,
        /// yet must be aligned on a byte boundary</param>
        /// <param name="macSize">size of the MAC (in bits) to use, either the
        /// values PHELIX_MAC_SIZE or PHELIX_MAC_SIZE_96 are valid</param>
        public void SetupKey(
            byte[] key,
            int keyOfs,
            int keySize,
            int macSize)
        {
            int i;
            uint[] X = _ksX0;
            if (PhelixKeySize < keySize || keySize < 0)
            {
                throw new PhelixException($"invalid key size {keySize}");
            }
            if (0 != (keySize & 7))
            {
                throw new PhelixException("key must be byte-sized");
            }
            _ksMacSize = (uint)macSize;
            _ksX1Bump = (uint)((keySize >> 1) + ((macSize % PhelixMacSize) << 8));
            var tmp = (keySize + 31) >> 5;
            for (i = 0; i < tmp; i++)
            {
                X[i] = key[keyOfs] |
                       ((uint)key[keyOfs + 1]) << 8 |
                       ((uint)key[keyOfs + 2]) << 16 |
                       ((uint)key[keyOfs + 3]) << 24;
                keyOfs += 4;
            }
            for (; i < 8; i++)
            {
                X[i] = 0;
            }
            if (0 != (0x1f & keySize))
            {
                X[keySize >> 5] &= (((uint)keySize & 0x1f) << 1) - 1;
            }
            tmp = (keySize >> 3) + 64;
            for (i = 0; i < 8; i++)
            {
                var k = (i & 1) << 2;
                var z0 = X[k];
                var z1 = X[k + 1];
                var z2 = X[k + 2];
                var z3 = X[k + 3];
                var z4 = (uint)tmp;
                // check if the compiler creates the constants!
                int rep;
                for (rep = 0; rep < 2; rep++)
                {
                    z0 += z3; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                    z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                    z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                    z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                    z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));
                    z0 ^= z3; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                    z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                    z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                    z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                    z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));
                }
                k = (k + 4) & 7;
                X[k] ^= z0;
                X[k + 1] ^= z1;
                X[k + 2] ^= z2;
                X[k + 3] ^= z3;
            }
        }

        /// <summary>
        /// Sets up the nonce (aka the initialization vector or IV), which makes
        /// each encryption unique. It is absolutely recommended to choose a new
        /// and secure random nonce for every new encryption stage. Failure to do
        /// so leads to possible weaknesses. The size of the nonce must always
        /// be PHELIX_NONCE_SIZE bits (16 bytes).
        /// </summary>
        /// <param name="nonce">buffer containing the nonce material</param>
        /// <param name="nonceOfs">where the nonce starts</param>
        public void SetupNonce(
            byte[] nonce,
            int nonceOfs)
        {
            uint i;
            var x0 = _ksX0;
            var x1 = _ksX1;
            var x1Bump = _ksX1Bump;
            var Z = _csZ;
            var oldZ = _csOldZ;
            for (i = 0; i < 4; i++, nonceOfs += 4)
            {
                var n = nonce[nonceOfs] |
                         ((uint)nonce[nonceOfs + 1] << 8) |
                         ((uint)nonce[nonceOfs + 2] << 16) |
                         ((uint)nonce[nonceOfs + 3] << 24);
                x1[i] = x0[i + 4] + n;
                x1[i + 4] = x0[i] + (i - n);
                Z[i] = x0[i + 3] ^ n;
            }
            x1[1] += x1Bump;
            x1[5] += x1Bump;
            Z[4] = x0[7];
            _csAadLen = 0L;
            _csMsgLen = 0L;
            var z0 = Z[0];
            var z1 = Z[1];
            var z2 = Z[2];
            var z3 = Z[3];
            var z4 = Z[4];
            for (i = 0; i < 8; i++)
            {
                var j = i & 7;
                z0 += z3; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));
                z0 ^= z3 + x0[j]; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));
                z0 += z3; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));
                z0 ^= z3 + x1[j] + i; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));
                oldZ[i & 3] = z4; //Z[OLD_Z_REG];
            }
            Z[0] = z0;
            Z[1] = z1;
            Z[2] = z2;
            Z[3] = z3;
            Z[4] = z4;
            Z[1] ^= (_csAadXor = AadMagicXOR);
            _csI = i;
        }

        /// <summary>
        /// Encrypts a certain number of bytes. Can be called repeatedly, with
        /// the exception that expect of the last portion each chunk of data
        /// must be aligned to a PHELIX_DATA_ALIGN border. Notice that the size
        /// of the produced cipher text is exactly the size of the plaintext.
        /// </summary>
        /// <param name="inbuf">buffer containing the plaintext</param>
        /// <param name="inbufOfs">where the plaintext starts</param>
        /// <param name="outbuf">buffer where to put the cipher text</param>
        /// <param name="outbufOfs">where to start writing the cipher text</param>
        /// <param name="msgLen">number of bytes to encrypt</param>
        public void EncryptBytes(
            byte[] inbuf,
            int inbufOfs,
            byte[] outbuf,
            int outbufOfs,
            int msgLen)
        {
            ProcessBytes(inbuf, inbufOfs, outbuf, outbufOfs, msgLen, 0);
        }

        /// <summary>
        /// Decrypts a certain number of bytes. Can be called repeatedly, with
        /// the exception that expect of the last portion each chunk of data
        /// must be aligned to a PHELIX_DATA_ALIGN border. Notice that the size
        /// of the produced plaintext is exactly the size of the ciphetext.
        /// </summary>
        /// <param name="inbuf">buffer containing the ciphetext</param>
        /// <param name="inbufOfs">where the ciphetext starts</param>
        /// <param name="outbuf">buffer where to put the plaintext</param>
        /// <param name="outbufOfs">where to start writing the plaintext</param>
        /// <param name="msgLen">number of bytes to decrypt</param>
        public void DecryptBytes(
            byte[] inbuf,
            int inbufOfs,
            byte[] outbuf,
            int outbufOfs,
            int msgLen)
        {
            ProcessBytes(inbuf, inbufOfs, outbuf, outbufOfs, msgLen, 1);
        }

        void ProcessBytes(
            byte[] inbuf,
            int inbufOfs,
            byte[] outbuf,
            int outbufOfs,
            int msgLen,
            int mode)
        {
            uint j;
            uint tmp;
            uint[] buckets = _cacheProcessbytesBuckets;

#if !NO_Z_LOCALS
#endif

            var x0 = _ksX0;
            var x1 = _ksX1;
            var z = _csZ;
            var oldZ = _csOldZ;
            if (0 != (3 & _csMsgLen))
            {
                throw new PhelixException("data misalignment, only the last data junk " +
                                          $"can be off a {PhelixDataAlign}-byte border");
            }
            _csMsgLen += (ulong)msgLen;
            var i = _csI;
            _csZ[1] ^= _csAadXor;
            _csAadXor = 0;

#if !NO_Z_LOCALS
            var z0 = z[0];
            var z1 = z[1];
            var z2 = z[2];
            var z3 = z[3];
            var z4 = z[4];
#endif
            var leftOver = msgLen & 3;
            var endOfs = inbufOfs + (msgLen - (int)leftOver);

            for (; inbufOfs < endOfs; i++, inbufOfs += 4, outbufOfs += 4)
            {
                j = i & 7;

#if NO_Z_LOCALS
                Z[0] += Z[3]; Z[3] = (Z[3] << ROT_3b) | (Z[3] >> (32 - ROT_3b));
                Z[1] += Z[4]; Z[4] = (Z[4] << ROT_4b) | (Z[4] >> (32 - ROT_4b));
                Z[2] ^= Z[0]; Z[0] = (Z[0] << ROT_0a) | (Z[0] >> (32 - ROT_0a));
                Z[3] ^= Z[1]; Z[1] = (Z[1] << ROT_1a) | (Z[1] >> (32 - ROT_1a));
                Z[4] += Z[2]; Z[2] = (Z[2] << ROT_2a) | (Z[2] >> (32 - ROT_2a));

                Z[0] ^= Z[3] + X_0[j]; Z[3] = (Z[3] << ROT_3a) | (Z[3] >> (32 - ROT_3a));
                Z[1] ^= Z[4];          Z[4] = (Z[4] << ROT_4a) | (Z[4] >> (32 - ROT_4a));
                Z[2] += Z[0];          Z[0] = (Z[0] << ROT_0b) | (Z[0] >> (32 - ROT_0b));
                Z[3] += Z[1];          Z[1] = (Z[1] << ROT_1b) | (Z[1] >> (32 - ROT_1b));
                Z[4] ^= Z[2];          Z[2] = (Z[2] << ROT_2b) | (Z[2] >> (32 - ROT_2b));
#else
                z0 += z3; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));

                z0 ^= z3 + x0[j]; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));
#endif
                buckets[0] = tmp = inbuf[inbufOfs] |
                            ((uint)inbuf[inbufOfs + 1] << 8) |
                            ((uint)inbuf[inbufOfs + 2] << 16) |
                            ((uint)inbuf[inbufOfs + 3] << 24);

#if NO_Z_LOCALS
                tmp ^= Z[4] + oldZ[i & 3];
#else
                tmp ^= z4 + oldZ[i & 3];
#endif

                outbuf[outbufOfs] = (byte)tmp;
                outbuf[outbufOfs + 1] = (byte)(tmp >> 8);
                outbuf[outbufOfs + 2] = (byte)(tmp >> 16);
                outbuf[outbufOfs + 3] = (byte)(tmp >> 24);

                buckets[1] = tmp;

#if NO_Z_LOCALS
                Z[0] += Z[3] ^ buckets[mode]; Z[3] = (Z[3] << ROT_3b) | (Z[3] >> (32 - ROT_3b));
                Z[1] += Z[4];                 Z[4] = (Z[4] << ROT_4b) | (Z[4] >> (32 - ROT_4b));
                Z[2] ^= Z[0];                 Z[0] = (Z[0] << ROT_0a) | (Z[0] >> (32 - ROT_0a));
                Z[3] ^= Z[1];                 Z[1] = (Z[1] << ROT_1a) | (Z[1] >> (32 - ROT_1a));
                Z[4] += Z[2];                 Z[2] = (Z[2] << ROT_2a) | (Z[2] >> (32 - ROT_2a));

                Z[0] ^= Z[3] + X_1[j] + i; Z[3] = (Z[3] << ROT_3a) | (Z[3] >> (32 - ROT_3a));
                Z[1] ^= Z[4];              Z[4] = (Z[4] << ROT_4a) | (Z[4] >> (32 - ROT_4a));
                Z[2] += Z[0];              Z[0] = (Z[0] << ROT_0b) | (Z[0] >> (32 - ROT_0b));
                Z[3] += Z[1];              Z[1] = (Z[1] << ROT_1b) | (Z[1] >> (32 - ROT_1b));
                Z[4] ^= Z[2];              Z[2] = (Z[2] << ROT_2b) | (Z[2] >> (32 - ROT_2b));

                oldZ[i & 3] = Z[OLD_Z_REG];                
#else
                z0 += z3 ^ buckets[mode]; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));

                z0 ^= z3 + x1[j] + i; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));

                oldZ[i & 3] = z4; //Z[OLD_Z_REG];
#endif
            }

            // do an extra step for the leftover to keep the main loop clean and fast;
            // this is different from the C code: we won't go over the msglen size
            // passed by the caller, neither for reading (*) nor for writing (**)

            if (0 != leftOver)
            {
                j = i & 7;

#if NO_Z_LOCALS
                Z[0] += Z[3]; Z[3] = (Z[3] << ROT_3b) | (Z[3] >> (32 - ROT_3b));
                Z[1] += Z[4]; Z[4] = (Z[4] << ROT_4b) | (Z[4] >> (32 - ROT_4b));
                Z[2] ^= Z[0]; Z[0] = (Z[0] << ROT_0a) | (Z[0] >> (32 - ROT_0a));
                Z[3] ^= Z[1]; Z[1] = (Z[1] << ROT_1a) | (Z[1] >> (32 - ROT_1a));
                Z[4] += Z[2]; Z[2] = (Z[2] << ROT_2a) | (Z[2] >> (32 - ROT_2a));

                Z[0] ^= Z[3] + X_0[j]; Z[3] = (Z[3] << ROT_3a) | (Z[3] >> (32 - ROT_3a));
                Z[1] ^= Z[4];          Z[4] = (Z[4] << ROT_4a) | (Z[4] >> (32 - ROT_4a));
                Z[2] += Z[0];          Z[0] = (Z[0] << ROT_0b) | (Z[0] >> (32 - ROT_0b));
                Z[3] += Z[1];          Z[1] = (Z[1] << ROT_1b) | (Z[1] >> (32 - ROT_1b));
                Z[4] ^= Z[2];          Z[2] = (Z[2] << ROT_2b) | (Z[2] >> (32 - ROT_2b));
#else
                z0 += z3; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));

                z0 ^= z3 + x0[j]; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));
#endif
                // dynamic MASK_TAB replacement (*)

                uint ptxt = 0;
                tmp = (uint)leftOver << 3;
                int c;
                for (c = 0; c < tmp; c += 8)
                {
                    ptxt |= (uint)inbuf[inbufOfs++] << c;
                }

                buckets[0] = tmp = ptxt;

#if NO_Z_LOCALS
                tmp ^= Z[4] + oldZ[i & 3];
#else
                tmp ^= z4 + oldZ[i & 3];
#endif

                buckets[1] = tmp & MaskLeftover[leftOver];

                // write what we have to write (**)
                for (c = 0; c < leftOver; c++, tmp >>= 8, outbufOfs++)
                {
                    outbuf[outbufOfs] = (byte)tmp;
                }
#if NO_Z_LOCALS
                Z[0] += Z[3] ^ buckets[mode]; Z[3] = (Z[3] << ROT_3b) | (Z[3] >> (32 - ROT_3b));
                Z[1] += Z[4];                 Z[4] = (Z[4] << ROT_4b) | (Z[4] >> (32 - ROT_4b));
                Z[2] ^= Z[0];                 Z[0] = (Z[0] << ROT_0a) | (Z[0] >> (32 - ROT_0a));
                Z[3] ^= Z[1];                 Z[1] = (Z[1] << ROT_1a) | (Z[1] >> (32 - ROT_1a));
                Z[4] += Z[2];                 Z[2] = (Z[2] << ROT_2a) | (Z[2] >> (32 - ROT_2a));

                Z[0] ^= Z[3] + X_1[j] + i; Z[3] = (Z[3] << ROT_3a) | (Z[3] >> (32 - ROT_3a));
                Z[1] ^= Z[4];              Z[4] = (Z[4] << ROT_4a) | (Z[4] >> (32 - ROT_4a));
                Z[2] += Z[0];              Z[0] = (Z[0] << ROT_0b) | (Z[0] >> (32 - ROT_0b));
                Z[3] += Z[1];              Z[1] = (Z[1] << ROT_1b) | (Z[1] >> (32 - ROT_1b));
                Z[4] ^= Z[2];              Z[2] = (Z[2] << ROT_2b) | (Z[2] >> (32 - ROT_2b));

                oldZ[i & 3] = Z[OLD_Z_REG];
#else
                z0 += z3 ^ buckets[mode]; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));

                z0 ^= z3 + x1[j] + i; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));

                oldZ[i & 3] = z4; //Z[OLD_Z_REG];
#endif
                i++;
            }

#if !NO_Z_LOCALS
            z[0] = z0;
            z[1] = z1;
            z[2] = z2;
            z[3] = z3;
            z[4] = z4;
#endif

            this._csI = i;
        }

        /// <summary>
        /// Processes AAD (Additional Authentication Data). Can be called
        /// repeatedly and even mixed with regular encryption and decryption
        /// calls, with the exception that expect of the last portion each
        /// chunk of data must be aligned to a PHELIX_DATA_ALIGN border. 
        /// </summary>
        /// <param name="aad">buffer with the AAD</param>
        /// <param name="aadOfs">where the AAD starts</param>
        /// <param name="aadLen">size of the AAD in bytes</param>
        public void ProcessAAD(
            byte[] aad,
            int aadOfs,
            int aadLen)
        {
            var x0 = _ksX0;
            var x1 = _ksX1;
            var z = _csZ;
            var oldZ = _csOldZ;

            if (0 != (3 & _csAadLen))
            {
                throw new PhelixException("data misalignment for AAD, only the last data junk " +
                                          $"can be off a {PhelixDataAlign}-byte border");
            }

            _csAadLen += (ulong)aadLen;
            var i = _csI;

            var z0 = z[0];
            var z1 = z[1];
            var z2 = z[2];
            var z3 = z[3];
            var z4 = z[4];

            for (; 0 < aadLen; i++, aadOfs += 4)
            {
                var j = i & 7;

                z0 += z3; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));

                z0 ^= z3 + x0[j]; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));

                // here we are less performance-oriented than in ProcessBytes() for now,
                // if the real world requires absolute speed then we'll do in the future

                uint ptxt;
                if (4 <= aadLen)
                {
                    ptxt = aad[aadOfs] |
                          ((uint)aad[aadOfs + 1] << 8) |
                          ((uint)aad[aadOfs + 2] << 16) |
                          ((uint)aad[aadOfs + 3] << 24);
                    aadLen -= 4;
                }
                else
                {
                    ptxt = 0;   // (jtptc)

                    aadLen <<= 3;
                    int c;
                    for (c = 0; c < aadLen; c += 8)
                    {
                        ptxt |= (uint)aad[aadOfs++] << c;
                    }
                    aadLen = 0;
                }

                z0 += z3 ^ ptxt; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));

                z0 ^= z3 + x1[j] + i; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));

                oldZ[i & 3] = z4; //Z[OLD_Z_REG];
            }

            z[0] = z0;
            z[1] = z1;
            z[2] = z2;
            z[3] = z3;
            z[4] = z4;

            this._csI = i;
        }

        /// <summary>
        /// Computes the MAC at the end of an encryption or decryption. The size
        /// of the MAC is determined by what was passed during the call of
        /// SetupKey(). After the MAC is received it usually stored for encryption
        /// or compared against an existing one for decryption.
        /// </summary>
        /// <param name="mac">buffer to received the MAC data</param>
        /// <param name="macOfs">where to start writing the MAC data</param>
        public void Finalize(
            byte[] mac,
            int macOfs)
        {
            int end;
            uint k;
            uint[] tmp = _cacheFinalizeTmp;
            var x0 = _ksX0;
            var x1 = _ksX1;
            var z = _csZ;
            var oldZ = _csOldZ;

            var i = _csI;
            var ptxt = (uint)_csMsgLen & 3;

            var z0 = z[0];
            var z1 = z[1];
            var z2 = z[2];
            var z3 = z[3];
            var z4 = z[4];

            z0 ^= MacMagicXOR;
            z4 ^= (uint)_csAadLen;
            z2 ^= (uint)(_csAadLen >> 32);
            z1 ^= _csAadXor;

            for (k = 0; k < tmp.Length; k++, i++)
            {
                var j = i & 7;

                z0 += z3; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));

                z0 ^= z3 + x0[j]; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));

                tmp[k] = ptxt ^ (z4 + oldZ[i & 3]);

                z0 += z3 ^ ptxt; z3 = (z3 << Rot3B) | (z3 >> (32 - Rot3B));
                z1 += z4; z4 = (z4 << Rot4B) | (z4 >> (32 - Rot4B));
                z2 ^= z0; z0 = (z0 << Rot0A) | (z0 >> (32 - Rot0A));
                z3 ^= z1; z1 = (z1 << Rot1A) | (z1 >> (32 - Rot1A));
                z4 += z2; z2 = (z2 << Rot2A) | (z2 >> (32 - Rot2A));

                z0 ^= z3 + x1[j] + i; z3 = (z3 << Rot3A) | (z3 >> (32 - Rot3A));
                z1 ^= z4; z4 = (z4 << Rot4A) | (z4 >> (32 - Rot4A));
                z2 += z0; z0 = (z0 << Rot0B) | (z0 >> (32 - Rot0B));
                z3 += z1; z1 = (z1 << Rot1B) | (z1 >> (32 - Rot1B));
                z4 ^= z2; z2 = (z2 << Rot2B) | (z2 >> (32 - Rot2B));

                oldZ[i & 3] = z4; //Z[OLD_Z_REG];
            }

            var c = end = MacInitCnt;
            end += (96 == _ksMacSize) ? 3 : 4;

            while (c < end)
            {
                var t = tmp[c++];
                mac[macOfs++] = (byte)t;
                mac[macOfs++] = (byte)(t >> 8);
                mac[macOfs++] = (byte)(t >> 16);
                mac[macOfs++] = (byte)(t >> 24);
            }
        }

        /// <summary>
        /// Clears all sensitive internal fields. Recommended to be called as
        /// soon as an instance is not needed anymore.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_ksX0, 0, _ksX0.Length);
            Array.Clear(_ksX1, 0, _ksX1.Length);
            Array.Clear(_csOldZ, 0, _csOldZ.Length);
            Array.Clear(_csZ, 0, _csZ.Length);
        }

        #endregion
    }
}
