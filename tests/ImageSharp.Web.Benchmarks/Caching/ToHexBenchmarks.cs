// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Benchmarks.Caching;

[Config(typeof(MemoryConfig))]
public class ToHexBenchmarks
{
    private static readonly byte[] Bytes = Hash();

    [Benchmark(Baseline = true, Description = "StringBuilder ToHex")]
    public string StringBuilderToHex()
    {
        const int len = 12;
        var sb = new StringBuilder(len);
        for (int i = 0; i < len / 2; i++)
        {
            sb.Append(Bytes[i].ToString("x2"));
        }

        return sb.ToString();
    }

    [Benchmark(Description = "Custom ToHex")]
    public string CustomToHex()
    {
        int length = Bytes.Length;
        char[] c = new char[length * 2];
        ref char charRef = ref c[0];
        ref byte bytesRef = ref Bytes[0];
        const int padHi = 0x37 + 0x20;
        const int padLo = 0x30;

        byte b;
        for (int bx = 0, cx = 0; bx < length; ++bx, ++cx)
        {
            byte bref = Unsafe.Add(ref bytesRef, bx);
            b = (byte)(bref >> 4);
            Unsafe.Add(ref charRef, cx) = (char)(b > 9 ? b + padHi : b + padLo);

            b = (byte)(bref & 0x0F);
            Unsafe.Add(ref charRef, ++cx) = (char)(b > 9 ? b + padHi : b + padLo);
        }

        return new string(c);
    }

    [Benchmark(Description = "HexEncoder.Encode with LUT")]
    public string CustomToHexUnsafe() => HexEncoder.Encode(new Span<byte>(Bytes).Slice(0, 6));

    private static byte[] Hash()
    {
        using (var hashAlgorithm = SHA256.Create())
        {
            // Concatenate the hash bytes into one long string.
            string value = "http://testwebsite.com/image-12345.jpeg?width=400";
            int byteCount = Encoding.ASCII.GetByteCount(value);
            byte[] buffer = new byte[byteCount];
            Encoding.ASCII.GetBytes(value, 0, value.Length, buffer, 0);
            return hashAlgorithm.ComputeHash(buffer, 0, byteCount);
        }
    }

    /*
    BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
    Intel Core i7-8650U CPU 1.90GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
    .NET Core SDK=3.1.401
      [Host]     : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
      DefaultJob : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT


    |                       Method |      Mean |    Error |   StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
    |----------------------------- |----------:|---------:|---------:|------:|-------:|------:|------:|----------:|
    |        'StringBuilder ToHex' | 201.78 ns | 0.900 ns | 0.752 ns |  1.00 | 0.0801 |     - |     - |     336 B |
    |               'Custom ToHex' |  83.37 ns | 0.856 ns | 0.801 ns |  0.41 | 0.0726 |     - |     - |     304 B |
    | 'HexEncoder.Encode with LUT' |  23.17 ns | 0.235 ns | 0.196 ns |  0.11 | 0.0114 |     - |     - |      48 B |
    */
}
