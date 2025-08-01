// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Resolvers.AWS;

namespace SixLabors.ImageSharp.Web.Providers.AWS;

/// <summary>
/// Returns images stored in AWS S3.
/// </summary>
public class AWSS3StorageImageProvider : IImageProvider, IDisposable
{
    /// <summary>
    /// Character array to remove from paths.
    /// </summary>
    private static readonly char[] SlashChars = { '\\', '/' };

    /// <summary>
    /// The containers for the blob services.
    /// </summary>
    private readonly Dictionary<string, AmazonS3BucketClient> buckets
        = new();

    private readonly AWSS3StorageImageProviderOptions storageOptions;
    private Func<HttpContext, bool>? match;
    private bool isDisposed;

    /// <summary>
    /// Contains various helper methods based on the current configuration.
    /// </summary>
    private readonly FormatUtilities formatUtilities;

    /// <summary>
    /// Initializes a new instance of the <see cref="AWSS3StorageImageProvider"/> class.
    /// </summary>
    /// <param name="storageOptions">The S3 storage options</param>
    /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
    /// <param name="serviceProvider">The current service provider.</param>
    public AWSS3StorageImageProvider(
        IOptions<AWSS3StorageImageProviderOptions> storageOptions,
        FormatUtilities formatUtilities,
        IServiceProvider serviceProvider)
    {
        Guard.NotNull(storageOptions, nameof(storageOptions));

        this.storageOptions = storageOptions.Value;

        this.formatUtilities = formatUtilities;

        foreach (AWSS3BucketClientOptions bucket in this.storageOptions.S3Buckets)
        {
            AmazonS3BucketClient s3Client =
                bucket.S3ClientFactory?.Invoke(bucket, serviceProvider)
                ?? AmazonS3ClientFactory.CreateClient(bucket);

            this.buckets.Add(s3Client.BucketName, s3Client);
        }
    }

    /// <inheritdoc/>
    public ProcessingBehavior ProcessingBehavior { get; } = ProcessingBehavior.All;

    /// <inheritdoc />
    public Func<HttpContext, bool> Match
    {
        get => this.match ?? this.IsMatch;
        set => this.match = value;
    }

    /// <inheritdoc />
    public bool IsValidRequest(HttpContext context)
        => this.formatUtilities.TryGetExtensionFromUri(context.Request.GetDisplayUrl(), out _);

    /// <inheritdoc />
    public async Task<IImageResolver?> GetAsync(HttpContext context)
    {
        // Strip the leading slash and bucket name from the HTTP request path and treat
        // the remaining path string as the key.
        // Path has already been correctly parsed before here.
        string bucketName = string.Empty;
        AmazonS3Client? s3Client = null;

        // We want an exact match here to ensure that bucket names starting with
        // the same prefix are not mixed up.
        string? path = context.Request.Path.Value?.TrimStart(SlashChars);

        if (path is null)
        {
            return null;
        }

        int index = path.IndexOfAny(SlashChars);
        string nameToMatch = index != -1 ? path[..index] : path;

        foreach (string k in this.buckets.Keys)
        {
            if (nameToMatch.Equals(k, StringComparison.OrdinalIgnoreCase))
            {
                bucketName = k;
                s3Client = this.buckets[k].Client;
                break;
            }
        }

        // Something has gone horribly wrong for this to happen but check anyway.
        if (s3Client is null)
        {
            return null;
        }

        // Key should be the remaining path string.
        string key = path[bucketName.Length..].TrimStart(SlashChars);

        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        KeyExistsResult keyExists = await KeyExists(s3Client, bucketName, key);
        if (!keyExists.Exists)
        {
            return null;
        }

        return new AWSS3StorageImageResolver(s3Client, bucketName, key, keyExists.Metadata);
    }

    private bool IsMatch(HttpContext context)
    {
        // Only match loosely here for performance.
        // Path matching conflicts should be dealt with by configuration.
        string? path = context.Request.Path.Value?.TrimStart(SlashChars);

        if (path is null)
        {
            return false;
        }

        foreach (string bucket in this.buckets.Keys)
        {
            if (path.StartsWith(bucket, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // ref https://github.com/aws/aws-sdk-net/blob/master/sdk/src/Services/S3/Custom/_bcl/IO/S3FileInfo.cs#L118
    private static async Task<KeyExistsResult> KeyExists(IAmazonS3 s3Client, string bucketName, string key)
    {
        try
        {
            GetObjectMetadataRequest request = new() { BucketName = bucketName, Key = key };

            // If the object doesn't exist then a "NotFound" will be thrown
            GetObjectMetadataResponse metadata = await s3Client.GetObjectMetadataAsync(request);
            return new KeyExistsResult(metadata);
        }
        catch (AmazonS3Exception e)
        {
            if (string.Equals(e.ErrorCode, "NoSuchBucket", StringComparison.Ordinal))
            {
                return default;
            }

            if (string.Equals(e.ErrorCode, "NotFound", StringComparison.Ordinal))
            {
                return default;
            }

            // If the object exists but the client is not authorized to access it, then a "Forbidden" will be thrown.
            if (string.Equals(e.ErrorCode, "Forbidden", StringComparison.Ordinal))
            {
                return default;
            }

            throw;
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="AWSS3StorageImageProvider"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                foreach (AmazonS3BucketClient client in this.buckets.Values)
                {
                    client?.Dispose();
                }

                this.buckets.Clear();
            }

            this.isDisposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);

        GC.SuppressFinalize(this);
    }

    private readonly record struct KeyExistsResult(GetObjectMetadataResponse Metadata)
    {
        public bool Exists => this.Metadata is not null;
    }
}
