// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Amazon.S3;

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Represents a scoped Amazon S3 client instance that is explicitly associated with a single S3 bucket.
/// This wrapper provides a strongly-typed link between the client and the bucket it operates on,
/// and optionally manages the lifetime of the underlying <see cref="AmazonS3Client"/>.
/// </summary>
public sealed class AmazonS3BucketClient : IDisposable
{
    private readonly bool disposeClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonS3BucketClient"/> class.
    /// </summary>
    /// <param name="bucketName">
    /// The bucket name associated with this client instance.
    /// </param>
    /// <param name="client">
    /// The underlying Amazon S3 client instance. This should be an already configured instance of <see cref="AmazonS3Client"/>.
    /// </param>
    /// <param name="disposeClient">
    /// A value indicating whether the underlying client should be disposed when this instance is disposed.
    /// </param>
    public AmazonS3BucketClient(string bucketName, AmazonS3Client client, bool disposeClient = true)
    {
        Guard.NotNullOrWhiteSpace(bucketName, nameof(bucketName));
        Guard.NotNull(client, nameof(client));
        this.BucketName = bucketName;
        this.Client = client;
        this.disposeClient = disposeClient;
    }

    /// <summary>
    /// Gets the bucket name associated with this client instance.
    /// </summary>
    public string BucketName { get; }

    /// <summary>
    /// Gets the underlying Amazon S3 client instance.
    /// </summary>
    public AmazonS3Client Client { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.disposeClient)
        {
            this.Client.Dispose();
        }
    }
}
