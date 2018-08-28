// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Specifies the contract for managing image buffers from different locations.
    /// </summary>
    public interface ICachedImage
    {
        /// <summary>
        /// Gets the date and time in coordinated universal time (UTC) since the source file was last modified.
        /// </summary>
        /// <returns>The <see cref="DateTime"/>.</returns>
        DateTime LastWriteTimeUtc();

        /// <summary>
        /// Gets the input image stream.
        /// </summary>
        /// <returns>The <see cref="Stream"/>.</returns>
        Stream OpenRead();

        /// <summary>
        /// Gets the output image stream.
        /// </summary>
        /// <returns>The <see cref="Stream"/>.</returns>
        Stream OpenWrite();
    }
}
