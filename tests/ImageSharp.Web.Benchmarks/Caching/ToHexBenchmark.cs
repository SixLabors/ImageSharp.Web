using System;
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
            // char[] c = new char[bytes.Length * 2];
            const int len = 12;
            char[] c = new char[len];

            byte b;

            for (int bx = 0, cx = 0; bx < len / 2; ++bx, ++cx)
            {
                b = ((byte)(bytes[bx] >> 4));
                c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = ((byte)(bytes[bx] & 0x0F));
                c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }

            return new string(c);
        }

        [Benchmark(Description = "Custom ToHex Unsafe")]
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
