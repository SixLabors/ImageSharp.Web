// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
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
        public AWSS3StorageCache(IOptions<AWSS3StorageCacheOptions> cacheOptions)
        {
            Guard.NotNull(cacheOptions, nameof(cacheOptions));
            AWSS3StorageCacheOptions options = cacheOptions.Value;
            this.bucket = options.BucketName;

            if (!string.IsNullOrEmpty(options.Endpoint) &&
                options.AccessKey != null &&
                options.AccessSecret != null)
            {
                var config = new AmazonS3Config { ServiceURL = options.Endpoint, ForcePathStyle = true };
                this.amazonS3Client = new AmazonS3Client(options.AccessKey, options.AccessSecret, config);
            }
            else if (!string.IsNullOrEmpty(options.AccessKey) &&
                     !string.IsNullOrEmpty(options.AccessSecret) &&
                     !string.IsNullOrEmpty(options.Region))
            {
                var region = RegionEndpoint.GetBySystemName(options.Region);
                this.amazonS3Client = new AmazonS3Client(options.AccessKey, options.AccessSecret, region);
            }
            else
            {
                var region = RegionEndpoint.GetBySystemName(options.Region);
                this.amazonS3Client = new AmazonS3Client(region);
            }
        }

        /// <inheritdoc/>
        public async Task<IImageCacheResolver> GetAsync(string key)
        {
            var request = new GetObjectMetadataRequest() { BucketName = this.bucket, Key = key };

            try
            {
                await this.amazonS3Client.GetObjectMetadataAsync(request);

                return new AWSS3StorageCacheResolver(this.amazonS3Client, this.bucket, key);
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

            var dt = metadata.ToDictionary();

            foreach (KeyValuePair<string, string> d in dt)
            {
                request.Metadata.Add(d.Key, d.Value);
            }

            return this.amazonS3Client.PutObjectAsync(request);
        }

        /// <summary>
        /// Creates a new container under the specified account if a container
        /// with the same name does not already exist.
        /// </summary>
        /// <param name="options">The Azure Blob Storage cache options.</param>
        /// <param name="acl">
        /// Specifies whether data in the bucket may be accessed publicly and the level of access.
        /// <see cref="S3CannedACL.PublicRead"/> specifies full public read access for bucket
        /// and object data. <see cref="S3CannedACL.Private"/> specifies that the container
        /// data is private to the account owner.
        /// </param>
        /// <returns>
        /// If the container does not already exist, a <see cref="PutBucketResponse"/> describing the newly
        /// created container. If the container already exists, <see langword="null"/>.
        /// </returns>
        public static PutBucketResponse CreateIfNotExists(
            AWSS3StorageCacheOptions options,
            S3CannedACL acl)
        {
            AmazonS3Client amazonS3Client;
            bool foundBucket = false;

            if (!string.IsNullOrEmpty(options.Endpoint) &&
                options.AccessKey != null &&
                options.AccessSecret != null)
            {
                var config = new AmazonS3Config { ServiceURL = options.Endpoint, ForcePathStyle = true };
                amazonS3Client = new AmazonS3Client(options.AccessKey, options.AccessSecret, config);
            }
            else if (!string.IsNullOrEmpty(options.AccessKey) &&
                     !string.IsNullOrEmpty(options.AccessSecret))
            {
                amazonS3Client = new AmazonS3Client(options.AccessKey, options.AccessSecret);
            }
            else
            {
                amazonS3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(options.Region));
            }

            ListBucketsResponse listBucketsResponse = amazonS3Client.ListBucketsAsync().GetAwaiter().GetResult();

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

                PutBucketResponse putBucketResponse =
                    amazonS3Client.PutBucketAsync(putBucketRequest).GetAwaiter().GetResult();

                return putBucketResponse;
            }

            return null;
        }
    }
}
