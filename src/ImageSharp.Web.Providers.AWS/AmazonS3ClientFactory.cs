// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace SixLabors.ImageSharp.Web;

internal static class AmazonS3ClientFactory
{
    /// <summary>
    /// Creates a new bucket under the specified account if a bucket
    /// with the same name does not already exist.
    /// </summary>
    /// <param name="options">The AWS S3 Storage cache options.</param>
    /// <returns>
    /// A new <see cref="AmazonS3Client"/>.
    /// </returns>
    /// <exception cref="ArgumentException">Invalid configuration.</exception>
    public static AmazonS3BucketClient CreateClient(IAWSS3BucketClientOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Endpoint))
        {
            // AccessKey can be empty.
            // AccessSecret can be empty.
            // PathStyle endpoint doesn't support AccelerateEndpoint.
            AmazonS3Config config = new() { ServiceURL = options.Endpoint, ForcePathStyle = true, AuthenticationRegion = options.Region };
            SetTimeout(config, options.Timeout);
            return new(options.BucketName, new AmazonS3Client(options.AccessKey, options.AccessSecret, config));
        }
        else if (!string.IsNullOrWhiteSpace(options.AccessKey))
        {
            // AccessSecret can be empty.
            Guard.NotNullOrWhiteSpace(options.Region, nameof(options.Region));
            RegionEndpoint region = RegionEndpoint.GetBySystemName(options.Region);
            AmazonS3Config config = new() { RegionEndpoint = region, UseAccelerateEndpoint = options.UseAccelerateEndpoint };
            SetTimeout(config, options.Timeout);
            return new(options.BucketName, new AmazonS3Client(options.AccessKey, options.AccessSecret, config));
        }
        else if (!string.IsNullOrWhiteSpace(options.Region))
        {
            RegionEndpoint region = RegionEndpoint.GetBySystemName(options.Region);
            AmazonS3Config config = new() { RegionEndpoint = region, UseAccelerateEndpoint = options.UseAccelerateEndpoint };
            SetTimeout(config, options.Timeout);
            return new(options.BucketName, new AmazonS3Client(config));
        }
        else
        {
            throw new ArgumentException("Invalid configuration.", nameof(options));
        }
    }

    private static void SetTimeout(ClientConfig config, TimeSpan? timeout)
    {
        // We don't want to override the default timeout if it's not set.
        if (timeout.HasValue)
        {
            config.Timeout = timeout.Value;
        }
    }
}
