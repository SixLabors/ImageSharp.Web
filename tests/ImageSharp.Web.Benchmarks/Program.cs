using System.Reflection;
using BenchmarkDotNet.Running;

namespace ImageSharp.Web.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            new BenchmarkSwitcher(typeof(Program).GetTypeInfo().Assembly).Run(args);
        }
    }
}
