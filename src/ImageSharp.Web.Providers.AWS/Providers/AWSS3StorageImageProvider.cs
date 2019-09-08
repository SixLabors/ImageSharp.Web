// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AWSS3StorageImageProvider> logger;
        private readonly AWSS3StorageImageProviderOptions storageOptions;
        private Func<HttpContext, bool> match;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSS3StorageImageProvider"/> class.
        /// </summary>
        /// <param name="amazonS3Client">Amazon S3 client</param>
        /// <param name="logger">Microsoft.Extensions.Logging ILogger</param>
        /// <param name="storageOptions">The S3 storage options</param>
        public AWSS3StorageImageProvider(IAmazonS3 amazonS3Client, ILogger<AWSS3StorageImageProvider> logger, IOptions<AWSS3StorageImageProviderOptions> storageOptions)
        {
            Guard.NotNull(storageOptions, nameof(storageOptions));

            this.amazonS3Client = amazonS3Client;
            this.logger = logger;
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
        {
            var displayUrl = context.Request.Path;
            return Path.GetExtension(displayUrl).EndsWith(".jpg") || Path.GetExtension(displayUrl).EndsWith(".png");
        }

        /// <inheritdoc />
        public async Task<IImageResolver> GetAsync(HttpContext context)
        {
            PathString displayUrl = context.Request.Path;
            this.logger.LogDebug("Getting image for {ImageUri}", displayUrl);
            string imageId = Path.GetFileNameWithoutExtension(displayUrl);
            this.logger.LogDebug("Image id is {ImageId}", imageId);

            string imagePath = $"{imageId}.png";

            bool imageExists = await this.KeyExists(this.storageOptions.BucketName, imagePath);

            if (!imageExists)
            {
                this.logger.LogDebug("No image found for {ImageId}", imageId);
                return null;
            }

            this.logger.LogDebug("Found image {ImageId}", imageId);

            return new AWSS3FileSystemResolver(this.amazonS3Client, this.storageOptions.BucketName, imagePath);
        }

        private bool IsMatch(HttpContext context)
        {
            string path = context.Request.Path.Value.TrimStart(SlashChars);
            return path.StartsWith(this.storageOptions.BucketName, StringComparison.OrdinalIgnoreCase);
        }

        // ref https://github.com/aws/aws-sdk-net/blob/master/sdk/src/Services/S3/Custom/_bcl/IO/S3FileInfo.cs#L118
        private async Task<bool> KeyExists(string bucketName, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.logger.LogDebug("Checking for the existence of key {Key} in bucket {BucketName}", key, bucketName);

            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                ((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request)
                    .AddBeforeRequestHandler(FileIORequestEventHandler);

                // If the object doesn't exist then a "NotFound" will be thrown
                await this.amazonS3Client.GetObjectMetadataAsync(request, cancellationToken).ConfigureAwait(false);
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

        private static void FileIORequestEventHandler(object sender, RequestEventArgs args)
        {
            if (args is WebServiceRequestEventArgs wsArgs)
            {
                string currentUserAgent = wsArgs.Headers[AWSSDKUtils.UserAgentHeader];
                wsArgs.Headers[AWSSDKUtils.UserAgentHeader] = currentUserAgent + " FileIO";
            }
        }
    }
}
