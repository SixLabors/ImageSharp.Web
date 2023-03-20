// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Providers.AWS;

/// <summary>
/// Configuration options for the <see cref="AWSS3StorageImageProviderOptions"/> provider.
/// </summary>
public class AWSS3StorageImageProviderOptions
{
    /// <summary>
    /// Gets or sets the collection of blob container client options.
    /// </summary>
    public ICollection<AWSS3BucketClientOptions> S3Buckets { get; set; } = new HashSet<AWSS3BucketClientOptions>();
}

/// <summary>
/// Configuration options for the <see cref="AWSS3StorageImageProvider"/> provider.
/// </summary>
public class AWSS3BucketClientOptions : IAWSS3BucketClientOptions
{
    /// <inheritdoc/>
    public string? Region { get; init; }

    /// <inheritdoc/>
    public string BucketName { get; init; } = null!;

    /// <inheritdoc/>
    public string? AccessKey { get; init; }

    /// <inheritdoc/>
    public string? AccessSecret { get; init; }

    /// <inheritdoc/>
    public string? Endpoint { get; init; }

    /// <inheritdoc/>
    public bool UseAccelerateEndpoint { get; init; }

    /// <inheritdoc/>
    public TimeSpan? Timeout { get; init; }
}
