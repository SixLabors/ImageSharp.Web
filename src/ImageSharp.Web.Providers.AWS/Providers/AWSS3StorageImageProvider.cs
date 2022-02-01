// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Resolvers.AWS;

namespace SixLabors.ImageSharp.Web.Providers.AWS
{
    /// <summary>
    /// Returns images stored in AWS S3.
    /// </summary>
    public class AWSS3StorageImageProvider : IImageProvider
    {
        /// <summary>
        /// Character array to remove from paths.
        /// </summary>
        private static readonly char[] SlashChars = { '\\', '/' };

        /// <summary>
        /// The containers for the blob services.
        /// </summary>
        private readonly Dictionary<string, AmazonS3Client> buckets
            = new();

        private readonly AWSS3StorageImageProviderOptions storageOptions;
        private Func<HttpContext, bool> match;

        /// <summary>
        /// Contains various helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities formatUtilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSS3StorageImageProvider"/> class.
        /// </summary>
        /// <param name="storageOptions">The S3 storage options</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public AWSS3StorageImageProvider(IOptions<AWSS3StorageImageProviderOptions> storageOptions, FormatUtilities formatUtilities)
        {
            Guard.NotNull(storageOptions, nameof(storageOptions));

            this.storageOptions = storageOptions.Value;

            this.formatUtilities = formatUtilities;

            foreach (AWSS3BucketClientOptions bucket in this.storageOptions.S3Buckets)
            {
                this.buckets.Add(bucket.BucketName, AmazonS3ClientFactory.CreateClient(bucket));
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
        public async Task<IImageResolver> GetAsync(HttpContext context)
        {
            // Strip the leading slash and bucket name from the HTTP request path and treat
            // the remaining path string as the key.
            // Path has already been correctly parsed before here.
            string bucketName = string.Empty;
            IAmazonS3 s3Client = null;

            // We want an exact match here to ensure that bucket names starting with
            // the same prefix are not mixed up.
            string path = context.Request.Path.Value.TrimStart(SlashChars);
            int index = path.IndexOfAny(SlashChars);
            string nameToMatch = index != -1 ? path.Substring(0, index) : path;

            foreach (string k in this.buckets.Keys)
            {
                if (nameToMatch.Equals(k, StringComparison.OrdinalIgnoreCase))
                {
                    bucketName = k;
                    s3Client = this.buckets[k];
                    break;
                }
            }

            // Something has gone horribly wrong for this to happen but check anyway.
            if (s3Client is null)
            {
                return null;
            }

            // Key should be the remaining path string.
            string key = path.Substring(bucketName.Length).TrimStart(SlashChars);

            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            if (!await this.KeyExists(s3Client, bucketName, key))
            {
                return null;
            }

            return new AWSS3StorageImageResolver(s3Client, bucketName, key);
        }

        private bool IsMatch(HttpContext context)
        {
            // Only match loosly here for performance.
            // Path matching conflicts should be dealt with by configuration.
            string path = context.Request.Path.Value.TrimStart(SlashChars);
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
        private async Task<bool> KeyExists(IAmazonS3 s3Client, string bucketName, string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                // If the object doesn't exist then a "NotFound" will be thrown
                await s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception e)
            {
                if (string.Equals(e.ErrorCode, "NoSuchBucket"))
                {
                    return false;
                }

                if (string.Equals(e.ErrorCode, "NotFound"))
                {
                    return false;
                }

                throw;
            }
        }
    }
}
