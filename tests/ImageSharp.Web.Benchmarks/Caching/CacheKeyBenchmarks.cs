// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Benchmarks.Caching;

[Config(typeof(MemoryConfig))]
public class CacheKeyBenchmarks
{
    private static readonly HttpContext Context = CreateContext();
    private static readonly CommandCollection Commands = new()
    {
        { "width", "400" }
    };

    private static readonly ICacheKey LegacyV1CacheKey = new LegacyV1CacheKey();
    private static readonly ICacheKey UriRelativeCacheKey = new UriRelativeCacheKey();
    private static readonly ICacheKey UriAbsoluteCacheKey = new UriAbsoluteCacheKey();
    private static readonly ICacheKey UriRelativeLowerInvariantCacheKey = new UriRelativeLowerInvariantCacheKey();
    private static readonly ICacheKey UriAbsoluteLowerInvariantCacheKey = new UriAbsoluteLowerInvariantCacheKey();

    [Benchmark(Baseline = true, Description = nameof(LegacyV1CacheKey))]
    public string CreateUsingBaseline() => LegacyV1CacheKey.Create(Context, Commands);

    [Benchmark(Description = nameof(UriRelativeCacheKey))]
    public string CreateUsingUriRelativeCacheKey() => UriRelativeCacheKey.Create(Context, Commands);

    [Benchmark(Description = nameof(UriRelativeLowerInvariantCacheKey))]
    public string CreateUsingUriRelativeLowerInvariantCacheKey() => UriRelativeCacheKey.Create(Context, Commands);

    [Benchmark(Description = nameof(UriAbsoluteCacheKey))]
    public string CreateUsingUriAbsoluteCacheKey() => UriAbsoluteCacheKey.Create(Context, Commands);

    [Benchmark(Description = nameof(UriAbsoluteLowerInvariantCacheKey))]
    public string CreateUsingUriAbsoluteLowerInvariantCacheKey() => UriAbsoluteCacheKey.Create(Context, Commands);

    /*
    BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
    Intel Core i7-8650U CPU 1.90GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
    .NET SDK=6.0.101
      [Host]     : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT
      DefaultJob : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT


    |                             Method |     Mean |    Error |   StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
    |----------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|
    |                   LegacyV1CacheKey | 666.8 ns | 13.22 ns | 14.14 ns |  1.00 |    0.00 | 0.1659 |     696 B |
    |                UriRelativeCacheKey | 358.0 ns |  1.98 ns |  1.65 ns |  0.54 |    0.01 | 0.0706 |     296 B |
    | UriRelativeLowerInvariantCacheKey | 363.1 ns |  7.20 ns | 11.21 ns |  0.55 |    0.02 | 0.0706 |     296 B |
    |                UriAbsoluteCacheKey | 490.3 ns |  5.14 ns |  4.80 ns |  0.74 |    0.02 | 0.0763 |     320 B |
    | UriAbsoluteLowerInvariantCacheKey | 475.4 ns |  4.18 ns |  3.71 ns |  0.71 |    0.02 | 0.0763 |     320 B |
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
