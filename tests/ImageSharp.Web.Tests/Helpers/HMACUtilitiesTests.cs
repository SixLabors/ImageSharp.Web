// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Helpers
{
    public class HMACUtilitiesTests
    {
        private static readonly byte[] HMACSecretKey = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };

        [Theory]
        [InlineData("The quick brown fox jumped.", "8ac58e6e983b9cdea7771a2f68b7430c8923ce43f690e23a51192f427fd7ece9")]
        [InlineData("Over the lazy dog.", "2ea9f37afac5ae93e27d835cc6061ad84b31064ae29f451c9136e88ad5686a1e")]
        public void CanGenerate256BitHMAC(string value, string expected)
        {
            string actual = HMACUtilities.ComputeHMACSHA256(value, HMACSecretKey);

            Assert.NotNull(actual);
            Assert.Equal(64, actual.Length);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("The quick brown fox jumped.", "1e955e8f1b00f53b39c318d14068f2efe7c409990201f1c93f641b0d631300d32bb60a818344aa3e644f98e15ae667ad")]
        [InlineData("Over the lazy dog.", "5430f82f9b105a35a205f872b8d30041a37dfe1140011ef33d61bd87c932e7d2662136d2f8f1b550eaec99d5b1fd8351")]
        public void CanGenerate384BitHMAC(string value, string expected)
        {
            string actual = HMACUtilities.ComputeHMACSHA384(value, HMACSecretKey);

            Assert.NotNull(actual);
            Assert.Equal(96, actual.Length);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("The quick brown fox jumped.", "1efec919563bfb3f0e0334f91133c9d55170f5ac55834f0bf7c8e367c9adfd8127815fef19e742ac021ea7da728a85dfc3ddfc894402706b27247126fbefd6e7")]
        [InlineData("Over the lazy dog.", "faaa0fbca75e0efce73126770735e1a7c43a1971ea6d7105523ff29e093e7422537a6dac2799e6bb9bec7004d04f6e7308aeb4c4a27ab9bd964d26127ae8f7c8")]
        public void CanGenerate512BitHMAC(string value, string expected)
        {
            string actual = HMACUtilities.ComputeHMACSHA512(value, HMACSecretKey);

            Assert.NotNull(actual);
            Assert.Equal(128, actual.Length);
            Assert.Equal(expected, actual);
        }
    }
}
