// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace SixLabors.ImageSharp.Web.Benchmarks
{
    public class MemoryConfig : ManualConfig
    {
        public MemoryConfig() => this.AddDiagnoser(MemoryDiagnoser.Default);
    }
}
