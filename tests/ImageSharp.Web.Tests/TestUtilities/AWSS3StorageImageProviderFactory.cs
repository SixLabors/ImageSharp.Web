// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Providers.AWS;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public static class AWSS3StorageImageProviderFactory
    {
        public static AWSS3StorageImageProvider Create(IServiceProvider services)
        {
            IOptions<AWSS3StorageImageProviderOptions> options = services.GetRequiredService<IOptions<AWSS3StorageImageProviderOptions>>();
            FormatUtilities utilities = services.GetRequiredService<FormatUtilities>();
            AsyncHelper.RunSync(() => InitializeAWSStorageAsync(services, options.Value));

            return new AWSS3StorageImageProvider(options, utilities);
        }

        private static async Task InitializeAWSStorageAsync(IServiceProvider services, AWSS3StorageImageProviderOptions options)
        {
            // Upload an image to the AWS Test Storage;
            AWSS3BucketClientOptions bucketOptions = options.S3Buckets.First();
            AmazonS3Client amazonS3Client = AmazonS3ClientFactory.CreateClient(bucketOptions);
            ListBucketsResponse listBucketsResponse = await amazonS3Client.ListBucketsAsync();

            bool foundBucket = false;
            foreach (S3Bucket b in listBucketsResponse.Buckets)
            {
                if (b.BucketName == bucketOptions.BucketName)
                {
                    foundBucket = true;
                    break;
                }
            }

            if (!foundBucket)
            {
                try
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketOptions.BucketName,
                        BucketRegion = bucketOptions.Region,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    await amazonS3Client.PutBucketAsync(putBucketRequest);
                }
                catch (AmazonS3Exception e)
                {
                    // CI tests are run in parallel and can sometimes return a
                    // false negative for the existance of a bucket.
                    if (string.Equals(e.ErrorCode, "BucketAlreadyExists"))
                    {
                        return;
                    }

                    throw;
                }
            }

            IWebHostEnvironment environment = services.GetRequiredService<IWebHostEnvironment>();

            try
            {
                GetObjectRequest request = new()
                {
                    BucketName = bucketOptions.BucketName,
                    Key = TestConstants.ImagePath
                };

                await amazonS3Client.GetObjectAsync(request);
            }
            catch
            {
                IFileInfo file = environment.WebRootFileProvider.GetFileInfo(TestConstants.ImagePath);
                using Stream stream = file.CreateReadStream();

                // Set the max-age property so we get coverage for testing in our AWS provider.
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

                await amazonS3Client.PutObjectAsync(putRequest);
            }
        }
    }
}
