// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.NoInlining)]
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
    |    MemoryStream |        0 |         48.57 ns |         0.398 ns |         0.373 ns |         48.59 ns |  1.00 |    0.00 |    0.0172 |         - |         - |        72 B |
    | ArrayPoolStream |        0 |         57.65 ns |         0.713 ns |         0.595 ns |         57.68 ns |  1.19 |    0.02 |    0.0172 |    0.0001 |         - |        72 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |      100 |         92.20 ns |         1.218 ns |         1.079 ns |         91.99 ns |  1.00 |    0.00 |    0.0842 |         - |         - |       352 B |
    | ArrayPoolStream |      100 |        108.85 ns |         1.035 ns |         0.918 ns |        108.74 ns |  1.18 |    0.02 |    0.0172 |         - |         - |        72 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |     1000 |        177.21 ns |         5.906 ns |        17.229 ns |        170.94 ns |  1.00 |    0.00 |    0.2620 |         - |         - |      1096 B |
    | ArrayPoolStream |     1000 |        163.75 ns |         1.163 ns |         1.088 ns |        163.60 ns |  0.97 |    0.05 |    0.0172 |         - |         - |        72 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |    10000 |      2,662.05 ns |        36.780 ns |        28.715 ns |      2,651.90 ns |  1.00 |    0.00 |    7.3776 |    0.0038 |         - |     30891 B |
    | ArrayPoolStream |    10000 |        949.87 ns |        18.911 ns |        19.420 ns |        952.24 ns |  0.36 |    0.01 |    0.0172 |         - |         - |        72 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |   100000 |     66,324.20 ns |     1,206.548 ns |     1,069.574 ns |     66,307.68 ns |  1.00 |    0.00 |   41.6260 |   41.6260 |   41.6260 |    260351 B |
    | ArrayPoolStream |   100000 |      6,233.35 ns |       194.150 ns |       181.608 ns |      6,166.53 ns |  0.09 |    0.00 |    0.0153 |         - |         - |        72 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |  1000000 |    928,935.06 ns |     8,751.441 ns |     7,757.925 ns |    929,483.59 ns |  1.00 |    0.00 |  499.0234 |  499.0234 |  499.0234 |   2095596 B |
    | ArrayPoolStream |  1000000 |     68,249.88 ns |       702.734 ns |       622.955 ns |     68,039.09 ns |  0.07 |    0.00 |         - |         - |         - |        72 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream |  5000000 |  7,188,608.08 ns |   147,084.294 ns |   201,330.642 ns |  7,115,904.69 ns |  1.00 |    0.00 |  742.1875 |  742.1875 |  742.1875 |  16775638 B |
    | ArrayPoolStream |  5000000 |  6,227,567.79 ns |    61,707.314 ns |    51,528.402 ns |  6,204,451.56 ns |  0.86 |    0.03 |  406.2500 |  406.2500 |  406.2500 |  14680278 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream | 10000000 | 14,220,921.25 ns |   241,265.703 ns |   225,680.088 ns | 14,198,512.50 ns |  1.00 |    0.00 | 1484.3750 | 1484.3750 | 1484.3750 |  33552960 B |
    | ArrayPoolStream | 10000000 | 13,837,654.90 ns |   273,710.823 ns |   457,309.298 ns | 13,746,661.72 ns |  0.98 |    0.04 |  640.6250 |  640.6250 |  640.6250 |  31457561 B |
    |                 |          |                  |                  |                  |                  |       |         |           |           |           |             |
    |    MemoryStream | 50000000 | 65,091,715.00 ns |   798,369.233 ns |   746,795.075 ns | 65,071,037.50 ns |  1.00 |    0.00 | 3875.0000 | 3875.0000 | 3875.0000 | 134216304 B |
    | ArrayPoolStream | 50000000 | 64,581,138.59 ns | 1,282,853.263 ns | 1,622,398.625 ns | 63,905,000.00 ns |  1.00 |    0.03 | 1875.0000 | 1875.0000 | 1875.0000 | 132120960 B |
    */
}
