// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Creates hashed keys for the given inputs hashing them to string of length ranging from 2 to 64.
    /// Hashed keys are the result of the SHA256 computation of the input value for the given length.
    /// This ensures low collision rates with a shorter file name.
    /// </summary>
    public sealed class CacheHash : ICacheHash
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHash"/> class.
        /// </summary>
        /// <param name="options">The middleware configuration options.</param>
        public CacheHash(IOptions<ImageSharpMiddlewareOptions> options)
        {
            Guard.NotNull(options, nameof(options));
            Guard.MustBeBetweenOrEqualTo<uint>(options.Value.CachedNameLength, 2, 64, nameof(options.Value.CachedNameLength));
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Create(string value, uint length)
        {
            int byteCount = Encoding.ASCII.GetByteCount(value);

            // Allocating a buffer from the pool is ~27% slower than stackalloc so use
            // that for short strings
            if (byteCount < 257)
            {
                return HashValue(value, length, stackalloc byte[byteCount]);
            }

            byte[] buffer = null;
            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(byteCount);
                return HashValue(value, length, buffer.AsSpan(0, byteCount));
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
            using var hashAlgorithm = SHA256.Create();
            Encoding.ASCII.GetBytes(value, bufferSpan);

            // Hashed output maxes out at 32 bytes @ 256bit/8 so we're safe to use stackalloc.
            Span<byte> hash = stackalloc byte[32];
            hashAlgorithm.TryComputeHash(bufferSpan, hash, out int _);

            // length maxes out at 64 since we throw if options is greater.
            return HexEncoder.Encode(hash.Slice(0, (int)(length / 2)));
        }
    }
}
