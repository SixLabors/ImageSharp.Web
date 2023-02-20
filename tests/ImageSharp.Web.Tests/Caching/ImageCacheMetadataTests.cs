// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.Caching;

public class ImageCacheMetaDataTests
{
    private static readonly DateTime SourceLastWriteTimeUtc = new DateTime(1980, 11, 3);
    private static readonly DateTime CacheLastWriteTimeUtc = new DateTime(1980, 11, 4);
    private static readonly TimeSpan MaxAge = TimeSpan.FromDays(7);
    private const string ContentType = "image/jpeg";
    private const long ContentLength = 1234;

    public ImageCacheMetaDataTests(ITestOutputHelper output) => this.Output = output;

    protected ITestOutputHelper Output { get; }

    [Fact]
    public void ConstructorAssignsProperties()
    {
        var meta = new ImageCacheMetadata(
            SourceLastWriteTimeUtc,
            CacheLastWriteTimeUtc,
            ContentType,
            MaxAge,
            ContentLength);

        Assert.Equal(SourceLastWriteTimeUtc, meta.SourceLastWriteTimeUtc);
        Assert.Equal(CacheLastWriteTimeUtc, meta.CacheLastWriteTimeUtc);
        Assert.Equal(ContentType, meta.ContentType);
        Assert.Equal(MaxAge, meta.CacheControlMaxAge);
        Assert.Equal(ContentLength, meta.ContentLength);
    }

    [Fact]
    public void EqualityChecksAreCorrect()
    {
        var meta = new ImageCacheMetadata(
            SourceLastWriteTimeUtc,
            CacheLastWriteTimeUtc,
            ContentType,
            MaxAge,
            ContentLength);
        var meta2 = new ImageCacheMetadata(
            meta.SourceLastWriteTimeUtc,
            meta.CacheLastWriteTimeUtc,
            meta.ContentType,
            meta.CacheControlMaxAge,
            meta.ContentLength);

        Assert.Equal(meta, meta2);

        var meta3 = new ImageCacheMetadata(
            SourceLastWriteTimeUtc,
            CacheLastWriteTimeUtc,
            "image/png",
            MaxAge,
            ContentLength);

        Assert.NotEqual(meta, meta3);
    }
}
