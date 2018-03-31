// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Web.Memory
{
    /// <summary>
    /// Encapsulates methods that enables reusing arrays for transporting encoded image data.
    /// </summary>
    public interface IBufferDataPool
    {
        /// <summary>
        /// Rents the array from the pool.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the array to return.</param>
        /// <returns>The <see cref="T:Byte[]"/></returns>
        byte[] Rent(int minimumLength);

        /// <summary>
        /// Returns the rented array back to the pool.
        /// </summary>
        /// <param name="array">The array to return to the buffer pool.</param>
        void Return(byte[] array);
    }
}