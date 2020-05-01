// Copyright (c) Six Labors and contributors.
// Licensed under the GNU Affero General Public License, Version 3.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Provides means to manage image buffers within the physical file system cache.
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
