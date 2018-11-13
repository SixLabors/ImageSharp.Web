using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Benchmarks.Caching
{
    [Config(typeof(MemoryConfig))]
    public class ToHexBenchmarks
    {
        private static readonly byte[] bytes = Hash();

        [Benchmark(Baseline = true, Description = "StringBuilder ToHex")]
        public string StringBuilderToHex()
        {
            const int len = 12;
            var sb = new StringBuilder(len);
            for (int i = 0; i < len / 2; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }

            return sb.ToString();
        }

        [Benchmark(Description = "Custom ToHex")]
        public string CustomToHex()
        {
            int length = bytes.Length;
            char[] c = new char[length * 2];
            ref char charRef = ref c[0];
            ref byte bytesRef = ref bytes[0];
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
        public string CustomToHexUnsafe() => HexEncoder.Encode(new Span<byte>(bytes).Slice(0, 6));

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
    }
}
