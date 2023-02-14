// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Tests.Helpers;

public class HMACUtilitiesTests
{
    private static readonly byte[] HMACSecretKey = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };

    [Theory]
    [InlineData("The quick brown fox jumped.")]
    [InlineData("Over the lazy dog.")]
    public void CanGenerate256BitHMAC(string value)
    {
        string expected = HMACUtilities.ComputeHMACSHA256(value, HMACSecretKey);
        string actual = HMACUtilities.ComputeHMACSHA256(value, HMACSecretKey);

        Assert.Equal(expected, actual);
        Assert.Equal(64, expected.Length);
    }

    [Theory]
    [InlineData("The quick brown fox jumped.")]
    [InlineData("Over the lazy dog.")]
    public void CanGenerate384BitHMAC(string value)
    {
        string expected = HMACUtilities.ComputeHMACSHA384(value, HMACSecretKey);
        string actual = HMACUtilities.ComputeHMACSHA384(value, HMACSecretKey);

        Assert.Equal(expected, actual);
        Assert.Equal(96, expected.Length);
    }

    [Theory]
    [InlineData("The quick brown fox jumped.")]
    [InlineData("Over the lazy dog.")]
    public void CanGenerate512BitHMAC(string value)
    {
        string expected = HMACUtilities.ComputeHMACSHA512(value, HMACSecretKey);
        string actual = HMACUtilities.ComputeHMACSHA512(value, HMACSecretKey);

        Assert.Equal(expected, actual);
        Assert.Equal(128, expected.Length);
    }
}
