// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.Util;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Providers.AWS;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public static class AWSS3StorageImageProviderFactory
    {
        public static AWSS3StorageImageProvider Create(IServiceProvider services)
        {
            IOptions<AWSS3StorageImageProviderOptions> options = services.GetRequiredService<IOptions<AWSS3StorageImageProviderOptions>>();
            FormatUtilities utilities = services.GetRequiredService<FormatUtilities>();
            InitializeAWSStorage(services, options.Value);

            return new AWSS3StorageImageProvider(options, utilities);
        }

        private static void InitializeAWSStorage(IServiceProvider services, AWSS3StorageImageProviderOptions options)
        {
            // Upload an image to the AWS Test Storage;
            AWSS3BucketClientOptions bucketOptions = options.S3Buckets.First();
            AmazonS3Client amazonS3Client = null;
            bool foundBucket = false;

            if (!string.IsNullOrEmpty(bucketOptions.Endpoint) &&
                bucketOptions.AccessKey != null &&
                bucketOptions.AccessSecret != null)
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = bucketOptions.Endpoint,
                    ForcePathStyle = true
                };
                amazonS3Client = new AmazonS3Client("", "", config);
            }
            else if (!string.IsNullOrEmpty(bucketOptions.AccessKey) &&
                     !string.IsNullOrEmpty(bucketOptions.AccessSecret) &&
                     !string.IsNullOrEmpty(bucketOptions.Region))
            {
                var region = RegionEndpoint.GetBySystemName(bucketOptions.Region);
                amazonS3Client = new AmazonS3Client(bucketOptions.AccessKey, bucketOptions.AccessSecret, region);
            }
            else
            {
                var region = RegionEndpoint.GetBySystemName(bucketOptions.Region);
                amazonS3Client = new AmazonS3Client(region);
            }

            var listBucketsResponse = amazonS3Client.ListBucketsAsync().GetAwaiter().GetResult();

            foreach (var b in listBucketsResponse.Buckets)
            {
                if (b.BucketName == bucketOptions.BucketName)
                {
                    foundBucket = true;
                    break;
                }
            }

            if (!foundBucket)
            {
                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = bucketOptions.BucketName,
                    BucketRegion = bucketOptions.Region,
                    CannedACL = S3CannedACL.PublicRead
                };

                amazonS3Client.PutBucketAsync(putBucketRequest).GetAwaiter().GetResult();
            }

#if NETCOREAPP2_1
            IHostingEnvironment environment = services.GetRequiredService<IHostingEnvironment>();
#else
            IWebHostEnvironment environment = services.GetRequiredService<IWebHostEnvironment>();
#endif

            GetObjectRequest request = new GetObjectRequest()
            {
                BucketName = bucketOptions.BucketName,
                Key = TestConstants.ImagePath
            };

            try
            {
                amazonS3Client.GetObjectAsync(request).GetAwaiter().GetResult();
            }
            catch
            {
                IFileInfo file = environment.WebRootFileProvider.GetFileInfo(TestConstants.ImagePath);
                using Stream stream = file.CreateReadStream();

                // Set the max-age property so we get coverage for testing is in our Azure provider.
                var cacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromDays(7),
                    MustRevalidate = true
                };

                var putRequest = new PutObjectRequest()
                {
                    BucketName = bucketOptions.BucketName,
                    Key = TestConstants.ImagePath,
                    Headers =
                    {
                        CacheControl = cacheControl.ToString()
                    },
                    ContentType = "	image/png",
                    InputStream = stream
                };

                amazonS3Client.PutObjectAsync(putRequest).GetAwaiter().GetResult();
            }
        }
    }
}
