// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Provides methods to compute a Hash-based Message Authentication Code (HMAC).
/// </summary>
public static class HMACUtilities
{
    /// <summary>
    /// The command used by image requests for transporting Hash-based Message Authentication Code (HMAC) tokens.
    /// </summary>
    public const string TokenCommand = "hmac";

    /// <summary>
    /// HMAC hash algorithms and their respective hash sizes in bytes.
    /// </summary>
    private enum HashAlgorithmSizes
    {
        HMACSHA256 = 32,
        HMACSHA384 = 48,
        HMACSHA512 = 64
    }

    /// <summary>
    /// Computes a Hash-based Message Authentication Code (HMAC) by using the SHA256 hash function.
    /// </summary>
    /// <param name="value">The value to hash</param>
    /// <param name="secret">
    /// The secret key for <see cref="HMACSHA256"/> encryption.
    /// The key can be any length. However, the recommended size is 64 bytes.
    /// </param>
    /// <returns>The hashed <see cref="string"/>.</returns>
    public static unsafe string ComputeHMACSHA256(string value, byte[] secret)
    {
        return CreateHMAC(value, secret, HashAlgorithmSizes.HMACSHA256);
    }

    /// <summary>
    /// Computes a Hash-based Message Authentication Code (HMAC) by using the SHA384 hash function.
    /// </summary>
    /// <param name="value">The value to hash</param>
    /// <param name="secret">
    /// The secret key for <see cref="HMACSHA256"/> encryption.
    /// The key can be any length. However, the recommended size is 128 bytes.
    /// </param>
    /// <returns>The hashed <see cref="string"/>.</returns>
    public static unsafe string ComputeHMACSHA384(string value, byte[] secret)
    {
        return CreateHMAC(value, secret, HashAlgorithmSizes.HMACSHA384);
    }

    /// <summary>
    /// Computes a Hash-based Message Authentication Code (HMAC) by using the SHA512 hash function.
    /// </summary>
    /// <param name="value">The value to hash</param>
    /// <param name="secret">
    /// The secret key for <see cref="HMACSHA256"/> encryption.
    /// The key can be any length. However, the recommended size is 128 bytes.
    /// </param>
    /// <returns>The hashed <see cref="string"/>.</returns>
    public static unsafe string ComputeHMACSHA512(string value, byte[] secret)
    {
        return CreateHMAC(value, secret, HashAlgorithmSizes.HMACSHA512);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe string CreateHMAC(string value, ReadOnlySpan<byte> secret, HashAlgorithmSizes hashSize)
    {
        int byteCount = Encoding.ASCII.GetByteCount(value);
        byte[]? buffer = null;

        try
        {
            // Allocating a buffer from the pool is ~27% slower than stackalloc so use that for short strings
            Span<byte> bytes = byteCount <= 128
                ? stackalloc byte[byteCount]
                : (buffer = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

            Encoding.ASCII.GetBytes(value, bytes);

            // Safe to always stackalloc here. We max out at 64 bytes.
            Span<byte> hash = stackalloc byte[(int)hashSize];
            switch (hashSize)
            {
                default:
                case HashAlgorithmSizes.HMACSHA256:
                    HMACSHA256.TryHashData(secret, bytes, hash, out _);
                    break;
                case HashAlgorithmSizes.HMACSHA384:
                    HMACSHA384.TryHashData(secret, bytes, hash, out _);
                    break;
                case HashAlgorithmSizes.HMACSHA512:
                    HMACSHA512.TryHashData(secret, bytes, hash, out _);
                    break;
            }

            // Finally encode the hash to make it web safe.
            return HexEncoder.Encode(hash);
        }
        finally
        {
            if (buffer is not null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
