// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp.Web.Caching;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class HexEncoderTests
    {
        [Fact]
        public void HexEncoderOutputIsCorrect()
        {
            byte[] hash = Hash();
            var sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }

            string expected = sb.ToString();
            string actual = HexEncoder.Encode(hash);

            Assert.Equal(expected, actual);
        }


        private static byte[] Hash()
        {
            using (var hashAlgorithm = SHA256.Create())
            {

                // Concatenate the hash bytes into one long string.
                string value = "http://testwebsite.com/image-12345.jpeg?width=400";
                int byteCount = Encoding.ASCII.GetByteCount(value);
                byte[] buffer = new byte[byteCount];
                Encoding.ASCII.GetBytes(value, 0, value.Length, buffer, 0);
                return hashAlgorithm.ComputeHash(buffer, 0, byteCount);
            }
        }
    }
}