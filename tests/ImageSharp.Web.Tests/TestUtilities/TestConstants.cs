// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities;

public static class TestConstants
{
    public const string AzureConnectionString = "UseDevelopmentStorage=true";
    public const string AzureContainerName = "azure";
    public const string AzureCacheContainerName = "is-cache";
    public const string AzureCacheFolder = "cache/folder";
    public const string AWSEndpoint = "http://localhost:4568/";
    public const string AWSRegion = "eu-west-2";
    public const string AWSBucketName = "aws";
    public const string AWSCacheBucketName = "aws-cache";
    public const string AWSAccessKey = "";
    public const string AWSAccessSecret = "";
    public const string AWSCacheFolder = "cache/folder";
    public const string ImagePath = "SubFolder/sîxläbörs.îmägéshärp.wéb.png";
    public const string PhysicalTestImage = "http://localhost/" + ImagePath;
    public const string AzureTestImage = "http://localhost/" + AzureContainerName + "/" + ImagePath;
    public const string AWSTestImage = "http://localhost/" + AWSBucketName + "/" + ImagePath;
    public static readonly TimeSpan AWSTimeout = TimeSpan.FromSeconds(10);
}
