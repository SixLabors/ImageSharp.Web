// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Resolvers.Azure;

namespace SixLabors.ImageSharp.Web.Caching.Azure;

/// <summary>
/// Implements an Azure Blob Storage based cache.
/// </summary>
public class AzureBlobStorageCache : IImageCache
{
    private readonly BlobContainerClient container;
    private readonly string cacheFolder;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobStorageCache"/> class.
    /// </summary>
    /// <param name="cacheOptions">The cache options.</param>
    /// <param name="serviceProvider">The current service provider.</param>
    public AzureBlobStorageCache(IOptions<AzureBlobStorageCacheOptions> cacheOptions, IServiceProvider serviceProvider)
    {
        Guard.NotNull(cacheOptions, nameof(cacheOptions));
        AzureBlobStorageCacheOptions options = cacheOptions.Value;

        this.container =
            options.BlobContainerClientFactory?.Invoke(options, serviceProvider)
            ?? new BlobContainerClient(options.ConnectionString, options.ContainerName);

        this.cacheFolder = string.IsNullOrWhiteSpace(options.CacheFolder)
            ? string.Empty
            : options.CacheFolder.Trim().Trim('/') + '/';
    }

    /// <inheritdoc/>
    public async Task<IImageCacheResolver?> GetAsync(string key)
    {
        BlobClient blob = this.GetBlob(key);

        if (!await blob.ExistsAsync())
        {
            return null;
        }

        return new AzureBlobStorageCacheResolver(blob);
    }

    /// <inheritdoc/>
    public Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata)
    {
        BlobClient blob = this.GetBlob(key);

        BlobHttpHeaders headers = new()
        {
            ContentType = metadata.ContentType,
        };

        return blob.UploadAsync(stream, httpHeaders: headers, metadata: metadata.ToDictionary());
    }

    /// <summary>
    /// Creates a new container under the specified account if a container
    /// with the same name does not already exist.
    /// </summary>
    /// <param name="options">The Azure Blob Storage cache options.</param>
    /// <param name="accessType">
    /// Optionally specifies whether data in the container may be accessed publicly and
    /// the level of access. <see cref="PublicAccessType.BlobContainer"/>
    /// specifies full public read access for container and blob data. Clients can enumerate
    /// blobs within the container via anonymous request, but cannot enumerate containers
    /// within the storage account. <see cref="PublicAccessType.Blob"/>
    /// specifies public read access for blobs. Blob data within this container can be
    /// read via anonymous request, but container data is not available. Clients cannot
    /// enumerate blobs within the container via anonymous request. <see cref="PublicAccessType.None"/>
    /// specifies that the container data is private to the account owner.
    /// </param>
    /// <returns>
    /// If the container does not already exist, a <see cref="Response{T}"/> describing the newly
    /// created container. If the container already exists, <see langword="null"/>.
    /// </returns>
    public static Response<BlobContainerInfo> CreateIfNotExists(
        AzureBlobStorageCacheOptions options,
        PublicAccessType accessType)
        => new BlobContainerClient(options.ConnectionString, options.ContainerName).CreateIfNotExists(accessType);

    private BlobClient GetBlob(string key)
        => this.container.GetBlobClient(this.cacheFolder + key);
}
