// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Benchmarks.Caching
{
    [Config(typeof(MemoryConfig))]
    public class CacheHashBenchmarks
    {
        private const string URL = "http://testwebsite.com/image-12345.jpeg?width=400";
        private static readonly IOptions<ImageSharpMiddlewareOptions> MWOptions = Options.Create(new ImageSharpMiddlewareOptions());
        private static readonly CacheHash Sha256Hasher = new CacheHash(MWOptions, MWOptions.Value.Configuration.MemoryAllocator);
        private static readonly CacheHashBaseline NaiveSha256Hasher = new CacheHashBaseline();

        [Benchmark(Baseline = true, Description = "Baseline Sha256Hasher")]
        public string HashUsingBaselineHash() => NaiveSha256Hasher.Create(URL, 12);

        [Benchmark(Description = "Sha256Hasher")]
        public string HashUsingSha256() => Sha256Hasher.Create(URL, 12);
    }
}
