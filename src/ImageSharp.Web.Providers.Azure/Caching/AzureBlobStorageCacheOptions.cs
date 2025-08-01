// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Azure.Storage.Blobs;

namespace SixLabors.ImageSharp.Web.Caching.Azure;

/// <summary>
/// Configuration options for the <see cref="AzureBlobStorageCache"/>.
/// </summary>
public class AzureBlobStorageCacheOptions
{
    /// <summary>
    /// Gets or sets a factory method to create an <see cref="BlobContainerClient"/>.
    /// </summary>
    public Func<AzureBlobStorageCacheOptions, IServiceProvider, BlobContainerClient>? BlobContainerClientFactory { get; set; }

    /// <summary>
    /// Gets or sets the Azure Blob Storage connection string.
    /// <see href="https://docs.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string."/>
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Azure Blob Storage container name.
    /// Must conform to Azure Blob Storage container naming guidelines.
    /// <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata#container-names"/>
    /// </summary>
    public string ContainerName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the cache folder's name that'll store cache files under the configured container.
    /// Must conform to Azure Blob Storage directory naming guidelines.
    /// <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata#directory-names"/>
    /// </summary>
    public string? CacheFolder { get; set; }
}
