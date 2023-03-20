// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Caching.AWS;

/// <summary>
/// Configuration options for the <see cref="AWSS3StorageCache"/> provider.
/// </summary>
public class AWSS3StorageCacheOptions : IAWSS3BucketClientOptions
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
