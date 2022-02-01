// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Providers.AWS;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Resolvers.AWS;

namespace SixLabors.ImageSharp.Web.Caching.AWS
{
    /// <summary>
    /// Implements an AWS S3 Storage based cache.
    /// </summary>
    public class AWSS3StorageCache : IImageCache
    {
        private readonly IAmazonS3 amazonS3Client;
        private readonly string bucket;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSS3StorageCache"/> class.
        /// </summary>
        /// <param name="cacheOptions">The cache options.</param>
        public AWSS3StorageCache(IOptions<AWSS3BucketClientOptions> cacheOptions)
        {
            Guard.NotNull(cacheOptions, nameof(cacheOptions));
            AWSS3BucketClientOptions options = cacheOptions.Value;
            this.bucket = options.BucketName;
            this.amazonS3Client = AmazonS3ClientFactory.CreateClient(options);
        }

        /// <inheritdoc/>
        public async Task<IImageCacheResolver> GetAsync(string key)
        {
            GetObjectMetadataRequest request = new() { BucketName = this.bucket, Key = key };
            try
            {
                // HEAD request throws a 404 if not found.
                MetadataCollection metadata = (await this.amazonS3Client.GetObjectMetadataAsync(request)).Metadata;
                return new AWSS3StorageCacheResolver(this.amazonS3Client, this.bucket, key, metadata);
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata)
        {
            var request = new PutObjectRequest()
            {
                BucketName = this.bucket,
                Key = key,
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
        public static PutBucketResponse CreateIfNotExists(
            AWSS3BucketClientOptions options,
            S3CannedACL acl)
            => AsyncHelper.RunSync(() => CreateIfNotExistsAsync(options, acl));

        private static async Task<PutBucketResponse> CreateIfNotExistsAsync(
            AWSS3BucketClientOptions options,
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
                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = options.BucketName,
                    BucketRegion = options.Region,
                    CannedACL = acl
                };

                return await client.PutBucketAsync(putBucketRequest);
            }

            return null;
        }
    }
}
