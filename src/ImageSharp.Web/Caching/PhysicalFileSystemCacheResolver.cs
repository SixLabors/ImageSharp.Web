// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Provides means to manage image buffers within the <see cref="PhysicalFileSystemCache"/>.
    /// </summary>
    public class PhysicalFileSystemCacheResolver : IImageCacheResolver
    {
        private readonly IFileInfo fileInfo;
        private readonly ImageCacheMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemCacheResolver"/> class.
        /// </summary>
        /// <param name="fileInfo">The input file info.</param>
        /// <param name="metadata">The image metadata associated with this file.</param>
        public PhysicalFileSystemCacheResolver(IFileInfo fileInfo, in ImageCacheMetadata metadata)
        {
            this.fileInfo = fileInfo;
            this.metadata = metadata;
        }

        /// <inheritdoc/>
        public Task<ImageCacheMetadata> GetMetaDataAsync() => Task.FromResult(this.metadata);

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => Task.FromResult(this.fileInfo.CreateReadStream());
    }
}
