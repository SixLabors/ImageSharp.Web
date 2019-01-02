// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;
using System.Text;
using SixLabors.Memory;
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
        public void ByteCountIsCorrect()
        {
            var meta = new ImageMetaData(LastWriteTimeUtc, ContentType);

            int dateBytes = Unsafe.SizeOf<DateTime>();
            int contentBytes = Encoding.ASCII.GetByteCount(ContentType);

            Assert.Equal(dateBytes + contentBytes, meta.GetByteCount());
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

        [Fact]
        public void CanSerializeCorrectly()
        {
            var meta = new ImageMetaData(LastWriteTimeUtc, ContentType);
            this.Output.WriteLine(meta.ToString());

            using (IManagedByteBuffer buffer = Configuration.Default.MemoryAllocator.AllocateManagedByteBuffer(meta.GetByteCount(), AllocationOptions.Clean))
            {
                meta.WriteTo(buffer.Memory.Span);

                var meta2 = ImageMetaData.Parse(buffer.Memory.Span);
                this.Output.WriteLine(meta2.ToString());

                Assert.Equal(meta, meta2);
            }
        }
    }
}