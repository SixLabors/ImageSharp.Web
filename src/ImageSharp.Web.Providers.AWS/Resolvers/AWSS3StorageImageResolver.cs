// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace SixLabors.ImageSharp.Web.Resolvers.AWS
{
    /// <summary>
    /// Provides means to manage image buffers within the AWS S3 file system.
    /// </summary>
    public class AWSS3StorageImageResolver : IImageResolver
    {
        private readonly IAmazonS3 amazonS3;
        private readonly string bucketName;
        private readonly string imagePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSS3StorageImageResolver"/> class.
        /// </summary>
        /// <param name="amazonS3">The Amazon S3 Client</param>
        /// <param name="bucketName">The bucket name.</param>
        /// <param name="imagePath">The image path.</param>
        public AWSS3StorageImageResolver(IAmazonS3 amazonS3, string bucketName, string imagePath)
        {
            this.amazonS3 = amazonS3;
            this.bucketName = bucketName;
            this.imagePath = imagePath;
        }

        /// <inheritdoc />
        public async Task<ImageMetadata> GetMetaDataAsync()
        {
            GetObjectMetadataResponse metadata = await this.amazonS3.GetObjectMetadataAsync(this.bucketName, this.imagePath);

            // Try to parse the max age from the source. If it's not zero then we pass it along
            // to set the cache control headers for the response.
            TimeSpan maxAge = TimeSpan.MinValue;
            if (CacheControlHeaderValue.TryParse(metadata.Headers.CacheControl, out CacheControlHeaderValue cacheControl))
            {
                // Weirdly passing null to TryParse returns true.
                if (cacheControl?.MaxAge.HasValue == true)
                {
                    maxAge = cacheControl.MaxAge.Value;
                }
            }

            return new ImageMetadata(metadata.LastModified, maxAge, metadata.ContentLength);
        }

        /// <inheritdoc />
        public Task<Stream> OpenReadAsync() => this.amazonS3.GetObjectStreamAsync(this.bucketName, this.imagePath, null);
    }
}
