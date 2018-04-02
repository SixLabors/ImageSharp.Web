// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Buffers;

namespace SixLabors.ImageSharp.Web.Memory
{
    /// <summary>
    /// Manages the allocation of reusable arrays for transporting encoded image data via a resource pool.
    /// </summary>
    public class PooledBufferManager : IBufferManager
    {
        /// <summary>
        /// The maximum length of each array in the pool (2^21).
        /// </summary>
        internal const int DefaultMaxLength = 1024 * 1024 * 2;

        /// <summary>
        /// The maximum number of array instances that may be stored in each bucket in the pool. This gives us a pool of up to 50MB.
        /// </summary>
        internal const int DefaultMaxArraysPerBucket = 25;

        /// <summary>
        /// The <see cref="ArrayPool{Byte}"/> which is not kept clean.
        /// </summary>
        private readonly ArrayPool<byte> arrayPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledBufferManager"/> class.
        /// </summary>
        public PooledBufferManager()
            : this(DefaultMaxLength, DefaultMaxArraysPerBucket)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledBufferManager"/> class.
        /// </summary>
        /// <param name="maxLength">The maximum length of an array instance that may be stored in the pool.</param>
        /// <param name="maxArraysPerBucket">
        /// The maximum number of array instances that may be stored in each bucket in the pool.
        /// The pool groups arrays of similar lengths into buckets for faster access.
        /// </param>
        public PooledBufferManager(int maxLength, int maxArraysPerBucket)
        {
            Guard.MustBeGreaterThan(maxLength, 0, nameof(maxLength));
            Guard.MustBeGreaterThan(maxArraysPerBucket, 0, nameof(maxArraysPerBucket));
            this.arrayPool = ArrayPool<byte>.Create(maxLength, maxArraysPerBucket);
        }

        /// <inheritdoc />
        public IByteBuffer Allocate(int minimumLength)
        {
            return new PooledByteBuffer(this, this.arrayPool.Rent(minimumLength), minimumLength);
        }

        /// <summary>
        /// Returns the rented array back to the pool.
        /// </summary>
        /// <param name="array">The array to return to the buffer pool.</param>
        internal void Return(byte[] array)
        {
            if (array != null)
            {
                this.arrayPool.Return(array);
            }
        }
    }
}