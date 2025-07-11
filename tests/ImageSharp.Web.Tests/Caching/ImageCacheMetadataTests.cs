// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.Caching;

public class ImageCacheMetaDataTests
{
    private static readonly DateTime SourceLastWriteTimeUtc = new(1980, 11, 3);
    private static readonly DateTime CacheLastWriteTimeUtc = new(1980, 11, 4);
    private static readonly TimeSpan MaxAge = TimeSpan.FromDays(7);
    private const string ContentType = "image/jpeg";
    private const long ContentLength = 1234;

    public ImageCacheMetaDataTests(ITestOutputHelper output) => this.Output = output;

    protected ITestOutputHelper Output { get; }

    [Fact]
    public void ConstructorAssignsProperties()
    {
        ImageCacheMetadata meta = new(
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
        ImageCacheMetadata meta = new(
            SourceLastWriteTimeUtc,
            CacheLastWriteTimeUtc,
            ContentType,
            MaxAge,
            ContentLength);
        ImageCacheMetadata meta2 = new(
            meta.SourceLastWriteTimeUtc,
            meta.CacheLastWriteTimeUtc,
            meta.ContentType,
            meta.CacheControlMaxAge,
            meta.ContentLength);

        Assert.Equal(meta, meta2);

        ImageCacheMetadata meta3 = new(
            SourceLastWriteTimeUtc,
            CacheLastWriteTimeUtc,
            "image/png",
            MaxAge,
            ContentLength);

        Assert.NotEqual(meta, meta3);
    }
}
