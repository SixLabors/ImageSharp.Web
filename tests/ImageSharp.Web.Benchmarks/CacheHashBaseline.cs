using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Benchmarks
{
    /// <summary>
    /// A baseline naive SHA256 hashing implementation
    /// </summary>
    public class CacheHashBaseline : ICacheHash
    {
        private readonly ImageSharpMiddlewareOptions options;
        private readonly FormatHelper formatHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHash"/> class.
        /// </summary>
        /// <param name="options">The middleware configuration options</param>
        public CacheHashBaseline(IOptions<ImageSharpMiddlewareOptions> options)
        {
            this.options = options.Value;
            this.formatHelper = new FormatHelper(this.options.Configuration);
        }

        /// <inheritdoc/>
        public string Create(string value, uint length)
        {
            if (length.CompareTo(2) < 0 || length.CompareTo(64) > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"Value must be greater than or equal to {2} and less than or equal to {64}.");
            }

            using (var hashAlgorithm = SHA256.Create())
            {
                // Concatenate the hash bytes into one long string.
                int len = (int)length;
                byte[] hash = hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(value));
                var sb = new StringBuilder(len);
                for (int i = 0; i < len / 2; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }

                sb.AppendFormat(".{0}", this.formatHelper.GetExtensionOrDefault(value));
                return sb.ToString();
            }
        }
    }
}