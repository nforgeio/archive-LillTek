﻿//-----------------------------------------------------------------------------
// FILE:        StreamEncryptor.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright (c) 2005-2015 by Jeffrey Lill.  All rights reserved.
// DESCRIPTION: Implements stream encryption using a symmetric encryption algorithm.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using LillTek.Common;

namespace LillTek.Cryptography
{
    /// <summary>
    /// Implements stream encryption using a symmetric encryption algorithm.
    /// </summary>
    /// <threadsafety instance="false" />
    public sealed class StreamEncryptor : IDisposable
    {
        private string              algorithm;
        private byte[]              key;
        private byte[]              IV;
        private SymmetricAlgorithm  provider;
        private ICryptoTransform    encryptor;

        /// <summary>
        /// Initializes the encryptor to use the specified encryption algorithm,
        /// key, and initialization vector.
        /// </summary>
        /// <param name="algorithm">The symmetric algorithm name.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="IV">The initialization vector.</param>
        public StreamEncryptor(string algorithm, byte[] key, byte[] IV)
        {
            this.algorithm    = algorithm;
            this.key          = key;
            this.IV           = IV;

            this.provider     = EncryptionConfig.CreateSymmetric(algorithm);
            this.provider.Key = key;
            this.provider.IV  = IV;

            this.encryptor    = null;
        }

        /// <summary>
        /// Initializes the encryptor to use the specified 
        /// <see cref="SymmetricKey" />.
        /// </summary>
        /// <param name="symmetricKey">The symmetric key.</param>
        public StreamEncryptor(SymmetricKey symmetricKey)
            : this(symmetricKey.Algorithm, symmetricKey.Key, symmetricKey.IV)
        {
        }

        /// <summary>
        /// Closes the encryptor, proactively releasing any associated unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (provider != null)
            {
                provider.Clear();
                provider = null;
            }

            if (encryptor != null)
            {
                encryptor.Dispose();
                encryptor = null;
            }
        }

        /// <summary>
        /// Encrypts the contents of the <paramref name="input" /> stream, from the current stream position
        /// to the end of the stream, and writes the encrypted data to the <paramref name="output" /> stream.
        /// </summary>
        /// <param name="input">The decrypted input stream.</param>
        /// <param name="output">The encrypted output stream.</param>
        /// <exception cref="ArgumentNullException">Thrown if either of <paramref name="input" /> or <paramref name="output"/> are <c>null</c>.</exception>
        /// <exception cref="IOException">Thrown for I/O errors.</exception>
        /// <exception cref="CryptographicException">Thrown for cryptographic related problems.</exception>
        public void Encrypt(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (output == null)
                throw new ArgumentNullException("output");

            if (encryptor == null)
                encryptor = provider.CreateEncryptor();

            var cs = new CryptoStream(output, encryptor, CryptoStreamMode.Write);

            try
            {
                var buffer = new byte[Crypto.StreamBufferSize];
                var cb = 0;

                while (true)
                {
                    cb = input.Read(buffer, 0, buffer.Length);
                    if (cb == 0)
                        break;

                    cs.Write(buffer, 0, cb);
                }

                cs.FlushFinalBlock();
            }
            finally
            {
                if (!encryptor.CanReuseTransform)
                {
                    encryptor.Dispose();
                    encryptor = null;
                }
            }
        }

        /// <summary>
        /// Encrypts <paramref name="count" /> bytes of the <paramref name="input" /> stream, from the current stream position
        /// and writes the encrypted data to the <paramref name="output" /> stream.
        /// </summary>
        /// <param name="input">The decrypted input stream.</param>
        /// <param name="output">The encrypted output stream.</param>
        /// <param name="count">The number of bytes to be encrypted.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="count" /> is negative.</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown if the end of the stream has been reached before <paramref name="count" /> bytes have been read.</exception>
        /// <exception cref="IOException">Thrown for I/O errors.</exception>
        /// <exception cref="CryptographicException">Thrown for cryptographic related problems.</exception>
        public void Encrypt(Stream input, Stream output, int count)
        {
            if (count < 0)
                throw new ArgumentException(string.Format("Cannot encrypt [count=[{0}] bytes.", count), "count");

            if (encryptor == null)
                encryptor = provider.CreateEncryptor();

            var cs = new CryptoStream(output, encryptor, CryptoStreamMode.Write);

            try
            {
                var buffer      = new byte[Crypto.StreamBufferSize];
                var cbRemaining = count;
                var cb          = 0;

                while (cbRemaining > 0)
                {
                    cb = input.Read(buffer, 0, Math.Min(buffer.Length, cbRemaining));
                    if (cb == 0)
                        break;

                    cs.Write(buffer, 0, cb);
                    cbRemaining -= cb;
                }

                cs.FlushFinalBlock();

                if (cbRemaining != 0)
                    throw new IndexOutOfRangeException(string.Format("Stream encryption failed because [{0}] bytes were to be processed but only [{1}] could be read from the input stream.", count, count - cbRemaining));
            }
            finally
            {
                if (!encryptor.CanReuseTransform)
                {
                    encryptor.Dispose();
                    encryptor = null;
                }
            }
        }

        /// <summary>
        /// Encrypts a file by name, creating or overwriting another file with the encrypted output.
        /// </summary>
        /// <param name="inputPath">The path to the input (decrypted) file.</param>
        /// <param name="outputPath">The path to the output (encrypted) file.</param>
        /// <exception cref="ArgumentNullException">Thrown if either of <paramref name="inputPath" /> or <paramref name="outputPath"/> are <c>null</c>.</exception>
        /// <exception cref="IOException">Thrown for I/O errors.</exception>
        /// <exception cref="CryptographicException">Thrown for cryptographic related problems.</exception>
        public void Encrypt(string inputPath, string outputPath)
        {
            if (inputPath == null)
                throw new ArgumentNullException("inputPath");

            if (outputPath == null)
                throw new ArgumentNullException("outputPath");

            FileStream input = null;
            FileStream output = null;

            try
            {
                input  = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
                output = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite);

                Encrypt(input, output);
            }
            finally
            {
                if (input != null)
                    input.Close();

                if (output != null)
                    output.Close();
            }
        }
    }
}