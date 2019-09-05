// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using Xunit;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class ImageMetaDataTests
    {
        private static readonly DateTime LastWriteTimeUtc = new DateTime(1980, 11, 4);
        private static readonly TimeSpan MaxAge = TimeSpan.FromDays(7);

        public ImageMetaDataTests(ITestOutputHelper output) => this.Output = output;

        protected ITestOutputHelper Output { get; }

        [Fact]
        public void ConstructorAssignsProperties()
        {
            var meta = new ImageMetadata(LastWriteTimeUtc, MaxAge);
            Assert.Equal(LastWriteTimeUtc, meta.LastWriteTimeUtc);
            Assert.Equal(MaxAge, meta.CacheControlMaxAge);
        }

        [Fact]
        public void EqualityChecksAreCorrect()
        {
            var meta = new ImageMetadata(LastWriteTimeUtc, MaxAge);
            var meta2 = new ImageMetadata(meta.LastWriteTimeUtc, meta.CacheControlMaxAge);
            Assert.Equal(meta, meta2);

            var meta3 = new ImageMetadata(meta.LastWriteTimeUtc, TimeSpan.FromDays(1));
            Assert.NotEqual(meta, meta3);
        }
    }
}
