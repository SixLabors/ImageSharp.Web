// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Resolvers.AWS;

namespace SixLabors.ImageSharp.Web.Caching.AWS
{

    /// <summary>
    /// Implements an Azure Blob Storage based cache.
    /// </summary>
    public class AWSS3StorageCache : IImageCache
    {
        private readonly IAmazonS3 amazonS3Client;
        private readonly String bucket;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageCache"/> class.
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
                     !string.IsNullOrEmpty(options.AccessSecret)&&
                     !string.IsNullOrEmpty(options.Region))
            {
                var region = RegionEndpoint.GetBySystemName(options.Region);
                amazonS3Client = new AmazonS3Client(options.AccessKey, options.AccessSecret, region);
            }
            else
            {
                var region = RegionEndpoint.GetBySystemName(options.Region);
                amazonS3Client = new AmazonS3Client(region);
            }
        }

        /// <inheritdoc/>
        public async Task<IImageCacheResolver> GetAsync(string key)
        {
            GetObjectMetadataRequest request = new GetObjectMetadataRequest() {BucketName = this.bucket, Key = key};

            try
            {
                await this.amazonS3Client.GetObjectMetadataAsync(request);

                return new AWSS3FileSystemCacheResolver(this.amazonS3Client, this.bucket, key);
            }
            catch (AmazonS3Exception exception)
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

            foreach (var d in dt)
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
        /// <param name="accessType">
        /// Optionally specifies whether data in the container may be accessed publicly and
        /// the level of access. <see cref="PublicAccessType.BlobContainer"/>
        /// specifies full public read access for container and blob data. Clients can enumerate
        /// blobs within the container via anonymous request, but cannot enumerate containers
        /// within the storage account. <see cref="Blob"/>
        /// specifies public read access for blobs. Blob data within this container can be
        /// read via anonymous request, but container data is not available. Clients cannot
        /// enumerate blobs within the container via anonymous request. <see cref="PublicAccessType.None"/>
        /// specifies that the container data is private to the account owner.
        /// </param>
        /// <returns>
        /// If the container does not already exist, a <see cref="Response{T}"/> describing the newly
        /// created container. If the container already exists, <see langword="null"/>.
        /// </returns>
        public static PutBucketResponse CreateIfNotExists(
            AWSS3StorageCacheOptions options,
            S3CannedACL acl)
        {
            AmazonS3Client amazonS3Client = null;
            bool foundBucket = false;

            if (!string.IsNullOrEmpty(options.Endpoint) &&
                options.AccessKey != null &&
                options.AccessSecret != null)
            {
                var config = new AmazonS3Config {ServiceURL = options.Endpoint, ForcePathStyle = true};
                amazonS3Client = new AmazonS3Client(options.AccessKey, options.AccessSecret, config);
            }
            else if (!string.IsNullOrEmpty(options.AccessKey) &&
                     !string.IsNullOrEmpty(options.AccessSecret))
            {
                amazonS3Client = new AmazonS3Client(options.AccessKey, options.AccessSecret);
            }
            else
            {
                amazonS3Client = new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(options.Region));
            }

            var listBucketsResponse = amazonS3Client.ListBucketsAsync().GetAwaiter().GetResult();

            foreach (var b in listBucketsResponse.Buckets)
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
                    CannedACL = S3CannedACL.PublicRead
                };

                PutBucketResponse putBucketResponse =
                    amazonS3Client.PutBucketAsync(putBucketRequest).GetAwaiter().GetResult();

                return putBucketResponse;
            }

            return null;
        }
    }
}
