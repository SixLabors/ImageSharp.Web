// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Processors;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Processors
{
    public class ResizeWebProcessorTests
    {
        [Theory]
        [InlineData(nameof(KnownResamplers.Bicubic))]
        [InlineData(nameof(KnownResamplers.Box))]
        [InlineData(nameof(KnownResamplers.CatmullRom))]
        [InlineData(nameof(KnownResamplers.Hermite))]
        [InlineData(nameof(KnownResamplers.Lanczos2))]
        [InlineData(nameof(KnownResamplers.Lanczos3))]
        [InlineData(nameof(KnownResamplers.Lanczos5))]
        [InlineData(nameof(KnownResamplers.Lanczos8))]
        [InlineData(nameof(KnownResamplers.MitchellNetravali))]
        [InlineData(nameof(KnownResamplers.NearestNeighbor))]
        [InlineData(nameof(KnownResamplers.Robidoux))]
        [InlineData(nameof(KnownResamplers.RobidouxSharp))]
        [InlineData(nameof(KnownResamplers.Spline))]
        [InlineData(nameof(KnownResamplers.Triangle))]
        [InlineData(nameof(KnownResamplers.Welch))]
        public void ResizeWebProcessor_UpdatesSize(string resampler)
        {
            const int width = 4;
            const int height = 6;

            var converters = new List<ICommandConverter>
            {
                new IntegralNumberConverter<uint>(),
                new ArrayConverter<float>(),
                new EnumConverter(),
                new SimpleCommandConverter<bool>(),
                new SimpleCommandConverter<float>()
            };

            var parser = new CommandParser(converters);
            CultureInfo culture = CultureInfo.InvariantCulture;

            CommandCollection commands = new()
            {
                // We only need to do this for the unit tests.
                // Commands generated via URL will automatically be converted to lowecase
                { new(ResizeWebProcessor.Sampler, resampler.ToLowerInvariant()) },
                { new(ResizeWebProcessor.Width, width.ToString()) },
                { new(ResizeWebProcessor.Height, height.ToString()) },
                { new(ResizeWebProcessor.Xy, "0,0") },
                { new(ResizeWebProcessor.Mode, nameof(ResizeMode.Stretch)) }
            };

            using var image = new Image<Rgba32>(1, 1);
            using var formatted = new FormattedImage(image, PngFormat.Instance);
            new ResizeWebProcessor().Process(formatted, null, commands, parser, culture);

            Assert.Equal(width, image.Width);
            Assert.Equal(height, image.Height);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, false)]
        [InlineData(2, false)]
        [InlineData(3, false)]
        [InlineData(4, false)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(7, true)]
        [InlineData(8, true)]
        public void ResizeWebProcessor_RespectsOrientation(ushort orientation, bool rotated)
        {
            const int width = 4;
            const int height = 6;

            var converters = new List<ICommandConverter>
            {
                new IntegralNumberConverter<uint>(),
                new ArrayConverter<float>(),
                new EnumConverter(),
                new SimpleCommandConverter<bool>(),
                new SimpleCommandConverter<float>()
            };

            var parser = new CommandParser(converters);
            CultureInfo culture = CultureInfo.InvariantCulture;

            CommandCollection commands = new()
            {
                { new(ResizeWebProcessor.Width, width.ToString()) },
                { new(ResizeWebProcessor.Height, height.ToString()) },
                { new(ResizeWebProcessor.Mode, nameof(ResizeMode.Stretch)) }
            };

            using var image = new Image<Rgba32>(1, 1);
            image.Metadata.ExifProfile = new();
            image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, orientation);

            using var formatted = new FormattedImage(image, PngFormat.Instance);
            new ResizeWebProcessor().Process(formatted, null, commands, parser, culture);

            if (rotated)
            {
                Assert.Equal(height, image.Width);
                Assert.Equal(width, image.Height);
            }
            else
            {
                Assert.Equal(width, image.Width);
                Assert.Equal(height, image.Height);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void ResizeWebProcessor_CanIgnoreOrientation(ushort orientation)
        {
            const int width = 4;
            const int height = 6;

            var converters = new List<ICommandConverter>
            {
                new IntegralNumberConverter<uint>(),
                new ArrayConverter<float>(),
                new EnumConverter(),
                new SimpleCommandConverter<bool>(),
                new SimpleCommandConverter<float>()
            };

            var parser = new CommandParser(converters);
            CultureInfo culture = CultureInfo.InvariantCulture;

            CommandCollection commands = new()
            {
                { new(ResizeWebProcessor.Width, width.ToString()) },
                { new(ResizeWebProcessor.Height, height.ToString()) },
                { new(ResizeWebProcessor.Mode, nameof(ResizeMode.Stretch)) },
                { new(ResizeWebProcessor.Orient, bool.FalseString) }
            };

            using var image = new Image<Rgba32>(1, 1);
            image.Metadata.ExifProfile = new();
            image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, orientation);

            using var formatted = new FormattedImage(image, PngFormat.Instance);
            new ResizeWebProcessor().Process(formatted, null, commands, parser, culture);

            Assert.Equal(width, image.Width);
            Assert.Equal(height, image.Height);
        }
    }
}
