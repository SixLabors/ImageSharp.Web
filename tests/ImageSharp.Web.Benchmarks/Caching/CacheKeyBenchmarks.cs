// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Benchmarks.Caching
{
    [Config(typeof(MemoryConfig))]
    public class CacheKeyBenchmarks
    {
        private static readonly HttpContext Context = CreateContext();
        private static readonly CommandCollection Commands = new()
        {
            { "width", "400" }
        };

        private static readonly ICacheKey UriRelativeCacheKey = new UriRelativeCacheKey();
        private static readonly ICacheKey UriAbsoluteCacheKey = new UriAbsoluteCacheKey();

        [Benchmark(Baseline = true, Description = "UriRelativeCacheKey")]
        public string CreateUsingUriRelativeCacheKey() => UriRelativeCacheKey.Create(Context, Commands);

        [Benchmark(Description = "UriAbsoluteCacheKey")]
        public string CreateUsingUriAbsoluteCacheKey() => UriAbsoluteCacheKey.Create(Context, Commands);

        private static HttpContext CreateContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = Uri.UriSchemeHttp;
            httpContext.Request.Host = new HostString("testwebsite.com");
            httpContext.Request.PathBase = "/images";
            httpContext.Request.Path = "/image-12345.jpeg";
            httpContext.Request.QueryString = new QueryString("?should-be=ignored");

            return httpContext;
        }
    }
}
