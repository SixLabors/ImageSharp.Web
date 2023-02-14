// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Web.Synchronization;

namespace SixLabors.ImageSharp.Web.Benchmarks.Synchronization;

[Config(typeof(MemoryConfig))]
public class RefCountedConcurrentDictionaryBenchmarks
{
    private readonly object testObj;
    private readonly RefCountedConcurrentDictionary<string, object> refCountedDictionary;

    private readonly ConcurrentDictionary<string, object> dictionary;

    public RefCountedConcurrentDictionaryBenchmarks()
    {
        this.testObj = new object();
        this.refCountedDictionary = new RefCountedConcurrentDictionary<string, object>((_) => this.testObj, null);

        this.dictionary = new ConcurrentDictionary<string, object>();

        this.refCountedDictionary.Get("foo");
        this.dictionary.TryAdd("foo", this.testObj);
    }

    [Benchmark]
    public void RefCountedConcurrentDictionary_ExistingKey()
    {
        this.refCountedDictionary.Get("foo");
        this.refCountedDictionary.Release("foo");
    }

    [Benchmark]
    public void RefCountedConcurrentDictionary_ExistingKey_JustGet()
    {
        this.refCountedDictionary.Get("foo");
    }

    [Benchmark]
    public void ConcurrentDictionary_ExistingKey_JustGet()
    {
        this.dictionary.GetOrAdd("foo", this.testObj);
    }

    [Benchmark]
    public void RefCountedConcurrentDictionary_NewKey()
    {
        this.refCountedDictionary.Get("bar");
        this.refCountedDictionary.Release("bar");
    }

    [Benchmark]
    public void ConcurrentDictionary_NewKey()
    {
        this.dictionary.GetOrAdd("bar", this.testObj);
        this.dictionary.TryRemove("bar", out _);
    }

    /*
    BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
    Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
    .NET SDK=6.0.100
      [Host]     : .NET Core 3.1.21 (CoreCLR 4.700.21.51404, CoreFX 4.700.21.51508), X64 RyuJIT
      DefaultJob : .NET Core 3.1.21 (CoreCLR 4.700.21.51404, CoreFX 4.700.21.51508), X64 RyuJIT


    |                                             Method |      Mean |    Error |   StdDev |  Gen 0 | Allocated |
    |--------------------------------------------------- |----------:|---------:|---------:|-------:|----------:|
    |         RefCountedConcurrentDictionary_ExistingKey | 138.65 ns | 1.077 ns | 1.007 ns | 0.0076 |      64 B |
    | RefCountedConcurrentDictionary_ExistingKey_JustGet |  68.00 ns | 0.372 ns | 0.310 ns | 0.0038 |      32 B |
    |           ConcurrentDictionary_ExistingKey_JustGet |  16.54 ns | 0.117 ns | 0.104 ns |      - |         - |
    |              RefCountedConcurrentDictionary_NewKey | 131.50 ns | 0.253 ns | 0.237 ns | 0.0095 |      80 B |
    |                        ConcurrentDictionary_NewKey |  83.55 ns | 0.375 ns | 0.351 ns | 0.0057 |      48 B |
    */
}
