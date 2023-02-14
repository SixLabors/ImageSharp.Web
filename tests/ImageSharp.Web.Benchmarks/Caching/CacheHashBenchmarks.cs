// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Benchmarks.Caching;

[Config(typeof(MemoryConfig))]
public class CacheHashBenchmarks
{
    private const string URL = "http://testwebsite.com/image-12345.jpeg?width=400";
    private static readonly IOptions<ImageSharpMiddlewareOptions> MWOptions = Options.Create(new ImageSharpMiddlewareOptions());
    private static readonly SHA256CacheHash Sha256Hasher = new SHA256CacheHash(MWOptions);
    private static readonly CacheHashBaseline NaiveSha256Hasher = new CacheHashBaseline();

    [Benchmark(Baseline = true, Description = "Baseline Sha256Hasher")]
    public string HashUsingBaselineHash() => NaiveSha256Hasher.Create(URL, 12);

    [Benchmark(Description = "Sha256Hasher")]
    public string HashUsingSha256() => Sha256Hasher.Create(URL, 12);

    /*
    BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
    Intel Core i7-8650U CPU 1.90GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
    .NET Core SDK=3.1.401
      [Host]     : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
      DefaultJob : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT


    |                  Method |       Mean |   Error |  StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
    |------------------------ |-----------:|--------:|--------:|------:|-------:|------:|------:|----------:|
    | 'Baseline Sha256Hasher' | 1,065.7 ns | 8.95 ns | 8.37 ns |  1.00 | 0.1564 |     - |     - |     656 B |
    |            Sha256Hasher |   786.7 ns | 9.77 ns | 9.14 ns |  0.74 | 0.0420 |     - |     - |     176 B |
    */
}
