// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Provides means to manage image buffers within the physical file system.
    /// </summary>
    public class PhysicalFileSystemResolver : IImageResolver
    {
        private readonly IFileInfo fileInfo;
        private readonly ImageMetaData metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemResolver"/> class.
        /// </summary>
        /// <param name="fileInfo">The input file info.</param>
        /// <param name="metadata">The image metadata associated with this file.</param>
        public PhysicalFileSystemResolver(IFileInfo fileInfo, in ImageMetaData metadata)
        {
            this.fileInfo = fileInfo;
            this.metadata = metadata;
        }

        /// <inheritdoc/>
        public Task<ImageMetaData> GetMetaDataAsync() => Task.FromResult(this.metadata);

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => Task.FromResult(this.fileInfo.CreateReadStream());
    }
}