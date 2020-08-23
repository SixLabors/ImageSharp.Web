// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace SixLabors.ImageSharp.Web.Benchmarks
{
    [Config(typeof(MemoryConfig))]
    [WarmupCount(2)]
    public class ArrayPoolStreamBenchmarks
    {
        private readonly byte[] chunk = new byte[2048];

        public ArrayPoolStreamBenchmarks()
            => new Random(12345).NextBytes(this.chunk);

        [Params(0, 100, 1_000, 10_000, 100_000, 1_000_000, 5_000_000, 10_000_000, 50_000_000)]
        public int Bytes { get; set; }

        [Benchmark(Baseline = true)]
        public long MemoryStream() => this.Write<MemoryStream>();

        [Benchmark]
        public long ArrayPoolStream() => this.Write<ArrayPoolStream>();

        private long Write<T>()
            where T : Stream, new()
        {
            using var stream = new T();
            int remaining = this.Bytes;
            while (remaining > 0)
            {
                int take = Math.Min(remaining, this.chunk.Length);
                stream.Write(this.chunk, 0, take);
                remaining -= take;
            }

            if (this.Bytes != stream.Length)
            {
                ThrowInvalidOperation();
            }

            return stream.Length;
        }

        private static void ThrowInvalidOperation()
        {
            throw new InvalidOperationException("Length mismatch!");
        }
    }

    /*
    BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
    Intel Core i7-8650U CPU 1.90GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
    .NET Core SDK=3.1.401
      [Host]     : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
      Job-WOWTKJ : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT

    WarmupCount=2

    |          Method |    Bytes |             Mean |            Error |           StdDev |           Median | Ratio | RatioSD |     Gen 0 |     Gen 1 |     Gen 2 |   Allocated |
    |---------------- |--------- |-----------------:|-----------------:|-----------------:|-----------------:|------:|--------:|----------:|----------:|----------:|------------:|
    |    MemoryStream |        0 |         51.25 ns |         0.558 ns |         0.495 ns |         51.27 ns |  1.00 |    0.00 |    0.0172 |         - |         - |        72 B |
    | ArrayPoolStream |        0 |         53.40 ns |         1.175 ns |         1.443 ns |         53.05 ns |  1.04 |    0.04 |    0.0172 |         - |         - |        72 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |      100 |         92.44 ns |         1.048 ns |         0.929 ns |         92.30 ns |  1.00 |    0.00 |    0.0842 |         - |         - |       352 B |
    | ArrayPoolStream |      100 |        441.07 ns |         8.399 ns |         7.857 ns |        439.17 ns |  4.77 |    0.10 |    0.0591 |         - |         - |       248 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |     1000 |        159.26 ns |         3.327 ns |         6.167 ns |        157.87 ns |  1.00 |    0.00 |    0.2620 |         - |         - |      1096 B |
    | ArrayPoolStream |     1000 |        461.64 ns |         4.988 ns |         4.666 ns |        461.09 ns |  2.84 |    0.14 |    0.0591 |         - |         - |       248 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |    10000 |      2,674.79 ns |        66.113 ns |        98.955 ns |      2,647.88 ns |  1.00 |    0.00 |    7.3776 |    0.0038 |         - |     30891 B |
    | ArrayPoolStream |    10000 |      2,449.27 ns |        27.052 ns |        25.304 ns |      2,446.45 ns |  0.91 |    0.04 |    0.2251 |         - |         - |       952 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |   100000 |     66,205.49 ns |     1,081.678 ns |       958.879 ns |     65,764.49 ns |  1.00 |    0.00 |   41.6260 |   41.6260 |   41.6260 |    260351 B |
    | ArrayPoolStream |   100000 |     22,814.34 ns |       509.994 ns |     1,378.799 ns |     23,285.38 ns |  0.33 |    0.02 |    2.0752 |         - |         - |      8697 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |  1000000 |  1,133,917.50 ns |    34,663.076 ns |    95,472.177 ns |  1,141,272.95 ns |  1.00 |    0.00 |  498.0469 |  498.0469 |  498.0469 |   2095596 B |
    | ArrayPoolStream |  1000000 |    216,385.51 ns |     1,938.804 ns |     1,813.559 ns |    216,093.73 ns |  0.19 |    0.01 |   20.5078 |         - |         - |     86143 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |  5000000 |  7,023,791.89 ns |   115,411.095 ns |    96,373.491 ns |  6,991,844.53 ns |  1.00 |    0.00 |  742.1875 |  742.1875 |  742.1875 |  16775638 B |
    | ArrayPoolStream |  5000000 |  7,370,820.63 ns |   143,402.584 ns |   223,260.547 ns |  7,430,733.98 ns |  1.03 |    0.04 |  539.0625 |  437.5000 |  437.5000 |  15110136 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream | 10000000 | 14,867,145.94 ns |    73,627.296 ns |    68,871.019 ns | 14,888,979.69 ns |  1.00 |    0.00 | 1484.3750 | 1484.3750 | 1484.3750 |  33552960 B |
    | ArrayPoolStream | 10000000 | 17,774,926.54 ns |   351,848.912 ns |   889,167.099 ns | 17,748,900.00 ns |  1.23 |    0.06 |  843.7500 |  656.2500 |  656.2500 |  32317038 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream | 50000000 | 64,290,899.11 ns |   443,201.966 ns |   392,886.999 ns | 64,369,756.25 ns |  1.00 |    0.00 | 3875.0000 | 3875.0000 | 3875.0000 | 134216304 B |
    | ArrayPoolStream | 50000000 | 56,506,362.44 ns | 1,116,487.780 ns | 2,013,257.770 ns | 55,936,050.00 ns |  0.89 |    0.04 | 2400.0000 | 1400.0000 | 1400.0000 | 136420398 B | 
    */
}
