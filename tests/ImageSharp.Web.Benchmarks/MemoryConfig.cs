using BenchmarkDotNet.Configs;

namespace ImageSharp.Web.Benchmarks
{
    public class MemoryConfig : ManualConfig
    {
        public MemoryConfig()
        {
            this.Add(new BenchmarkDotNet.Diagnosers.MemoryDiagnoser());
        }
    }
}
