// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Provides means to manage image buffers within the AWS S3 file system.
    /// </summary>
    public class AWSS3FileSystemResolver : IImageResolver
    {
        private readonly IAmazonS3 amazonS3;
        private readonly string bucketName;
        private readonly string imagePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSS3FileSystemResolver"/> class.
        /// </summary>
        /// <param name="amazonS3">Amazon S3 Client</param>
        /// <param name="bucketName">Bucket Name for where the files are</param>
        /// <param name="imagePath">S3 Key</param>
        public AWSS3FileSystemResolver(IAmazonS3 amazonS3, string bucketName, string imagePath)
        {
            this.amazonS3 = amazonS3;
            this.bucketName = bucketName;
            this.imagePath = imagePath;
        }

        /// <inheritdoc />
        public async Task<ImageMetadata> GetMetaDataAsync()
        {
            GetObjectMetadataResponse metadata = await this.amazonS3.GetObjectMetadataAsync(this.bucketName, this.imagePath);
            return new ImageMetadata(metadata.LastModified);
        }

        /// <inheritdoc />
        public async Task<Stream> OpenReadAsync()
        {
            GetObjectResponse s3Object = await this.amazonS3.GetObjectAsync(this.bucketName, this.imagePath);
            return s3Object.ResponseStream;
        }
    }
}
