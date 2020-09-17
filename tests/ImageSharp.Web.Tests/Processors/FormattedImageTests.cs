// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Processors
{
    public class FormattedImageTests
    {
        [Fact]
        public void ConstructorSetsProperties()
        {
            using var image = new Image<Rgba32>(1, 1);
            using var formatted = new FormattedImage(image, JpegFormat.Instance);

            Assert.NotNull(formatted.Image);
            Assert.Equal(image, formatted.Image);

            Assert.NotNull(formatted.Format);
            Assert.Equal(JpegFormat.Instance, formatted.Format);

            Assert.NotNull(formatted.Encoder);
            Assert.Equal(typeof(JpegEncoder), formatted.Encoder.GetType());
        }

        [Fact]
        public void CanSetFormat()
        {
            using var image = new Image<Rgba32>(1, 1);
            using var formatted = new FormattedImage(image, JpegFormat.Instance);

            Assert.NotNull(formatted.Format);
            Assert.Equal(JpegFormat.Instance, formatted.Format);

            Assert.Throws<ArgumentNullException>(() => formatted.Format = null);

            formatted.Format = PngFormat.Instance;
            Assert.Equal(PngFormat.Instance, formatted.Format);
            Assert.Equal(typeof(PngEncoder), formatted.Encoder.GetType());
        }

        [Fact]
        public void CanSetEncoder()
        {
            using var image = new Image<Rgba32>(1, 1);
            using var formatted = new FormattedImage(image, PngFormat.Instance);

            Assert.NotNull(formatted.Format);
            Assert.Equal(PngFormat.Instance, formatted.Format);

            Assert.Throws<ArgumentNullException>(() => formatted.Encoder = null);
            Assert.Throws<ArgumentException>(() => formatted.Encoder = new JpegEncoder());

            formatted.Format = JpegFormat.Instance;
            Assert.Equal(typeof(JpegEncoder), formatted.Encoder.GetType());

            JpegSubsample current = ((JpegEncoder)formatted.Encoder).Subsample.GetValueOrDefault();

            Assert.Equal(JpegSubsample.Ratio444, current);
            formatted.Encoder = new JpegEncoder { Subsample = JpegSubsample.Ratio420 };

            JpegSubsample replacement = ((JpegEncoder)formatted.Encoder).Subsample.GetValueOrDefault();

            Assert.NotEqual(current, replacement);
            Assert.Equal(JpegSubsample.Ratio420, replacement);
        }
    }
}
