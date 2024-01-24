// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching.AWS;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Providers.AWS;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities;

public class AWSS3StorageCacheCacheFolderTestServerFixture : TestServerFixture
{
    protected override void ConfigureCustomServices(IServiceCollection services, IImageSharpBuilder builder)
        => builder
        .Configure<AWSS3StorageImageProviderOptions>(o =>
            o.S3Buckets.Add(
            new AWSS3BucketClientOptions
            {
                Endpoint = TestConstants.AWSEndpoint,
                BucketName = TestConstants.AWSBucketName,
                AccessKey = TestConstants.AWSAccessKey,
                AccessSecret = TestConstants.AWSAccessSecret,
                Region = TestConstants.AWSRegion,
                Timeout = TestConstants.AWSTimeout,
            }))
        .AddProvider(AWSS3StorageImageProviderFactory.Create)
        .Configure<AWSS3StorageCacheOptions>(o =>
        {
            o.Endpoint = TestConstants.AWSEndpoint;
            o.BucketName = TestConstants.AWSCacheBucketName;
            o.AccessKey = TestConstants.AWSAccessKey;
            o.AccessSecret = TestConstants.AWSAccessSecret;
            o.Region = TestConstants.AWSRegion;
            o.Timeout = TestConstants.AWSTimeout;
            o.CacheFolder = TestConstants.AWSCacheFolder;

            AWSS3StorageCache.CreateIfNotExists(o, S3CannedACL.Private);
        })
        .SetCache<AWSS3StorageCache>();
}
