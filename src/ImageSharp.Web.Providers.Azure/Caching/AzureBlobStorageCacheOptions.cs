// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Caching.Azure;

/// <summary>
/// Configuration options for the <see cref="AzureBlobStorageCache"/>.
/// </summary>
public class AzureBlobStorageCacheOptions
{
    /// <inheritdoc/>
    public string ConnectionString { get; set; } = null!;

    /// <inheritdoc/>
    public string ContainerName { get; set; } = null!;
}
