/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System.IO;
using System.IO.Compression;
using System.Text;

#endregion

namespace Highlander.Utilities.Compression
{
    /// <summary>
    /// A support class providing text compression to/from streams.
    /// </summary>
    public static class CompressionHelper
    {
        private static void Pump(Stream input, Stream output)
        {
            var bytes = new byte[4096];
            int n;
            while ((n = input.Read(bytes, 0, bytes.Length)) != 0)
            {
                output.Write(bytes, 0, n);
            }
        }

        private static void CompressStream(Stream source, Stream destination)
        {
            using (var zip = new GZipStream(destination, CompressionMode.Compress))
            {
                Pump(source, zip);
            }
        }

        private static void DecompressStream(Stream source, Stream destination)
        {
            using (var zip = new GZipStream(source, CompressionMode.Decompress))
            {
                Pump(zip, destination);
            }
        }

        /// <summary>
        /// Compresses a string via GZipStream to a buffer.
        /// </summary>
        /// <param name="dataText">The data text.</param>
        /// <returns></returns>
        public static byte[] CompressToBuffer(string dataText)
        {
            if (dataText == null)
                return null;
            byte[] inpBuffer = Encoding.UTF8.GetBytes(dataText);
            using (Stream inpStream = new MemoryStream(inpBuffer))
            using (var outStream = new MemoryStream())
            {
                CompressStream(inpStream, outStream);
                return outStream.ToArray();
            }
        }
        /// <summary>
        /// Decompresses a buffer via GZipStream to a string.
        /// </summary>
        /// <param name="dataBuffer">The data buffer.</param>
        /// <returns></returns>
        public static string DecompressToString(byte[] dataBuffer)
        {
            if (dataBuffer == null)
                return null;
            using (Stream inpStream = new MemoryStream(dataBuffer))
            using (var outStream = new MemoryStream())
            {
                DecompressStream(inpStream, outStream);
                byte[] outBuffer = outStream.ToArray();
                return Encoding.UTF8.GetString(outBuffer);
            }
        }
    }
}
