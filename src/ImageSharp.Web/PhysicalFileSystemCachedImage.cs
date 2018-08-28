// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Provides means to manage image buffers within the physical file system.
    /// </summary>
    public class PhysicalFileSystemCachedImage : ICachedImage
    {
        private readonly IFileInfo fileInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemCachedImage"/> class.
        /// </summary>
        /// <param name="fileInfo">The input file info.</param>
        public PhysicalFileSystemCachedImage(IFileInfo fileInfo) => this.fileInfo = fileInfo;

        /// <inheritdoc/>
        public DateTime LastWriteTimeUtc() => this.fileInfo.LastModified.UtcDateTime;

        /// <inheritdoc/>
        public Stream OpenRead() => this.fileInfo.CreateReadStream();

        /// <inheritdoc/>
        public Stream OpenWrite() => new MemoryStream(); // TODO: Chunked stream
    }
}