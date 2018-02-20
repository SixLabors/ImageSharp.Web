// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using Xunit;
using SixLabors.ImageSharp.Web.Caching;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class CacheHashTests
    {
        private static readonly ICacheHash cacheHash = new CacheHash(Options.Create(new ImageSharpMiddlewareOptions()));

        [Fact]
        public void CacheHashEncodesExtensionCorrectly()
        {
            // Expected extension should match the default extension of the installed format
            string input = "http://testwebsite.com/image-12345.jpeg?width=400";
            string expected = ".jpeg";
            string actual = cacheHash.Create(input, 8);

            Assert.Equal(expected, Path.GetExtension(actual));

            string input2 = "http://testwebsite.com/image-12345.jpeg?width=400&format=png";
            string expected2 = ".png";
            string actual2 = cacheHash.Create(input2, 8);

            Assert.Equal(expected2, Path.GetExtension(actual2));
        }

        [Fact]
        public void CachHashProducesIdenticalResults()
        {
            string input = "http://testwebsite.com/image-12345.jpeg?width=400";
            string expected = cacheHash.Create(input, 8);
            string actual = cacheHash.Create(input, 8);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CachHashLengthIsIdentical()
        {
            string input = "http://testwebsite.com/image-12345.jpeg?width=400";
            string input2 = "http://testwebsite.com/image-12345.jpeg";
            int expected = cacheHash.Create(input, 12).Length;
            int actual = cacheHash.Create(input2, 12).Length;

            Assert.Equal(expected, actual);
            Assert.Equal(17, actual);
        }
    }
}