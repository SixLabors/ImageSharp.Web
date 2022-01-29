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

        private static readonly ICacheKey CacheKeyBaseline = new CacheKeyBaseline();
        private static readonly ICacheKey UriRelativeCacheKey = new UriRelativeCacheKey();
        private static readonly ICacheKey UriAbsoluteCacheKey = new UriAbsoluteCacheKey();

        [Benchmark(Baseline = true, Description = "Baseline")]
        public string CreateUsingBaseline() => CacheKeyBaseline.Create(Context, Commands);

        [Benchmark(Description = "UriRelativeCacheKey")]
        public string CreateUsingUriRelativeCacheKey() => UriRelativeCacheKey.Create(Context, Commands);

        [Benchmark(Description = "UriAbsoluteCacheKey")]
        public string CreateUsingUriAbsoluteCacheKey() => UriAbsoluteCacheKey.Create(Context, Commands);

        /*
        BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
        Intel Core i7-10750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
        .NET SDK=6.0.101
          [Host]     : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT
          DefaultJob : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT


        |              Method |     Mean |   Error |  StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
        |-------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|
        |            Baseline | 497.4 ns | 8.26 ns | 7.33 ns |  1.00 |    0.00 | 0.1106 |     696 B |
        | UriRelativeCacheKey | 270.9 ns | 5.47 ns | 7.11 ns |  0.55 |    0.02 | 0.0587 |     368 B |
        | UriAbsoluteCacheKey | 447.4 ns | 3.38 ns | 3.16 ns |  0.90 |    0.01 | 0.0939 |     592 B |
        */

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
