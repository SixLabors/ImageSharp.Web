// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

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
        /// Asynchronously gets metadata associated with this image.
        /// </summary>
        /// <returns>The <see cref="ImageMetadata"/>.</returns>
        Task<ImageMetadata> GetMetaDataAsync();

        /// <summary>
        /// Asynchronously gets the input image stream.
        /// </summary>
        /// <returns>The <see cref="Task{Stream}"/>.</returns>
        Task<Stream> OpenReadAsync();
    }
}
