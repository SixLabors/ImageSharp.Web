// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Benchmarks.Caching;

/// <summary>
/// A baseline naive SHA256 hashing implementation
/// </summary>
public class CacheHashBaseline : ICacheHash
{
    /// <inheritdoc/>
    public string Create(string value, uint length)
    {
        // Don't use in benchmark.
        // if (length.CompareTo(2) < 0 || length.CompareTo(64) > 0)
        // {
        //    throw new ArgumentOutOfRangeException(nameof(length), $"Value must be greater than or equal to {2} and less than or equal to {64}.");
        // }
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

            return sb.ToString();
        }
    }
}
