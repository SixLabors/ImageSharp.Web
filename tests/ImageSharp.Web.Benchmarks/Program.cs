// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Reflection;
using BenchmarkDotNet.Running;

namespace SixLabors.ImageSharp.Web.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        new BenchmarkSwitcher(typeof(Program).GetTypeInfo().Assembly).Run(args);
    }
}
