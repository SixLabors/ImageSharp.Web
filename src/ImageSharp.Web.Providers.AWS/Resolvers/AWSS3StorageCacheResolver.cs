// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Amazon.S3;
using Amazon.S3.Model;

namespace SixLabors.ImageSharp.Web.Resolvers.AWS;

/// <summary>
/// Provides means to manage image buffers within the <see cref="AWSS3StorageCacheResolver"/>.
/// </summary>
public class AWSS3StorageCacheResolver : IImageCacheResolver
{
    private readonly IAmazonS3 amazonS3;
    private readonly string bucketName;
    private readonly string imagePath;
    private readonly MetadataCollection metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="AWSS3StorageCacheResolver"/> class.
    /// </summary>
    /// <param name="amazonS3">The Amazon S3 Client</param>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="imagePath">The image path.</param>
    /// <param name="metadata">The metadata collection.</param>
    public AWSS3StorageCacheResolver(IAmazonS3 amazonS3, string bucketName, string imagePath, MetadataCollection metadata)
    {
        this.amazonS3 = amazonS3;
        this.bucketName = bucketName;
        this.imagePath = imagePath;
        this.metadata = metadata;
    }

    /// <inheritdoc/>
    public Task<ImageCacheMetadata> GetMetaDataAsync()
    {
        Dictionary<string, string> dict = new();
        foreach (string key in this.metadata.Keys)
        {
            // Trim automatically added x-amz-meta-
            dict.Add(key[11..].ToUpperInvariant(), this.metadata[key]);
        }

        return Task.FromResult(ImageCacheMetadata.FromDictionary(dict));
    }

    /// <inheritdoc/>
    public Task<Stream> OpenReadAsync() => this.amazonS3.GetObjectStreamAsync(this.bucketName, this.imagePath, null);
}
