// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SixLabors.ImageSharp.Web.Resolvers.Azure
{
    /// <summary>
    /// Provides means to manage image buffers within the Azure Blob file system.
    /// </summary>
    public class AzureBlobStorageImageResolver : IImageResolver
    {
        private readonly BlobClient blob;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageImageResolver"/> class.
        /// </summary>
        /// <param name="blob">The Azure blob.</param>
        public AzureBlobStorageImageResolver(BlobClient blob) => this.blob = blob;

        /// <inheritdoc/>
        public async Task<ImageMetadata> GetMetaDataAsync()
        {
            Response<BlobProperties> properties = await this.blob.GetPropertiesAsync();
            return new ImageMetadata(properties?.Value.LastModified.DateTime ?? DateTime.UtcNow);
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenReadAsync()
        {
            Stream blobStream = (await this.blob.DownloadAsync()).Value.Content;
            var memoryStream = new ChunkedMemoryStream();

            await blobStream.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
    }
}
