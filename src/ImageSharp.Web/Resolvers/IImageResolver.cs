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
        /// Gets the date and time in coordinated universal time (UTC) since the source file was last modified.
        /// </summary>
        /// <returns>The <see cref="DateTime"/>.</returns>
        Task<DateTime> GetLastWriteTimeUtcAsync();

        /// <summary>
        /// Gets the input image stream in an asynchronous manner.
        /// </summary>
        /// <returns>The <see cref="Task{Stream}"/>.</returns>
        Task<Stream> OpenReadAsync();

        /// <summary>
        /// Gets the output image stream.
        /// </summary>
        /// <returns>The <see cref="Stream"/>.</returns>
        Stream OpenWrite();
    }
}
