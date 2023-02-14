// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.
#nullable disable

namespace SixLabors.ImageSharp.Web.Providers.Azure;

/// <summary>
/// Configuration options for the <see cref="AzureBlobStorageImageProvider"/> provider.
/// </summary>
public class AzureBlobStorageImageProviderOptions
{
    /// <summary>
    /// Gets or sets the collection of blob container client options.
    /// </summary>
    public ICollection<AzureBlobContainerClientOptions> BlobContainers { get; set; } = new HashSet<AzureBlobContainerClientOptions>();
}

/// <summary>
/// Represents a single Azure Blob Storage connection and container.
/// </summary>
public class AzureBlobContainerClientOptions : IAzureBlobContainerClientOptions
{
    /// <inheritdoc/>
    public string ConnectionString { get; set; }

    /// <inheritdoc/>
    public string ContainerName { get; set; }
}
