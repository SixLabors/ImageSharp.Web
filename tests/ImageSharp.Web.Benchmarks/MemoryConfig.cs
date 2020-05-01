// Copyright (c) Six Labors and contributors.
// Licensed under the GNU Affero General Public License, Version 3.

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
