// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
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
        StringBuilder sb = new(hash.Length * 2);
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
        }

        string expected = sb.ToString();
        string actual = HexEncoder.Encode(hash);

        Assert.Equal(expected, actual);
    }

    private static byte[] Hash()
    {
        // Concatenate the hash bytes into one long string.
        const string value = "http://testwebsite.com/image-12345.jpeg?width=400";
        int byteCount = Encoding.ASCII.GetByteCount(value);
        byte[] buffer = new byte[byteCount];
        Encoding.ASCII.GetBytes(value, 0, value.Length, buffer, 0);
        return SHA256.HashData(buffer.AsSpan(0, byteCount));
    }
}
