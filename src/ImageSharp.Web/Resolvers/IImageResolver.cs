// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Specifies the contract for resolving image buffers from different locations.
    /// </summary>
    public interface IImageResolver
    {
        /// <summary>
        /// Gets metadata associated with this image.
        /// </summary>
        /// <returns>The <see cref="ImageMetadata"/>.</returns>
        Task<ImageMetadata> GetImageMetadataAsync();

        /// <summary>
        /// Gets the input image stream in an asynchronous manner.
        /// </summary>
        /// <returns>The <see cref="Task{Stream}"/>.</returns>
        Task<Stream> OpenReadAsync();
    }
}
