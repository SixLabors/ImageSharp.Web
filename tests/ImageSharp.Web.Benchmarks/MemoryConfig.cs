// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace SixLabors.ImageSharp.Web.Benchmarks
{
    public class MemoryConfig : ManualConfig
    {
        public MemoryConfig()
        {
            this.Add(MemoryDiagnoser.Default);
        }
    }
}
