// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SixLabors.ImageSharp.Web.Caching.Azure;

namespace SixLabors.ImageSharp.Web.Resolvers.Azure;

/// <summary>
/// Provides means to manage image buffers within the <see cref="AzureBlobStorageCache"/>.
/// </summary>
public class AzureBlobStorageCacheResolver : IImageCacheResolver
{
    private readonly BlobClient blob;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobStorageCacheResolver"/> class.
    /// </summary>
    /// <param name="blob">The Azure blob.</param>
    public AzureBlobStorageCacheResolver(BlobClient blob)
        => this.blob = blob;

    /// <inheritdoc/>
    public async Task<ImageCacheMetadata> GetMetaDataAsync()
    {
        // I've had a good read through the SDK source and I believe we cannot get
        // a 304 here since 'If-Modified-Since' header is not set by default.
        BlobProperties properties = await this.blob.GetPropertiesAsync();
        return ImageCacheMetadata.FromDictionary(properties.Metadata);
    }

    /// <inheritdoc/>
    public Task<Stream> OpenReadAsync() => this.blob.OpenReadAsync();
}
