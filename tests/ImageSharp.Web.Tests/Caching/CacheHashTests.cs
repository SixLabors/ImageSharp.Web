// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;
using Xunit;

using MSOptions = Microsoft.Extensions.Options.Options;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class CacheHashTests
    {
        private static readonly IOptions<ImageSharpMiddlewareOptions> Options = MSOptions.Create(new ImageSharpMiddlewareOptions());
        private static readonly ICacheHash CacheHash = new CacheHash(Options, Options.Value.Configuration.MemoryAllocator);

        [Fact]
        public void CachHashProducesIdenticalResults()
        {
            const string Input = "http://testwebsite.com/image-12345.jpeg?width=400";
            string expected = CacheHash.Create(Input, 8);
            string actual = CacheHash.Create(Input, 8);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CachHashProducesDifferentResults()
        {
            const string Input = "http://testwebsite.com/image-12345.jpeg?width=400";
            const string Input2 = "http://testwebsite.com/image-23456.jpeg?width=400";
            string expected = CacheHash.Create(Input, 8);
            string actual = CacheHash.Create(Input2, 8);

            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public void CachHashLengthIsIdentical()
        {
            const int Length = 12;
            const string Input = "http://testwebsite.com/image-12345.jpeg?width=400";
            const string Input2 = "http://testwebsite.com/image-12345.jpeg";
            int expected = CacheHash.Create(Input, Length).Length;
            int actual = CacheHash.Create(Input2, Length).Length;

            Assert.Equal(expected, actual);
            Assert.Equal(Length, actual);
        }
    }
}
