// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Buffers;

namespace SixLabors.ImageSharp.Web.Memory
{
    /// <summary>
    /// Provides a resource pool that enables reusing arrays for transporting encoded image data.
    /// </summary>
    public class BufferDataPool : IBufferDataPool
    {
        /// <summary>
        /// The maximum length of each array in the pool (2^21).
        /// </summary>
        private const int MaxLength = 1024 * 1024 * 2;

        /// <summary>
        /// The <see cref="ArrayPool{Byte}"/> which is not kept clean. This gives us a pool of up to 100MB.
        /// </summary>
        private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Create(MaxLength, 50);

        /// <inheritdoc />
        public byte[] Rent(int minimumLength)
        {
            return ArrayPool.Rent(minimumLength);
        }

        /// <inheritdoc />
        public void Return(byte[] array)
        {
            try
            {
                if (array != null)
                {
                    ArrayPool.Return(array);
                }
            }
            catch
            {
                // Do nothing. Someone didn't use the Bufferpool in their IImageResolver
                // and they only have themselves to blame for the performance hit.
            }
        }
    }
}