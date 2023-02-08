// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace SixLabors.ImageSharp.Web.Benchmarks;

public class MemoryConfig : ManualConfig
{
    public MemoryConfig() => this.AddDiagnoser(MemoryDiagnoser.Default);
}
