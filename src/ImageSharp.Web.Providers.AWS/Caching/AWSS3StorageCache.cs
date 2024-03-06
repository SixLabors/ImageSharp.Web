// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Resolvers.AWS;

namespace SixLabors.ImageSharp.Web.Caching.AWS;

/// <summary>
/// Implements an AWS S3 Storage based cache.
/// </summary>
public class AWSS3StorageCache : IImageCache
{
    private readonly IAmazonS3 amazonS3Client;
    private readonly string bucketName;
    private readonly string cacheFolder;

    /// <summary>
    /// Initializes a new instance of the <see cref="AWSS3StorageCache"/> class.
    /// </summary>
    /// <param name="cacheOptions">The cache options.</param>
    public AWSS3StorageCache(IOptions<AWSS3StorageCacheOptions> cacheOptions)
    {
        Guard.NotNull(cacheOptions, nameof(cacheOptions));
        AWSS3StorageCacheOptions options = cacheOptions.Value;
        this.bucketName = options.BucketName;
        this.amazonS3Client = AmazonS3ClientFactory.CreateClient(options);
        this.cacheFolder = string.IsNullOrEmpty(options.CacheFolder)
            ? string.Empty
            : options.CacheFolder.Trim().Trim('/') + '/';
    }

    /// <inheritdoc/>
    public async Task<IImageCacheResolver?> GetAsync(string key)
    {
        string keyWithFolder = this.GetKeyWithFolder(key);
        GetObjectMetadataRequest request = new() { BucketName = this.bucketName, Key = keyWithFolder };
        try
        {
            // HEAD request throws a 404 if not found.
            MetadataCollection metadata = (await this.amazonS3Client.GetObjectMetadataAsync(request)).Metadata;
            return new AWSS3StorageCacheResolver(this.amazonS3Client, this.bucketName, keyWithFolder, metadata);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata)
    {
        PutObjectRequest request = new()
        {
            BucketName = this.bucketName,
            Key = this.GetKeyWithFolder(key),
            ContentType = metadata.ContentType,
            InputStream = stream,
            AutoCloseStream = false
        };

        foreach (KeyValuePair<string, string> d in metadata.ToDictionary())
        {
            request.Metadata.Add(d.Key, d.Value);
        }

        return this.amazonS3Client.PutObjectAsync(request);
    }

    /// <summary>
    /// Creates a new bucket under the specified account if a bucket
    /// with the same name does not already exist.
    /// </summary>
    /// <param name="options">The AWS S3 Storage cache options.</param>
    /// <param name="acl">
    /// Specifies whether data in the bucket may be accessed publicly and the level of access.
    /// <see cref="S3CannedACL.PublicRead"/> specifies full public read access for bucket
    /// and object data. <see cref="S3CannedACL.Private"/> specifies that the bucket
    /// data is private to the account owner.
    /// </param>
    /// <returns>
    /// If the bucket does not already exist, a <see cref="PutBucketResponse"/> describing the newly
    /// created bucket. If the container already exists, <see langword="null"/>.
    /// </returns>
    public static PutBucketResponse? CreateIfNotExists(
        AWSS3StorageCacheOptions options,
        S3CannedACL acl)
        => AsyncHelper.RunSync(() => CreateIfNotExistsAsync(options, acl));

    private static async Task<PutBucketResponse?> CreateIfNotExistsAsync(
        AWSS3StorageCacheOptions options,
        S3CannedACL acl)
    {
        AmazonS3Client client = AmazonS3ClientFactory.CreateClient(options);

        bool foundBucket = false;
        ListBucketsResponse listBucketsResponse = await client.ListBucketsAsync();
        foreach (S3Bucket b in listBucketsResponse.Buckets)
        {
            if (b.BucketName == options.BucketName)
            {
                foundBucket = true;
                break;
            }
        }

        if (!foundBucket)
        {
            PutBucketRequest putBucketRequest = new()
            {
                BucketName = options.BucketName,
                BucketRegion = options.Region,
                CannedACL = acl
            };

            return await client.PutBucketAsync(putBucketRequest);
        }

        return null;
    }

    private string GetKeyWithFolder(string key)
        => this.cacheFolder + key;

    /// <summary>
    /// <see href="https://github.com/aspnet/AspNetIdentity/blob/b7826741279450c58b230ece98bd04b4815beabf/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs"/>
    /// </summary>
    private static class AsyncHelper
    {
        private static readonly TaskFactory TaskFactory
            = new(
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        /// <summary>
        /// Executes an async <see cref="Task"/> method synchronously.
        /// </summary>
        /// <param name="task">The task to excecute.</param>
        public static void RunSync(Func<Task> task)
        {
            CultureInfo cultureUi = CultureInfo.CurrentUICulture;
            CultureInfo culture = CultureInfo.CurrentCulture;
            TaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return task();
            }).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes an async <see cref="Task{TResult}"/> method which has
        /// a <paramref name="task"/> return type synchronously.
        /// </summary>
        /// <typeparam name="TResult">The type of result to return.</typeparam>
        /// <param name="task">The task to excecute.</param>
        /// <returns>The <typeparamref name="TResult"/>.</returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> task)
        {
            CultureInfo cultureUi = CultureInfo.CurrentUICulture;
            CultureInfo culture = CultureInfo.CurrentCulture;
            return TaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return task();
            }).Unwrap().GetAwaiter().GetResult();
        }
    }
}
