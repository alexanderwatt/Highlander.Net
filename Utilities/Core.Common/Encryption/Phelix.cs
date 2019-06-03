
// Phelix.NET - Copyright 2006 Markus Hahn - All rights reserved.

// experimental switch to see the performance improvement of local Z[] caching
// (measured to gain 25% speed) - NOT RECOMMENED TO USE EXCEPT FOR SUCH TESTS!
//#define NO_Z_LOCALS

/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)
 Copyright (C) 2019 Simon Dudley

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace Core.Common.Encryption
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
        public const int PHELIX_NONCE_SIZE = 128;
        /// <summary>
        /// Size of a standard Phelix MAC (128bit) in bits.
        /// </summary>
        public const int PHELIX_MAC_SIZE = 128;
        /// <summary>
        /// Size of a compact Phelix MAC (96bit) in bits.
        /// </summary>
        public const int PHELIX_MAC_SIZE_96 = 96;
        /// <summary>
        /// Maximum size of a key for Phelix in bits.
        /// </summary>
        public const int PHELIX_KEY_SIZE = 256;
        /// <summary>
        /// Block size (in bytes) for continous data passed for encryption and
        /// decryption.
        /// </summary>
        public const int PHELIX_DATA_ALIGN = 4;

        #endregion

        #region Private Constants

        const int OLD_Z_REG = 4;
        const int ZERO_INIT_CNT = 8;
        const int MAC_INIT_CNT = 8;
        const int MAC_WORD_CNT = PHELIX_MAC_SIZE / 32;
        const int ROT_0a = 9;
        const int ROT_1a = 10;
        const int ROT_2a = 17;
        const int ROT_3a = 30;
        const int ROT_4a = 13;
        const int ROT_0b = 20;
        const int ROT_1b = 11;
        const int ROT_2b = 5;
        const int ROT_3b = 15;
        const int ROT_4b = 25;
        const uint MAC_MAGIC_XOR = 0x912d94f1;
        const uint AAD_MAGIC_XOR = 0xaadaadaa;

        static uint[] MASK_LEFTOVER = { 0, 0x00ff, 0x00ffff, 0x00ffffff };

        #endregion

        #region State

        // key setup state
        uint ks_macSize;
        uint ks_X_1_bump;
        uint[] ks_X_0 = new uint[8];
        uint[] ks_X_1 = new uint[8];
        // cipher state
        uint[] cs_oldZ = new uint[4];
        uint[] cs_Z = new uint[5];
        uint cs_i;
        ulong cs_aadLen;
        ulong cs_msgLen;
        uint cs_aadXor;

        #endregion

        #region Local Caches

        uint[] cache_finalize_tmp = new uint[MAC_INIT_CNT + MAC_WORD_CNT];
        uint[] cache_processbytes_buckets = new uint[2];

        #endregion

        /// <summary>
        /// Default ctor. Notice that a new instance is not usable without
        /// consecutive and proper setup calls!
        /// </summary>
        public Phelix()
        {
            // this is a matter of discussion: should Init() always be called explicitely?
            Init();
        }

        #region API

        /// <summary>
        /// Resets the internal state. To be called before the actual setup.
        /// </summary>
        public void Init()
        {
            cs_i = 0;
            cs_msgLen = 0;
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
            uint[] X = ks_X_0;
            if (PHELIX_KEY_SIZE < keySize || keySize < 0)
            {
                throw new PhelixException($"invalid key size {keySize}");
            }
            if (0 != (keySize & 7))
            {
                throw new PhelixException("key must be byte-sized");
            }
            ks_macSize = (uint)macSize;
            ks_X_1_bump = (uint)((keySize >> 1) + ((macSize % PHELIX_MAC_SIZE) << 8));
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
                var Z_0 = X[k];
                var Z_1 = X[k + 1];
                var Z_2 = X[k + 2];
                var Z_3 = X[k + 3];
                var Z_4 = (uint)tmp;
                // check if the compiler creates the constants!
                int rep;
                for (rep = 0; rep < 2; rep++)
                {
                    Z_0 += Z_3; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                    Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                    Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                    Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                    Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));
                    Z_0 ^= Z_3; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                    Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                    Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                    Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                    Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));
                }
                k = (k + 4) & 7;
                X[k] ^= Z_0;
                X[k + 1] ^= Z_1;
                X[k + 2] ^= Z_2;
                X[k + 3] ^= Z_3;
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
            var X_0 = ks_X_0;
            var X_1 = ks_X_1;
            var X_1_bump = ks_X_1_bump;
            var Z = cs_Z;
            var oldZ = cs_oldZ;
            for (i = 0; i < 4; i++, nonceOfs += 4)
            {
                var n = nonce[nonceOfs] |
                         ((uint)nonce[nonceOfs + 1] << 8) |
                         ((uint)nonce[nonceOfs + 2] << 16) |
                         ((uint)nonce[nonceOfs + 3] << 24);
                X_1[i] = X_0[i + 4] + n;
                X_1[i + 4] = X_0[i] + (i - n);
                Z[i] = X_0[i + 3] ^ n;
            }
            X_1[1] += X_1_bump;
            X_1[5] += X_1_bump;
            Z[4] = X_0[7];
            cs_aadLen = 0L;
            cs_msgLen = 0L;
            var Z_0 = Z[0];
            var Z_1 = Z[1];
            var Z_2 = Z[2];
            var Z_3 = Z[3];
            var Z_4 = Z[4];
            for (i = 0; i < 8; i++)
            {
                var j = i & 7;
                Z_0 += Z_3; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));
                Z_0 ^= Z_3 + X_0[j]; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));
                Z_0 += Z_3; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));
                Z_0 ^= Z_3 + X_1[j] + i; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));
                oldZ[i & 3] = Z_4; //Z[OLD_Z_REG];
            }
            Z[0] = Z_0;
            Z[1] = Z_1;
            Z[2] = Z_2;
            Z[3] = Z_3;
            Z[4] = Z_4;
            Z[1] ^= (cs_aadXor = AAD_MAGIC_XOR);
            cs_i = i;
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
            int leftOver, endOfs, c;
            uint i, j, ptxt, tmp;
            uint[] buckets = this.cache_processbytes_buckets;
            uint[] X_0, X_1, oldZ, Z;

