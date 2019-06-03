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

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;

#endregion

namespace Core.Common.Encryption
{
    internal class KeyTriple
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 
        /// </summary>
        public int Alg { get; }

        /// <summary>
        /// 
        /// </summary>
        public string TranspKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="alg"></param>
        /// <param name="transpKey"></param>
        /// <param name="publicKey"></param>
        /// <param name="secretKey"></param>
        public KeyTriple(string id, int alg, string transpKey, string publicKey, string secretKey)
        {
            // construct new session, public and secret keys
            Id = id;
            Alg = alg;
            TranspKey = transpKey;
            PublicKey = publicKey;
            SecretKey = secretKey;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICryptoManager
    {
        string GenerateNewKeys();
        string GetTranspKey(string id);
        void SetTranspKey(string id, string transpKey);
        string GetPublicKey(string id);
        void SetPublicKey(string id, string publicKey);
        byte[] DecryptWithTranspKey(string id, byte[] buffer);
        byte[] EncryptWithTranspKey(string id, byte[] buffer);
        byte[] DecryptWithSecretKey(string id, byte[] buffer);
        byte[] EncryptWithPublicKey(string id, byte[] buffer);
        bool VerifySignature(string id, byte[] dataBuffer, byte[] signBuffer);
        byte[] CreateSignature(string id, byte[] dataBuffer);
    }

    /// <summary>
    /// 
    /// </summary>
    public class DefaultCryptoManager : ICryptoManager
    {
        private readonly IDictionary<string, KeyTriple> _keyCache;

        /// <summary>
        /// 
        /// </summary>
        public DefaultCryptoManager()
        {
            // default settings
            _keyCache = new Dictionary<string, KeyTriple>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GenerateNewKeys()
        {
            RSACryptoServiceProvider RSAProvider = new RSACryptoServiceProvider();
            KeyTriple keys = new KeyTriple(
                Guid.NewGuid().ToString(),
                1,
                Guid.NewGuid().ToString(),
                RSAProvider.ToXmlString(false),
                RSAProvider.ToXmlString(true));
            _keyCache.Add(keys.Id, keys);
            return keys.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTranspKey(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (_keyCache.TryGetValue(id, out var keys))
                return keys.TranspKey;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="transpKey"></param>
        public void SetTranspKey(string id, string transpKey)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            KeyTriple keys;
            if (!_keyCache.TryGetValue(id, out keys))
            {
                keys = new KeyTriple(id, 1, null, null, null);
                _keyCache.Add(keys.Id, keys);
            }
            keys.TranspKey = transpKey;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetPublicKey(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            KeyTriple keys;
            if (_keyCache.TryGetValue(id, out keys))
                return keys.PublicKey;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="publicKey"></param>
        public void SetPublicKey(string id, string publicKey)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            KeyTriple keys;
            if (!_keyCache.TryGetValue(id, out keys))
            {
                keys = new KeyTriple(id, 1, null, null, null);
                _keyCache.Add(keys.Id, keys);
            }
            keys.PublicKey = publicKey;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetSecretKey(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            KeyTriple keys;
            if (_keyCache.TryGetValue(id, out keys))
                return keys.SecretKey;
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="secretKey"></param>
        public void SetSecretKey(string id, string secretKey)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (!_keyCache.TryGetValue(id, out var keys))
            {
                keys = new KeyTriple(id, 1, null, null, null);
                _keyCache.Add(keys.Id, keys);
            }
            keys.SecretKey = secretKey;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public byte[] EncryptWithTranspKey(string id, byte[] buffer)
        {
            if (buffer == null)
                return null;
            if (!_keyCache.TryGetValue(id, out var keys))
                throw new ArgumentException("Invalid key id", nameof(id));
            if (keys.TranspKey == null)
                throw new ArgumentException("Transport key not set for key id: '" + id + "'");
            switch (keys.Alg)
            {
                case 1: // Phelix
                    PhelixSession phx = new PhelixSession(keys.TranspKey);
                    return phx.EncryptBuffer(buffer);
                default:
                    throw new ArgumentException("Unknown alg: " + keys.Alg.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public byte[] DecryptWithTranspKey(string id, byte[] buffer)
        {
            if (buffer == null)
                return null;
            KeyTriple keys;
            if (!_keyCache.TryGetValue(id, out keys))
                throw new ArgumentException("Invalid key id", nameof(id));
            if (keys.TranspKey == null)
                throw new ArgumentException("Transport key not set for key id: '" + id + "'");
            switch (keys.Alg)
            {
                case 1: // Phelix
                    PhelixSession phx = new PhelixSession(keys.TranspKey);
                    return phx.DecryptBuffer(buffer);
                default:
                    throw new ArgumentException("Unknown alg: " + keys.Alg.ToString());
            }
        }

        private const int RSAEncryptBlockSize = 32;
        private const int RSADecryptBlockSize = 128;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretKey"></param>
        /// <param name="inpBuffer"></param>
        /// <returns></returns>
        private static byte[] RSABinaryDecrypt(string secretKey, byte[] inpBuffer)
        {
            RSACryptoServiceProvider RSAProvider = new RSACryptoServiceProvider();
            RSAProvider.FromXmlString(secretKey);
            int nBlocks = (inpBuffer.Length / RSADecryptBlockSize);
            if ((nBlocks * RSADecryptBlockSize) != inpBuffer.Length)
                throw new ArgumentException("Input buffer not multiple of block size!");
            byte[][] inpBlocks = new byte[nBlocks][];
            byte[][] outBlocks = new byte[nBlocks][];
            int srcOffset = 0;
            for (int i = 1; i <= nBlocks; i++)
            {
                if (i == nBlocks)
                {
                    int len = inpBuffer.Length - srcOffset;
                    byte[] buf = new byte[len];
                    Array.Copy(inpBuffer, srcOffset, buf, 0, len);
                    inpBlocks[i - 1] = buf;
                    srcOffset += len;
                }
                else
                {
                    int len = RSADecryptBlockSize;
                    byte[] buf = new byte[len];
                    Array.Copy(inpBuffer, srcOffset, buf, 0, len);
                    inpBlocks[i - 1] = buf;
                    srcOffset += len;
                }
            }
            int outLength = 0;
            for (int i = 0; i < nBlocks; i++)
            {
                outBlocks[i] = RSAProvider.Decrypt(inpBlocks[i], true);
                outLength += outBlocks[i].Length;
            }
            byte[] outBuffer = new byte[outLength];
            int dstOffset = 0;
            for (int i = 0; i < nBlocks; i++)
            {
                int len = outBlocks[i].Length;
                Array.Copy(outBlocks[i], 0, outBuffer, dstOffset, len);
                dstOffset += len;
            }
            return outBuffer;
        }

        private static byte[] RSABinaryEncrypt(string publicKey, byte[] inpBuffer)
        {
            RSACryptoServiceProvider RSAProvider = new RSACryptoServiceProvider();
            RSAProvider.FromXmlString(publicKey);
            int nBlocks = (inpBuffer.Length / RSAEncryptBlockSize) + 1;
            byte[][] inpBlocks = new byte[nBlocks][];
            byte[][] outBlocks = new byte[nBlocks][];
            int srcOffset = 0;
            for (int i = 1; i <= nBlocks; i++)
            {
                if (i == nBlocks)
                {
                    int len = inpBuffer.Length - srcOffset;
                    byte[] buf = new byte[len];
                    Array.Copy(inpBuffer, srcOffset, buf, 0, len);
                    inpBlocks[i - 1] = buf;
                    srcOffset += len;
                }
                else
                {
                    int len = RSAEncryptBlockSize;
                    byte[] buf = new byte[len];
                    Array.Copy(inpBuffer, srcOffset, buf, 0, len);
                    inpBlocks[i - 1] = buf;
                    srcOffset += len;
                }
            }
            int outLength = 0;
            for (int i = 0; i < nBlocks; i++)
            {
                outBlocks[i] = RSAProvider.Encrypt(inpBlocks[i], true);
                outLength += outBlocks[i].Length;
            }
            byte[] outBuffer = new byte[outLength];
            int dstOffset = 0;
            for (int i = 0; i < nBlocks; i++)
            {
                int len = outBlocks[i].Length;
                Array.Copy(outBlocks[i], 0, outBuffer, dstOffset, len);
                dstOffset += len;
            }
            return outBuffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public byte[] DecryptWithSecretKey(string id, byte[] buffer)
        {
            if (buffer == null)
                return null;
            if (!_keyCache.TryGetValue(id, out var keys))
                throw new ArgumentException("Invalid key id", nameof(id));
            if (keys.SecretKey == null)
                throw new ArgumentException("Secret key not set for key id: '" + id + "'");
            switch (keys.Alg)
            {
                case 1: // RSA
                    return RSABinaryDecrypt(keys.SecretKey, buffer);
                default:
                    throw new ArgumentException("Unknown alg: " + keys.Alg.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public byte[] EncryptWithPublicKey(string id, byte[] buffer)
        {
            if (buffer == null)
                return null;
            if (!_keyCache.TryGetValue(id, out var keys))
                throw new ArgumentException("Invalid key id", nameof(id));
            if (keys.PublicKey == null)
                throw new ArgumentException("Public key not set for key id: '" + id + "'");
            switch (keys.Alg)
            {
                case 1: // RSA
                    return RSABinaryEncrypt(keys.PublicKey, buffer);
                default:
                    throw new ArgumentException("Unknown alg: " + keys.Alg.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataBuffer"></param>
        /// <param name="signBuffer"></param>
        /// <returns></returns>
        public bool VerifySignature(string id, byte[] dataBuffer, byte[] signBuffer)
        {
            if (!_keyCache.TryGetValue(id, out var keys))
                throw new ArgumentException("Invalid key id", nameof(id));
            if (keys.PublicKey == null)
                throw new ArgumentException("Public key not set for key id: '" + id + "'");
            switch (keys.Alg)
            {
                case 1: // RSA
                    RSACryptoServiceProvider RSAProvider = new RSACryptoServiceProvider();
                    RSAProvider.FromXmlString(keys.PublicKey);
                    return RSAProvider.VerifyData(dataBuffer, new SHA1CryptoServiceProvider(), signBuffer);
                default:
                    throw new ArgumentException("Unknown alg: " + keys.Alg.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public byte[] CreateSignature(string id, byte[] buffer)
        {
            if (buffer == null)
                return null;
            if (!_keyCache.TryGetValue(id, out var keys))
                throw new ArgumentException("Invalid key id", nameof(id));
            if (keys.SecretKey == null)
                throw new ArgumentException("Secret key not set for key id: '" + id + "'");
            switch (keys.Alg)
            {
                case 1: // RSA
                    RSACryptoServiceProvider RSAProvider = new RSACryptoServiceProvider();
                    RSAProvider.FromXmlString(keys.SecretKey);
                    return RSAProvider.SignData(buffer, new SHA1CryptoServiceProvider());
                default:
                    throw new ArgumentException("Unknown alg: " + keys.Alg.ToString());
            }
        }
    }

    /// <summary>
    /// Transient string encryption (and decryption) using the Phelix algorithm.
    /// Strings are converted to UTF-8, the binary representation is encrypted,
    /// the nonce is prepended and the 96bit MAC appended. The whole construct
    /// then is BASE64 encoded to form the final representation. Even very
    /// large text data can be encrypted, but notice that memory consumption is
    /// sometimes three times the size of the actual string. A regular
    /// password string's UTF-8 presentation gets hashed using SHA-1 to gain
    /// 160 bits of key material. The extraordinary advantage of this class is that
    /// data integrity is automatically guaranteed, no additional checking is
    /// needed (usually done by some kind of check summing). Notice that an instance
    /// is not thread safe!
    /// </summary>
    public class PhelixSession
    {
        Phelix phx;
        byte[] key, mac;
        RandomNumberGenerator rng;
        UTF8Encoding enc;

        /// <summary>
        /// Default ctor.
        /// </summary>
        /// <param name="sessionKey">the password to use for encryption and decryption</param>
        public PhelixSession(string sessionKey)
        {
            enc = new UTF8Encoding(false, true);
            key = (new SHA1CryptoServiceProvider()).ComputeHash(enc.GetBytes(sessionKey));
            phx = new Phelix();
            rng = new RNGCryptoServiceProvider();
            mac = new byte[Phelix.PHELIX_MAC_SIZE_96 / 8];
        }

        /// <summary>
        /// Encrypt a string.
        /// </summary>
        /// <param name="plaintext">the plaintext to encrypt</param>
        /// <returns>the cipher text (always larger than the plaintext)</returns>
        public String EncryptString(string plaintext)
        {
            byte[] plainBuffer = enc.GetBytes(plaintext);
            byte[] cipherBuffer = EncryptBuffer(plainBuffer);
            return Convert.ToBase64String(cipherBuffer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plainBuffer"></param>
        /// <returns></returns>
        public byte[] EncryptBuffer(byte[] plainBuffer)
        {
            MemoryStream ms = new MemoryStream();
            // create and put out the nonce
            var nonce = new byte[Phelix.PHELIX_NONCE_SIZE / 8];
            rng.GetBytes(nonce);
            ms.Write(nonce, 0, nonce.Length);
            // set up the cipher
            phx.Init();
            phx.SetupKey(key, 0, key.Length * 8, mac.Length * 8);
            phx.SetupNonce(nonce, 0);
            var ctxt = new byte[plainBuffer.Length];
            // encrypt and store the whole string
            phx.EncryptBytes(plainBuffer, 0, ctxt, 0, ctxt.Length);
            ms.Write(ctxt, 0, ctxt.Length);
            // calculate the 128bit MAC and append it
            phx.Finalize(mac, 0);
            ms.Write(mac, 0, mac.Length);
            byte[] returnBuffer = ms.ToArray();
            return returnBuffer;
        }

        const int OVERHEAD_SIZE_BYTES = (Phelix.PHELIX_NONCE_SIZE + Phelix.PHELIX_MAC_SIZE_96) / 8;

        /// <summary>
        /// Decrypt a string which was formerly encrypted with the same password.
        /// If decryption fails or the data is corrupt a PhelixException is thrown.
        /// </summary>
        /// <param name="sCipherText">the cipher text to decrypt</param>
        /// <returns>the plaintext (always smaller than the cipher text), with a
        /// very, very high probability the data is of the original form</returns>
        public String DecryptString(string sCipherText)
        {
            byte[] cipherBuffer;
            // get all of the binary data
            try
            {
                cipherBuffer = Convert.FromBase64String(sCipherText);
            }
            catch (FormatException fex)
            {
                throw new PhelixException(fex.Message);
            }
            byte[] plainBuffer = DecryptBuffer(cipherBuffer);
            return new String(enc.GetChars(plainBuffer));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cipherBuffer"></param>
        /// <returns></returns>
        public byte[] DecryptBuffer(byte[] cipherBuffer)
        {
            int nI;
            // minimum size?
            if (OVERHEAD_SIZE_BYTES > cipherBuffer.Length)
            {
                throw new PhelixException("data size below the minimum");
            }
            // set up the cipher
            phx.Init();
            phx.SetupKey(key, 0, this.key.Length * 8, mac.Length * 8);
            phx.SetupNonce(cipherBuffer, 0);
            // decrypt and decode the string
            var ptxt = new byte[cipherBuffer.Length - OVERHEAD_SIZE_BYTES];
            phx.DecryptBytes(cipherBuffer, Phelix.PHELIX_NONCE_SIZE / 8, ptxt, 0, ptxt.Length);
            // check the MAC
            phx.Finalize(mac, 0);
            var nOfs = Phelix.PHELIX_NONCE_SIZE / 8 + ptxt.Length;
            for (nI = 0; nI < mac.Length; nI++, nOfs++)
            {
                if (mac[nI] != cipherBuffer[nOfs])
                {
                    throw new PhelixException($"MAC mismatch at position {nI}");
                }
            }
            return ptxt;
        }

        /// <summary>
        /// Clears sensitive internal data. Recommended to be called if an instance's
        /// not needed anymore. After the call the instance must not be used again!
        /// </summary>
        public void Clear()
        {
            phx.Clear();
        }
    }
}
