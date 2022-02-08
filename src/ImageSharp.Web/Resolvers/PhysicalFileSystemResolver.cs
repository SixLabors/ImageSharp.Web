// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Threading.Tasks;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Provides means to manage image buffers within the physical file system.
    /// </summary>
    public class PhysicalFileSystemResolver : IImageResolver
    {
        private readonly FileInfo fileInfo;
        private readonly ImageMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemResolver"/> class.
        /// </summary>
        /// <param name="fileInfo">The input file info.</param>
        /// <param name="metadata">The image metadata associated with this file.</param>
        public PhysicalFileSystemResolver(FileInfo fileInfo, in ImageMetadata metadata)
        {
            this.fileInfo = fileInfo;
            this.metadata = metadata;
        }

        /// <inheritdoc/>
        public Task<ImageMetadata> GetMetaDataAsync() => Task.FromResult(this.metadata);

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => Task.FromResult<Stream>(this.fileInfo.OpenRead());
    }
}
