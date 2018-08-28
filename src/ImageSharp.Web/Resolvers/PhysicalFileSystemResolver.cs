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

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemResolver"/> class.
        /// </summary>
        /// <param name="fileInfo">The input file info.</param>
        public PhysicalFileSystemResolver(IFileInfo fileInfo) => this.fileInfo = fileInfo;

        /// <inheritdoc/>
        public Task<DateTime> GetLastWriteTimeUtcAsync() => Task.FromResult(this.fileInfo.LastModified.UtcDateTime);

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => Task.FromResult(this.fileInfo.CreateReadStream());

        /// <inheritdoc/>
        public Stream OpenWrite() => new MemoryStream(); // TODO: Chunked stream
    }
}