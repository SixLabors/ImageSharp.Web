// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Providers
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

        private readonly IAmazonS3 amazonS3Client;
        private readonly AWSS3StorageImageProviderOptions storageOptions;
        private Func<HttpContext, bool> match;

        /// <summary>
        /// Contains various helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities formatUtilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSS3StorageImageProvider"/> class.
        /// </summary>
        /// <param name="amazonS3Client">Amazon S3 client</param>
        /// <param name="storageOptions">The S3 storage options</param>
        public AWSS3StorageImageProvider(IAmazonS3 amazonS3Client, IOptions<AWSS3StorageImageProviderOptions> storageOptions, FormatUtilities formatUtilities)
        {
            Guard.NotNull(storageOptions, nameof(storageOptions));

            this.amazonS3Client = amazonS3Client;
            this.formatUtilities = formatUtilities;
            this.storageOptions = storageOptions.Value;
        }

        /// <inheritdoc />
        public Func<HttpContext, bool> Match
        {
            get => this.match ?? this.IsMatch;
            set => this.match = value;
        }

        /// <inheritdoc />
        public bool IsValidRequest(HttpContext context)
            => this.formatUtilities.GetExtensionFromUri(context.Request.GetDisplayUrl()) != null;

        /// <inheritdoc />
        public async Task<IImageResolver> GetAsync(HttpContext context)
        {
            // Strip the leading slash and bucket name from the HTTP request path and treat
            // the remaining path string as the file key.
            // Path has already been correctly parsed before here.
            string key = context.Request.Path.Value.TrimStart(SlashChars)
                            .Substring(this.storageOptions.BucketName.Length)
                            .TrimStart(SlashChars);

            bool imageExists = await this.KeyExists(this.storageOptions.BucketName, key);

            return !imageExists ? null : new AWSS3FileSystemResolver(this.amazonS3Client, this.storageOptions.BucketName, key);
        }

        private bool IsMatch(HttpContext context)
        {
            string path = context.Request.Path.Value.TrimStart(SlashChars);
            return path.StartsWith(this.storageOptions.BucketName, StringComparison.OrdinalIgnoreCase);
        }

        // ref https://github.com/aws/aws-sdk-net/blob/master/sdk/src/Services/S3/Custom/_bcl/IO/S3FileInfo.cs#L118
        private async Task<bool> KeyExists(string bucketName, string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                // If the object doesn't exist then a "NotFound" will be thrown
                await this.amazonS3Client.GetObjectMetadataAsync(request).ConfigureAwait(false);
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
