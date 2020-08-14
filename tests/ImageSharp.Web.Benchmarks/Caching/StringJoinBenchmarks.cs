// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace SixLabors.ImageSharp.Web.Benchmarks.Caching
{
    [Config(typeof(MemoryConfig))]
    public class StringJoinBenchmarks
    {
        private const string Key = "abcdefghijkl";
        private const int CachedNameLength = 12;

        [Benchmark(Baseline = true, Description = "String.Join")]
        public string JoinUsingString()
            => $"{string.Join("/", Key.Substring(0, CachedNameLength).ToCharArray())}/{Key}";

        [Benchmark(Description = "StringBuilder.Append")]
        public string JoinUsingStringBuilder()
        {
            ReadOnlySpan<char> keySpan = Key;
            const char separator = '/';

            // Each key substring char + separator + key
            var sb = new StringBuilder((CachedNameLength * 2) + Key.Length);
            ReadOnlySpan<char> paths = keySpan.Slice(0, CachedNameLength);
            for (int i = 0; i < paths.Length; i++)
            {
                sb.Append(paths[i]);
                sb.Append(separator);
            }

            sb.Append(Key);
            return sb.ToString();
        }

        [Benchmark(Description = "String.Create")]
        public unsafe string JoinUsingStringCreate()
        {
            ReadOnlySpan<char> keySpan = Key;
            const char separator = '/';

            // Each key substring char + separator + key
            int length = (CachedNameLength * 2) + Key.Length;
            fixed (char* keyPtr = Key)
            {
                return string.Create(length, (Ptr: (IntPtr)keyPtr, Key.Length), (chars, args) =>
                {
                    var keySpan = new ReadOnlySpan<char>((char*)args.Ptr, args.Length);
                    ref char keyRef = ref MemoryMarshal.GetReference(keySpan);
                    ref char charRef = ref MemoryMarshal.GetReference(chars);

                    int index = 0;
                    for (int i = 0; i < CachedNameLength; i++)
                    {
                        Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref keyRef, i);
                        Unsafe.Add(ref charRef, index++) = separator;
                    }

                    for (int i = 0; i < keySpan.Length; i++)
                    {
                        Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref keyRef, i);
                    }
                });
            }
        }
    }
}
