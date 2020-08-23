// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using BenchmarkDotNet.Attributes;

namespace SixLabors.ImageSharp.Web.Benchmarks
{
    [Config(typeof(MemoryConfig))]
    [WarmupCount(2)]
    public class BufferClear
    {
        private static readonly byte[] Buffer = new byte[10000];

        [Benchmark(Baseline = true)]
        public int ArrayClear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
            return Buffer.Length;
        }

        [Benchmark]
        public int SpanClear()
        {
            new Span<byte>(Buffer, 0, Buffer.Length).Clear();
            return Buffer.Length;
        }
    }
}
