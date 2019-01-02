// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

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
        public void CachHashProducesIdenticalResults()
        {
            const string input = "http://testwebsite.com/image-12345.jpeg?width=400";
            string expected = cacheHash.Create(input, 8);
            string actual = cacheHash.Create(input, 8);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CachHashProducesDifferentResults()
        {
            const string input = "http://testwebsite.com/image-12345.jpeg?width=400";
            const string input2 = "http://testwebsite.com/image-23456.jpeg?width=400";
            string expected = cacheHash.Create(input, 8);
            string actual = cacheHash.Create(input2, 8);

            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public void CachHashLengthIsIdentical()
        {
            const int length = 12;
            const string input = "http://testwebsite.com/image-12345.jpeg?width=400";
            const string input2 = "http://testwebsite.com/image-12345.jpeg";
            int expected = cacheHash.Create(input, length).Length;
            int actual = cacheHash.Create(input2, length).Length;

            Assert.Equal(expected, actual);
            Assert.Equal(length, actual);
        }
    }
}