// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Resolvers.Azure;

namespace SixLabors.ImageSharp.Web.Caching.Azure
{
    /// <summary>
    /// Implements an Azure Blob Storage based cache.
    /// </summary>
    public class AzureBlobStorageCache : IImageCache
    {
        private readonly BlobContainerClient container;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageCache"/> class.
        /// </summary>
        /// <param name="cacheOptions">The cache options.</param>
        public AzureBlobStorageCache(IOptions<AzureBlobStorageCacheOptions> cacheOptions)
        {
            Guard.NotNull(cacheOptions, nameof(cacheOptions));
            AzureBlobStorageCacheOptions options = cacheOptions.Value;

            this.container = new BlobContainerClient(options.ConnectionString, options.ContainerName);
            this.container.CreateIfNotExistsAsync(PublicAccessType.None);
        }

        /// <inheritdoc/>
        public async Task<IImageCacheResolver> GetAsync(string key)
        {
            BlobClient blob = this.container.GetBlobClient(key);

            if (!await blob.ExistsAsync())
            {
                return null;
            }

            return new AzureBlobStorageCacheResolver(blob);
        }

        /// <inheritdoc/>
        public Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata)
        {
            BlobClient blob = this.container.GetBlobClient(key);

            var headers = new BlobHttpHeaders
            {
                ContentType = metadata.ContentType,
            };

            return blob.UploadAsync(stream, httpHeaders: headers, metadata: metadata.ToDictionary());
        }
    }
}
