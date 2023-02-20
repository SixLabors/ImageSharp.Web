// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.
#nullable disable

namespace SixLabors.ImageSharp.Web.Caching.Azure;

/// <summary>
/// Configuration options for the <see cref="AzureBlobStorageCache"/>.
/// </summary>
public class AzureBlobStorageCacheOptions : IAzureBlobContainerClientOptions
{
    /// <inheritdoc/>
    public string ConnectionString { get; set; }

    /// <inheritdoc/>
    public string ContainerName { get; set; }
}
