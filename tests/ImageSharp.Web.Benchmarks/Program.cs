using System.Reflection;
using BenchmarkDotNet.Running;

namespace SixLabors.ImageSharp.Web.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            new BenchmarkSwitcher(typeof(Program).GetTypeInfo().Assembly).Run(args);
        }
    }
}
