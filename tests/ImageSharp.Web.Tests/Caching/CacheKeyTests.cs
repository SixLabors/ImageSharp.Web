// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class CacheKeyTests
    {
        private static readonly ICacheKey UriRelativeCacheKey = new UriRelativeCacheKey();
        private static readonly ICacheKey UriAbsoluteCacheKey = new UriAbsoluteCacheKey();

        [Fact]
        public void UriRelativeCacheKeyOuputIsRelativeAndLowercase()
        {
            HttpContext context = CreateHttpContext();
            var commands = new CommandCollection()
            {
                { "Width", "400" }
            };

            string expected = "/images/image-12345.jpeg?width=400";
            string actual = UriRelativeCacheKey.Create(context, commands);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UriAbsoluteCacheKeyOuputIsAbsoluteAndLowercase()
        {
            HttpContext context = CreateHttpContext();
            var commands = new CommandCollection()
            {
                { "Width", "400" }
            };

            string expected = "http://testwebsite.com/images/image-12345.jpeg?width=400";
            string actual = UriAbsoluteCacheKey.Create(context, commands);

            Assert.Equal(expected, actual);
        }

        private static HttpContext CreateHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = Uri.UriSchemeHttp;
            httpContext.Request.Host = new HostString("Testwebsite.com");
            httpContext.Request.PathBase = "/Images";
            httpContext.Request.Path = "/Image-12345.jpeg";
            httpContext.Request.QueryString = new QueryString("?Should-Be=Ignored");

            return httpContext;
        }
    }
}