#if !NO_Z_LOCALS
            uint Z_0, Z_1, Z_2, Z_3, Z_4;
#endif

            X_0 = ks_X_0;
            X_1 = ks_X_1;
            Z = cs_Z;
            oldZ = cs_oldZ;
            if (0 != (3 & cs_msgLen))
            {
                throw new PhelixException("data misalignment, only the last data junk " +
                                          $"can be off a {PHELIX_DATA_ALIGN}-byte border");
            }
            cs_msgLen += (ulong)msgLen;
            i = cs_i;
            cs_Z[1] ^= cs_aadXor;
            cs_aadXor = 0;

#if !NO_Z_LOCALS
            Z_0 = Z[0];
            Z_1 = Z[1];
            Z_2 = Z[2];
            Z_3 = Z[3];
            Z_4 = Z[4];
#endif
            leftOver = msgLen & 3;
            endOfs = inbufOfs + (msgLen - (int)leftOver);

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
                Z_0 += Z_3; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));

                Z_0 ^= Z_3 + X_0[j]; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));
#endif
                buckets[0] = tmp = inbuf[inbufOfs] |
                            ((uint)inbuf[inbufOfs + 1] << 8) |
                            ((uint)inbuf[inbufOfs + 2] << 16) |
                            ((uint)inbuf[inbufOfs + 3] << 24);

#if NO_Z_LOCALS
                tmp ^= Z[4] + oldZ[i & 3];
#else
                tmp ^= Z_4 + oldZ[i & 3];
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
                Z_0 += Z_3 ^ buckets[mode]; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));

                Z_0 ^= Z_3 + X_1[j] + i; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));

                oldZ[i & 3] = Z_4; //Z[OLD_Z_REG];
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
                Z_0 += Z_3; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));

                Z_0 ^= Z_3 + X_0[j]; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));
#endif
                // dynamic MASK_TAB replacement (*)

                ptxt = 0;   // (jtptc)
                tmp = (uint)leftOver << 3;
                for (c = 0; c < tmp; c += 8)
                {
                    ptxt |= (uint)inbuf[inbufOfs++] << c;
                }

                buckets[0] = tmp = ptxt;

#if NO_Z_LOCALS
                tmp ^= Z[4] + oldZ[i & 3];
#else
                tmp ^= Z_4 + oldZ[i & 3];
#endif

                buckets[1] = tmp & MASK_LEFTOVER[leftOver];

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
                Z_0 += Z_3 ^ buckets[mode]; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));

                Z_0 ^= Z_3 + X_1[j] + i; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));

                oldZ[i & 3] = Z_4; //Z[OLD_Z_REG];
#endif
                i++;
            }

#if !NO_Z_LOCALS
            Z[0] = Z_0;
            Z[1] = Z_1;
            Z[2] = Z_2;
            Z[3] = Z_3;
            Z[4] = Z_4;
