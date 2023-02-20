// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Tests.Caching;

public class CacheKeyTests
{
    private static readonly ICacheKey LegacyV1CacheKey = new LegacyV1CacheKey();
    private static readonly ICacheKey UriRelativeCacheKey = new UriRelativeCacheKey();
    private static readonly ICacheKey UriAbsoluteCacheKey = new UriAbsoluteCacheKey();
    private static readonly ICacheKey UriRelativeLowerInvariantCacheKey = new UriRelativeLowerInvariantCacheKey();
    private static readonly ICacheKey UriAbsoluteLowerInvariantCacheKey = new UriAbsoluteLowerInvariantCacheKey();

    [Fact]
    public void UriRelativeCacheKey_Is_Relative()
    {
        HttpContext context = CreateHttpContext();
        var commands = new CommandCollection()
        {
            { "Width", "400" }
        };

        const string expected = "/Images/Image-12345.jpeg?width=400";
        string actual = UriRelativeCacheKey.Create(context, commands);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void UriRelativeLowerInvariantCacheKey_Is_RelativeAndLowercase()
    {
        HttpContext context = CreateHttpContext();
        var commands = new CommandCollection()
        {
            { "Width", "400" }
        };

        const string expected = "/images/image-12345.jpeg?width=400";
        string actual = UriRelativeLowerInvariantCacheKey.Create(context, commands);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void UriAbsoluteCacheKey_Is_Absolute()
    {
        HttpContext context = CreateHttpContext();
        var commands = new CommandCollection()
        {
            { "Width", "400" }
        };

        const string expected = "Testwebsite.com/Images/Image-12345.jpeg?width=400";
        string actual = UriAbsoluteCacheKey.Create(context, commands);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void UriAbsoluteLowerInvariantCacheKey_Is_AbsoluteAndLowercase()
    {
        HttpContext context = CreateHttpContext();
        var commands = new CommandCollection()
        {
            { "Width", "400" }
        };

        const string expected = "testwebsite.com/images/image-12345.jpeg?width=400";
        string actual = UriAbsoluteLowerInvariantCacheKey.Create(context, commands);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void LegacyV1CacheKey_Matches_V1_Behavior()
    {
        HttpContext context = CreateHttpContext();
        var commands = new CommandCollection()
        {
            { "Width", "400" }
        };

        const string expected = "testwebsite.com/images//image-12345.jpeg?width=400";
        string actual = LegacyV1CacheKey.Create(context, commands);

        Assert.Equal(expected, actual);
    }

    private static HttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = Uri.UriSchemeHttp;
        httpContext.Request.Host = new HostString("Testwebsite.com");
        httpContext.Request.PathBase = "/Images";
        httpContext.Request.Path = "/Image-12345.jpeg";
        httpContext.Request.QueryString = new QueryString("?Should-Be=Ignored");

        return httpContext;
    }
}
