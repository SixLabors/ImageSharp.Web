// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using Amazon;
using Amazon.S3;

namespace SixLabors.ImageSharp.Web
{
    internal static class AmazonS3ClientFactory
    {
        /// <summary>
        /// Creates a new bucket under the specified account if a bucket
        /// with the same name does not already exist.
        /// </summary>
        /// <param name="options">The AWS S3 Storage cache options.</param>
        /// <returns>
        /// A new <see cref="AmazonS3Client"/>.
        /// </returns>
        public static AmazonS3Client CreateClient(IAWSS3BucketClientOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Endpoint))
            {
                // AccessKey can be empty.
                // AccessSecret can be empty.
                AmazonS3Config config = new() { ServiceURL = options.Endpoint, ForcePathStyle = true, AuthenticationRegion = options.Region };
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
