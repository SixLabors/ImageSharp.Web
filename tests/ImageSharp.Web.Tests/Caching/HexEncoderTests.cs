// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Tests.Caching;

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
            const string Value = "http://testwebsite.com/image-12345.jpeg?width=400";
            int byteCount = Encoding.ASCII.GetByteCount(Value);
            byte[] buffer = new byte[byteCount];
            Encoding.ASCII.GetBytes(Value, 0, Value.Length, buffer, 0);
            return hashAlgorithm.ComputeHash(buffer, 0, byteCount);
        }
    }
}
