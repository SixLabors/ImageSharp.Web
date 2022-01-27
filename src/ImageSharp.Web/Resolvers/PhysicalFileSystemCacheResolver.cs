// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Provides means to manage image buffers within the <see cref="PhysicalFileSystemCache"/>.
    /// </summary>
    public class PhysicalFileSystemCacheResolver : IImageCacheResolver
    {
        private readonly FileInfo metaFileInfo;
        private readonly FormatUtilities formatUtilities;
        private ImageCacheMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemCacheResolver"/> class.
        /// </summary>
        /// <param name="metaFileInfo">The cached metadata file info.</param>
        /// <param name="formatUtilities">
        /// Contains various format helper methods based on the current configuration.
        /// </param>
        public PhysicalFileSystemCacheResolver(FileInfo metaFileInfo, FormatUtilities formatUtilities)
        {
            this.metaFileInfo = metaFileInfo;
            this.formatUtilities = formatUtilities;
        }

        /// <inheritdoc/>
        public async Task<ImageCacheMetadata> GetMetaDataAsync()
        {
            using Stream stream = this.metaFileInfo.OpenRead();
            this.metadata = await ImageCacheMetadata.ReadAsync(stream);
            return this.metadata;
        }

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync()
        {
            string path = Path.ChangeExtension(
                this.metaFileInfo.FullName,
                this.formatUtilities.GetExtensionFromContentType(this.metadata.ContentType));

            return Task.FromResult<Stream>(File.OpenRead(path));
        }
    }
}
