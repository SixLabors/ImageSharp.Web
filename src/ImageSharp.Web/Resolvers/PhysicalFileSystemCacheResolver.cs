// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Runtime.CompilerServices;
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
            using Stream stream = OpenFileStream(this.metaFileInfo.FullName);
            if (this.metadata == default)
            {
                this.metadata = await ImageCacheMetadata.ReadAsync(stream);
            }

            return this.metadata;
        }

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync()
        {
            string path = Path.ChangeExtension(
                this.metaFileInfo.FullName,
                this.formatUtilities.GetExtensionFromContentType(this.metadata.ContentType));

            return Task.FromResult(OpenFileStream(path));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Stream OpenFileStream(string path)
        {
            // We are setting buffer size to 1 to prevent FileStream from allocating it's internal buffer
            // 0 causes constructor to throw
            const int bufferSize = 1;
            return new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                bufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
        }
    }
}
