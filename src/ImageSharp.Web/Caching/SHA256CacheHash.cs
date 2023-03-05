// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Caching;

/// <summary>
/// Creates hashed keys for the given inputs hashing them to string of length ranging from 2 to 64.
/// Hashed keys are the result of the SHA256 computation of the input value for the given length.
/// This ensures low collision rates with a shorter file name.
/// </summary>
public sealed class SHA256CacheHash : ICacheHash
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SHA256CacheHash"/> class.
    /// </summary>
    /// <param name="options">The middleware configuration options.</param>
    public SHA256CacheHash(IOptions<ImageSharpMiddlewareOptions> options)
    {
        Guard.NotNull(options, nameof(options));
        Guard.MustBeBetweenOrEqualTo<uint>(options.Value.CacheHashLength, 2, 64, nameof(options.Value.CacheHashLength));
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Create(string value, uint length)
    {
        int byteCount = Encoding.ASCII.GetByteCount(value);
        byte[]? buffer = null;

        try
        {
            // Allocating a buffer from the pool is ~27% slower than stackalloc so use that for short strings
            Span<byte> bytes = byteCount <= 128
                ? stackalloc byte[byteCount]
                : (buffer = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

            return HashValue(value, length, bytes);
        }
        finally
        {
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string HashValue(ReadOnlySpan<char> value, uint length, Span<byte> bufferSpan)
    {
        Encoding.ASCII.GetBytes(value, bufferSpan);

        // Hashed output maxes out at 32 bytes @ 256bit/8 so we're safe to use stackalloc
        Span<byte> hash = stackalloc byte[32];
        SHA256.TryHashData(bufferSpan, hash, out int _);

        // Length maxes out at 64 since we throw if options is greater
        return HexEncoder.Encode(hash[..(int)(length / 2)]);
    }
}
