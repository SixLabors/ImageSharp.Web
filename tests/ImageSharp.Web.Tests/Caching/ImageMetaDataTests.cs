// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.Caching;

public class ImageMetaDataTests
{
    private static readonly DateTime LastWriteTimeUtc = new(1980, 11, 4);
    private static readonly TimeSpan MaxAge = TimeSpan.FromDays(7);
    private const long ContentLength = 1234;

    public ImageMetaDataTests(ITestOutputHelper output) => this.Output = output;

    protected ITestOutputHelper Output { get; }

    [Fact]
    public void ConstructorAssignsProperties()
    {
        ImageMetadata meta = new(LastWriteTimeUtc, MaxAge, ContentLength);
        Assert.Equal(LastWriteTimeUtc, meta.LastWriteTimeUtc);
        Assert.Equal(MaxAge, meta.CacheControlMaxAge);
        Assert.Equal(ContentLength, meta.ContentLength);
    }

    [Fact]
    public void EqualityChecksAreCorrect()
    {
        ImageMetadata meta = new(LastWriteTimeUtc, MaxAge, ContentLength);
        ImageMetadata meta2 = new(meta.LastWriteTimeUtc, meta.CacheControlMaxAge, meta.ContentLength);
        Assert.Equal(meta, meta2);

        ImageMetadata meta3 = new(meta.LastWriteTimeUtc, TimeSpan.FromDays(1), 4321);
        Assert.NotEqual(meta, meta3);
    }
}
