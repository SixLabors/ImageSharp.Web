// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Benchmarks.Commands
{
    [Config(typeof(MemoryConfig))]
    public class CommandCollectionBenchmarks
    {
        private static readonly CommandCollection Commands = new()
        {
            { "width", "400" },
            { "height", "400" }
        };

        private static readonly Consumer Consumer = new();

        [Benchmark(Baseline = true, Description = nameof(CommandCollection.Keys))]
        public void ConsumeKeys() => Commands.Keys.Consume(Consumer);

        [Benchmark(Description = "KeysList")]
        public void ConsumeKeysList() => new List<string>(Commands.Keys).Consume(Consumer);

        [Benchmark(Description = nameof(CommandCollection.UnorderedKeys))]
        public void ConsumeAllKeys() => Commands.UnorderedKeys.Consume(Consumer);

        /*
        BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
        11th Gen Intel Core i7-11370H 3.30GHz, 1 CPU, 8 logical and 4 physical cores
        .NET SDK=6.0.300
          [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
          DefaultJob : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT


        |        Method |      Mean |    Error |   StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
        |-------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|
        |          Keys |  68.70 ns | 0.738 ns | 0.690 ns |  1.00 |    0.00 | 0.0153 |      96 B |
        |      KeysList | 133.10 ns | 0.728 ns | 0.645 ns |  1.94 |    0.03 | 0.0355 |     224 B |
        | UnorderedKeys |  32.70 ns | 0.174 ns | 0.154 ns |  0.48 |    0.01 | 0.0063 |      40 B |
        */
    }
}
