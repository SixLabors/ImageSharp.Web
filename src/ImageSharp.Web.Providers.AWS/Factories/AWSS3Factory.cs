// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp.Web.Caching.AWS;
using SixLabors.ImageSharp.Web.Providers.AWS;

namespace SixLabors.ImageSharp.Web.Factories
{
    internal class AWSS3Factory
    {
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

        public static AmazonS3Client CreateClient(AWSS3BucketClientOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Endpoint))
            {
                // AccessKey can be empty.
                // AccessSecret can be empty.
                AmazonS3Config config = new() { ServiceURL = options.Endpoint, ForcePathStyle = true };
                return new AmazonS3Client(options.AccessKey, options.AccessSecret, config);
            }
            else if (!string.IsNullOrWhiteSpace(options.AccessKey))
            {
                // AccessSecret can be empty.
                Guard.NotNullOrWhiteSpace(options.Region, nameof(options.Region));
                var region = RegionEndpoint.GetBySystemName(options.Region);
                return new AmazonS3Client(options.AccessKey, options.AccessSecret, region);
            }
            else if (!string.IsNullOrWhiteSpace(options.Region))
            {
                var region = RegionEndpoint.GetBySystemName(options.Region);
                return new AmazonS3Client(region);
            }
            else
            {
                throw new ArgumentException("Invalid configuration.", nameof(options));
            }
        }
    }
}
