using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;

namespace ImageSharp.Web.Benchmarks.Caching
{
    [Config(typeof(MemoryConfig))]
    public class CacheHashBenchmarks
    {
        private const string URL = "http://testwebsite.com/image-12345.jpeg?width=400";
        private static CacheHash Sha256Hasher = new CacheHash(Options.Create(new ImageSharpMiddlewareOptions()));
        private static CacheHashBaseline NaiveSha256Hasher = new CacheHashBaseline(Options.Create(new ImageSharpMiddlewareOptions()));

        [Benchmark(Baseline = true, Description = "Baseline Sha256Hasher")]
        public string HashUsingXxHash()
        {
            return NaiveSha256Hasher.Create(URL, 12);
        }

        [Benchmark(Description = "Sha256Hasher")]
        public string HashUsingSha256()
        {
            return Sha256Hasher.Create(URL, 12);
        }
    }
}
