// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Text;
using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Benchmarks.Caching;

[Config(typeof(MemoryConfig))]
public class StringJoinBenchmarks
{
    private const string Key = "abcdefghijkl";

    [Params(0, 8, 12)]
    public int CacheFolderDepth { get; set; }

    [Benchmark(Baseline = true, Description = "String.Join")]
    public string JoinUsingString()
        => $"{string.Join("/", Key.Substring(0, this.CacheFolderDepth).ToCharArray())}/{Key}";

    [Benchmark(Description = "StringBuilder.Append")]
    public string JoinUsingStringBuilder()
    {
        ReadOnlySpan<char> keySpan = Key;
        const char separator = '/';

        // Each key substring char + separator + key
        var sb = new StringBuilder((this.CacheFolderDepth * 2) + Key.Length);
        ReadOnlySpan<char> paths = keySpan.Slice(0, this.CacheFolderDepth);
        for (int i = 0; i < paths.Length; i++)
        {
            sb.Append(paths[i]);
            sb.Append(separator);
        }

        sb.Append(Key);
        return sb.ToString();
    }

    [Benchmark(Description = "String.Create")]
    public string JoinUsingStringCreate()
        => PhysicalFileSystemCache.ToFilePath(Key, this.CacheFolderDepth);

    /*
    BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
    Intel Core i7-8650U CPU 1.90GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
    .NET Core SDK=3.1.401
      [Host]     : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
      DefaultJob : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT


    |               Method |      Mean |    Error |   StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
    |--------------------- |----------:|---------:|---------:|------:|-------:|------:|------:|----------:|
    |          String.Join | 285.17 ns | 5.006 ns | 4.180 ns |  1.00 | 0.1278 |     - |     - |     536 B |
    | StringBuilder.Append |  96.72 ns | 0.549 ns | 0.487 ns |  0.34 | 0.0573 |     - |     - |     240 B |
    |        String.Create |  42.03 ns | 0.186 ns | 0.165 ns |  0.15 | 0.0440 |     - |     - |     184 B |
    */
}
