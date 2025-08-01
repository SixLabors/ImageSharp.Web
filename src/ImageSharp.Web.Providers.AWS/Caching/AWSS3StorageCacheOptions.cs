// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Caching.AWS;

/// <summary>
/// Configuration options for the <see cref="AWSS3StorageCache"/> provider.
/// </summary>
public class AWSS3StorageCacheOptions : IAWSS3BucketClientOptions
{
    /// <inheritdoc/>
    public Func<IAWSS3BucketClientOptions, IServiceProvider, AmazonS3BucketClient>? S3ClientFactory { get; set; }

    /// <inheritdoc/>
    public string? Region { get; set; }

    /// <inheritdoc/>
    public string BucketName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the cache folder's name that'll store cache files under the configured bucket.
    /// </summary>
    public string? CacheFolder { get; set; }

    /// <inheritdoc/>
    public string? AccessKey { get; set; }

    /// <inheritdoc/>
    public string? AccessSecret { get; set; }

    /// <inheritdoc/>
    public string? Endpoint { get; set; }

    /// <inheritdoc/>
    public bool UseAccelerateEndpoint { get; set; }

    /// <inheritdoc/>
    public TimeSpan? Timeout { get; set; }
}
