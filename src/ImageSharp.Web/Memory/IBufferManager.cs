// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.Memory;

namespace SixLabors.ImageSharp.Web.Memory
{
    /// <summary>
    /// Encapsulates methods that enables managing the allocation of arrays for transporting encoded image data.
    /// </summary>
    public interface IBufferManager
    {
        /// <summary>
        /// Allocates a buffer with a backing array of size <paramref name="length"/>.
        /// </summary>
        /// <param name="length">The minimum length of the array to return.</param>
        /// <returns>The <see cref="IByteBuffer"/>.</returns>
        IManagedByteBuffer Allocate(int length);
    }
}