#endif

            this.cs_i = i;
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
            int c;
            uint i, j, ptxt;
            uint[] X_0, X_1, oldZ, Z;
            uint Z_0, Z_1, Z_2, Z_3, Z_4;


            X_0 = this.ks_X_0;
            X_1 = this.ks_X_1;
            Z = this.cs_Z;
            oldZ = this.cs_oldZ;

            if (0 != (3 & this.cs_aadLen))
            {
                throw new PhelixException(String.Format("data misalignment for AAD, only the last data junk " +
                    "can be off a {0}-byte border", PHELIX_DATA_ALIGN));
            }

            this.cs_aadLen += (ulong)aadLen;
            i = this.cs_i;

            Z_0 = Z[0];
            Z_1 = Z[1];
            Z_2 = Z[2];
            Z_3 = Z[3];
            Z_4 = Z[4];

            for (; 0 < aadLen; i++, aadOfs += 4)
            {
                j = i & 7;

                Z_0 += Z_3; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));

                Z_0 ^= Z_3 + X_0[j]; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));

                // here we are less performance-oriented than in ProcessBytes() for now,
                // if the real world requires absolute speed then we'll do in the future

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
                    for (c = 0; c < aadLen; c += 8)
                    {
                        ptxt |= (uint)aad[aadOfs++] << c;
                    }
                    aadLen = 0;
                }

                Z_0 += Z_3 ^ ptxt; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));

                Z_0 ^= Z_3 + X_1[j] + i; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));

                oldZ[i & 3] = Z_4; //Z[OLD_Z_REG];
            }

            Z[0] = Z_0;
            Z[1] = Z_1;
            Z[2] = Z_2;
            Z[3] = Z_3;
            Z[4] = Z_4;

            this.cs_i = i;
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
            int c, end;
            uint i, j, k, t, ptxt;
            uint[] Z, X_0, X_1, oldZ;
            uint Z_0, Z_1, Z_2, Z_3, Z_4;
            uint[] tmp = this.cache_finalize_tmp;


            X_0 = this.ks_X_0;
            X_1 = this.ks_X_1;
            Z = this.cs_Z;
            oldZ = this.cs_oldZ;

            i = this.cs_i;
            ptxt = (uint)this.cs_msgLen & 3;

            Z_0 = Z[0];
            Z_1 = Z[1];
            Z_2 = Z[2];
            Z_3 = Z[3];
            Z_4 = Z[4];

            Z_0 ^= MAC_MAGIC_XOR;
            Z_4 ^= (uint)this.cs_aadLen;
            Z_2 ^= (uint)(this.cs_aadLen >> 32);
            Z_1 ^= this.cs_aadXor;

            for (k = 0; k < tmp.Length; k++, i++)
            {
                j = i & 7;

                Z_0 += Z_3; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));

                Z_0 ^= Z_3 + X_0[j]; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));

                tmp[k] = ptxt ^ (Z_4 + oldZ[i & 3]);

                Z_0 += Z_3 ^ ptxt; Z_3 = (Z_3 << ROT_3b) | (Z_3 >> (32 - ROT_3b));
                Z_1 += Z_4; Z_4 = (Z_4 << ROT_4b) | (Z_4 >> (32 - ROT_4b));
                Z_2 ^= Z_0; Z_0 = (Z_0 << ROT_0a) | (Z_0 >> (32 - ROT_0a));
                Z_3 ^= Z_1; Z_1 = (Z_1 << ROT_1a) | (Z_1 >> (32 - ROT_1a));
                Z_4 += Z_2; Z_2 = (Z_2 << ROT_2a) | (Z_2 >> (32 - ROT_2a));

                Z_0 ^= Z_3 + X_1[j] + i; Z_3 = (Z_3 << ROT_3a) | (Z_3 >> (32 - ROT_3a));
                Z_1 ^= Z_4; Z_4 = (Z_4 << ROT_4a) | (Z_4 >> (32 - ROT_4a));
                Z_2 += Z_0; Z_0 = (Z_0 << ROT_0b) | (Z_0 >> (32 - ROT_0b));
                Z_3 += Z_1; Z_1 = (Z_1 << ROT_1b) | (Z_1 >> (32 - ROT_1b));
                Z_4 ^= Z_2; Z_2 = (Z_2 << ROT_2b) | (Z_2 >> (32 - ROT_2b));

                oldZ[i & 3] = Z_4; //Z[OLD_Z_REG];
            }

            c = end = MAC_INIT_CNT;
            end += (96 == this.ks_macSize) ? 3 : 4;

            while (c < end)
            {
                t = tmp[c++];
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
            Array.Clear(ks_X_0, 0, ks_X_0.Length);
            Array.Clear(ks_X_1, 0, ks_X_1.Length);
            Array.Clear(cs_oldZ, 0, cs_oldZ.Length);
            Array.Clear(cs_Z, 0, cs_Z.Length);
        }

        #endregion
    }
}
