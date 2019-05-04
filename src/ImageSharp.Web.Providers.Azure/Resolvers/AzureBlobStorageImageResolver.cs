// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Provides means to manage image buffers within the Azure Blob file system.
    /// </summary>
    public class AzureBlobStorageImageResolver : IImageResolver
    {
        private readonly CloudBlob blob;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageImageResolver"/> class.
        /// </summary>
        /// <param name="blob">The Azure blob.</param>
        public AzureBlobStorageImageResolver(CloudBlob blob) => this.blob = blob;

        /// <inheritdoc/>
        public async Task<ImageMetaData> GetMetaDataAsync()
        {
            await this.blob.FetchAttributesAsync().ConfigureAwait(false);
            return new ImageMetaData(this.blob.Properties?.LastModified?.DateTime ?? DateTime.UtcNow);
        }

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => this.blob.OpenReadAsync();
    }
}