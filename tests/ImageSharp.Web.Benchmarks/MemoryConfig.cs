using BenchmarkDotNet.Configs;

namespace SixLabors.ImageSharp.Web.Benchmarks
{
    public class MemoryConfig : ManualConfig
    {
        public MemoryConfig()
        {
            this.Add(new BenchmarkDotNet.Diagnosers.MemoryDiagnoser());
        }
    }
}
