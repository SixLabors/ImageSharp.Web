// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.Memory;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Creates hashed keys for the given inputs hashing them to string of length ranging from 2 to 64.
    /// Hashed keys are the result of the SHA256 computation of the input value for the given length.
    /// This ensures low collision rates with a shorter file name.
    /// </summary>
    public sealed class CacheHash : ICacheHash
    {
        private readonly MemoryAllocator memoryAllocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHash"/> class.
        /// </summary>
        /// <param name="options">The middleware configuration options.</param>
        /// <param name="memoryAllocator">The memory allocator.</param>
        public CacheHash(IOptions<ImageSharpMiddlewareOptions> options, MemoryAllocator memoryAllocator)
        {
            Guard.NotNull(options, nameof(options));
            Guard.MustBeBetweenOrEqualTo<uint>(options.Value.CachedNameLength, 2, 64, nameof(options.Value.CachedNameLength));

            this.memoryAllocator = memoryAllocator;
        }

        /// <inheritdoc/>
        public string Create(string value, uint length)
        {
            int len = (int)length;
            int byteCount = Encoding.ASCII.GetByteCount(value);
            using (var hashAlgorithm = SHA256.Create())
            using (IManagedByteBuffer buffer = this.memoryAllocator.AllocateManagedByteBuffer(byteCount))
            {
                Encoding.ASCII.GetBytes(value, 0, byteCount, buffer.Array, 0);
                byte[] hash = hashAlgorithm.ComputeHash(buffer.Array, 0, byteCount);
                return $"{HexEncoder.Encode(new Span<byte>(hash).Slice(0, len / 2))}";
            }
        }
    }
}