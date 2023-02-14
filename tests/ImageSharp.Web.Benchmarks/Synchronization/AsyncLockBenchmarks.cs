// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Web.Synchronization;

namespace SixLabors.ImageSharp.Web.Benchmarks.Synchronization;

/// <summary>
/// Each of these tests obtains a lock (which completes synchronously as nobody else is holding the lock)
/// and then releases that lock.
/// </summary>
[Config(typeof(MemoryConfig))]
public class AsyncLockBenchmarks
{
    private readonly AsyncLock asyncLock = new();
    private readonly AsyncReaderWriterLock asyncReaderWriterLock = new();
    private readonly AsyncKeyLock<string> asyncKeyLock = new();
    private readonly AsyncKeyReaderWriterLock<string> asyncKeyReaderWriterLock = new();

    [Benchmark]
    public void AsyncLock() => this.asyncLock.LockAsync().Result.Dispose();

    [Benchmark]
    public void AsyncReaderWriterLock_Reader() => this.asyncReaderWriterLock.ReaderLockAsync().Result.Dispose();

    [Benchmark]
    public void AsyncReaderWriterLock_Writer() => this.asyncReaderWriterLock.WriterLockAsync().Result.Dispose();

    [Benchmark]
    public void AsyncKeyLock() => this.asyncKeyLock.LockAsync("key").Result.Dispose();

    [Benchmark]
    public void AsyncKeyReaderWriterLock_Reader() => this.asyncKeyReaderWriterLock.ReaderLockAsync("key").Result.Dispose();

    [Benchmark]
    public void AsyncKeyReaderWriterLock_Writer() => this.asyncKeyReaderWriterLock.WriterLockAsync("key").Result.Dispose();

    /*
    BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
    Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
    .NET SDK=6.0.100
        [Host]     : .NET Core 3.1.21 (CoreCLR 4.700.21.51404, CoreFX 4.700.21.51508), X64 RyuJIT
        DefaultJob : .NET Core 3.1.21 (CoreCLR 4.700.21.51404, CoreFX 4.700.21.51508), X64 RyuJIT


    |                          Method |      Mean |    Error |   StdDev |  Gen 0 | Allocated |
    |-------------------------------- |----------:|---------:|---------:|-------:|----------:|
    |                       AsyncLock |  34.93 ns | 0.144 ns | 0.135 ns |      - |         - |
    |    AsyncReaderWriterLock_Reader |  28.45 ns | 0.117 ns | 0.109 ns |      - |         - |
    |    AsyncReaderWriterLock_Writer |  28.66 ns | 0.125 ns | 0.117 ns |      - |         - |
    |                    AsyncKeyLock | 276.48 ns | 5.379 ns | 5.031 ns | 0.0210 |     176 B |
    | AsyncKeyReaderWriterLock_Reader | 261.96 ns | 1.522 ns | 1.423 ns | 0.0210 |     176 B |
    | AsyncKeyReaderWriterLock_Writer | 266.35 ns | 1.661 ns | 1.554 ns | 0.0210 |     176 B |
    */
}
