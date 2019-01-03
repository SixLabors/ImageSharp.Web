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
        private const string ContentType = "image/jpeg";

        public ImageMetaDataTests(ITestOutputHelper output) => this.Output = output;

        protected ITestOutputHelper Output { get; }

        [Fact]
        public void ConstructorAssignsProperties()
        {
            var meta = new ImageMetaData(LastWriteTimeUtc, ContentType);
            Assert.Equal(LastWriteTimeUtc, meta.LastWriteTimeUtc);
            Assert.Equal(ContentType, meta.ContentType);
        }

        [Fact]
        public void EqualityChecksAreCorrect()
        {
            var meta = new ImageMetaData(LastWriteTimeUtc, ContentType);
            var meta2 = new ImageMetaData(meta.LastWriteTimeUtc, meta.ContentType);
            Assert.Equal(meta, meta2);

            var meta3 = new ImageMetaData(meta.LastWriteTimeUtc, "image/png");
            Assert.NotEqual(meta, meta3);
        }
    }
}