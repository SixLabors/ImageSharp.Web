// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Provides means to manage image buffers from an <see cref="IImageInfo"/> instance.
    /// </summary>
    public class FileInfoImageResolver : IImageResolver
    {
        private readonly IFileInfo fileInfo;
        private readonly ImageMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfoImageResolver"/> class.
        /// </summary>
        /// <param name="fileInfo">The input file info.</param>
        /// <param name="metadata">The image metadata associated with this file.</param>
        public FileInfoImageResolver(IFileInfo fileInfo, in ImageMetadata metadata)
        {
            this.fileInfo = fileInfo;
            this.metadata = metadata;
        }

        /// <inheritdoc/>
        public Task<ImageMetadata> GetMetaDataAsync() => Task.FromResult(this.metadata);

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => Task.FromResult(this.fileInfo.CreateReadStream());
    }
}
