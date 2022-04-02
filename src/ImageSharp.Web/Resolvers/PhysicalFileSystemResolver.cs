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

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemResolver"/> class.
        /// </summary>
        /// <param name="fileInfo">The input file info.</param>
        public PhysicalFileSystemResolver(FileInfo fileInfo) => this.fileInfo = fileInfo;

        /// <inheritdoc/>
        public Task<ImageMetadata> GetMetaDataAsync() => Task.FromResult(new ImageMetadata(this.fileInfo.LastWriteTimeUtc, this.fileInfo.Length));

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync()
        {
            // We are setting buffer size to 1 to prevent FileStream from allocating it's internal buffer
            // 0 causes constructor to throw
            int bufferSize = 1;
            return Task.FromResult<Stream>(new FileStream(
                this.fileInfo.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                bufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan));
        }
    }
}
