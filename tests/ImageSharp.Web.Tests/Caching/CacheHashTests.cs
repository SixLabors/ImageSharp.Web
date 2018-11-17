// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class CacheHashTests
    {
        private static readonly IOptions<ImageSharpMiddlewareOptions> options = Options.Create(new ImageSharpMiddlewareOptions());
        private static readonly ICacheHash cacheHash = new CacheHash(options, options.Value.Configuration.MemoryAllocator);

        [Fact]
        public void CacheHashEncodesExtensionCorrectly()
        {
            // Expected extension should match the default extension of the installed format
            const string input = "http://testwebsite.com/image-12345.jpeg?width=400";
            const string expected = ".jpeg";
            string actual = cacheHash.Create(input, 8);

            Assert.Equal(expected, Path.GetExtension(actual));

            const string input2 = "http://testwebsite.com/image-12345.jpeg?width=400&format=png";
            const string expected2 = ".png";
            string actual2 = cacheHash.Create(input2, 8);

            Assert.Equal(expected2, Path.GetExtension(actual2));
        }

        [Fact]
        public void CachHashProducesIdenticalResults()
        {
            const string input = "http://testwebsite.com/image-12345.jpeg?width=400";
            string expected = cacheHash.Create(input, 8);
            string actual = cacheHash.Create(input, 8);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CachHashLengthIsIdentical()
        {
            const string input = "http://testwebsite.com/image-12345.jpeg?width=400";
            const string input2 = "http://testwebsite.com/image-12345.jpeg";
            int expected = cacheHash.Create(input, 12).Length;
            int actual = cacheHash.Create(input2, 12).Length;

            Assert.Equal(expected, actual);
            Assert.Equal(17, actual);
        }
    }
}