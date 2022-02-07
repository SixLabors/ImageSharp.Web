// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
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
        [InlineData(ExifOrientationMode.Unknown, false)]
        [InlineData(ExifOrientationMode.TopLeft, false)]
        [InlineData(ExifOrientationMode.TopRight, false)]
        [InlineData(ExifOrientationMode.BottomRight, false)]
        [InlineData(ExifOrientationMode.BottomLeft, false)]
        [InlineData(ExifOrientationMode.LeftTop, true)]
        [InlineData(ExifOrientationMode.RightTop, true)]
        [InlineData(ExifOrientationMode.RightBottom, true)]
        [InlineData(ExifOrientationMode.LeftBottom, true)]
        public void ResizeWebProcessor_RespectsOrientation_Size(ushort orientation, bool rotated)
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
        [InlineData(ExifOrientationMode.Unknown, false)]
        [InlineData(ExifOrientationMode.TopLeft, false)]
        [InlineData(ExifOrientationMode.TopRight, false)]
        [InlineData(ExifOrientationMode.BottomRight, false)]
        [InlineData(ExifOrientationMode.BottomLeft, false)]
        [InlineData(ExifOrientationMode.LeftTop, true)]
        [InlineData(ExifOrientationMode.RightTop, true)]
        [InlineData(ExifOrientationMode.RightBottom, true)]
        [InlineData(ExifOrientationMode.LeftBottom, true)]
        public void ResizeWebProcessor_RespectsOrientation_Center(ushort orientation, bool rotated)
        {
            const int width = 4;
            const int height = 6;
            const float x = 1;
            const float y = 2;

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
                { new(ResizeWebProcessor.Xy, $"{x},{y}") },
                { new(ResizeWebProcessor.Mode, nameof(ResizeMode.Stretch)) }
            };

            using var image = new Image<Rgba32>(1, 1);
            image.Metadata.ExifProfile = new();
            image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, orientation);

            using var formatted = new FormattedImage(image, PngFormat.Instance);

            ResizeOptions options = ResizeWebProcessor.GetResizeOptions(formatted, commands, parser, culture);
            PointF xy = options.CenterCoordinates.Value;

            if (rotated)
            {
                Assert.Equal(x, xy.Y);
                Assert.Equal(y, xy.X);
            }
            else
            {
                Assert.Equal(x, xy.X);
                Assert.Equal(y, xy.Y);
            }
        }

        [Theory]
        [InlineData(ExifOrientationMode.Unknown)]
        [InlineData(ExifOrientationMode.TopLeft)]
        [InlineData(ExifOrientationMode.TopRight)]
        [InlineData(ExifOrientationMode.BottomRight)]
        [InlineData(ExifOrientationMode.BottomLeft)]
        [InlineData(ExifOrientationMode.LeftTop)]
        [InlineData(ExifOrientationMode.RightTop)]
        [InlineData(ExifOrientationMode.RightBottom)]
        [InlineData(ExifOrientationMode.LeftBottom)]
        public void ResizeWebProcessor_RespectsOrientation_Anchor(ushort orientation)
        {
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

            using Image<Rgba32> image = new(1, 1);
            image.Metadata.ExifProfile = new();
            image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, orientation);
            using FormattedImage formatted = new(image, PngFormat.Instance);

            foreach (AnchorPositionMode anchor in (AnchorPositionMode[])Enum.GetValues(typeof(AnchorPositionMode)))
            {
                CommandCollection commands = new()
                {
                    { new(ResizeWebProcessor.Width, 4.ToString()) },
                    { new(ResizeWebProcessor.Height, 6.ToString()) },
                    { new(ResizeWebProcessor.Mode, nameof(ResizeMode.Stretch)) },
                    { new(ResizeWebProcessor.Anchor, anchor.ToString()) },
                };

                ResizeOptions options = ResizeWebProcessor.GetResizeOptions(formatted, commands, parser, culture);

                AnchorPositionMode expected = GetExpectedAnchor(orientation, anchor);
                Assert.Equal(expected, options.Position);
            }
        }

        [Theory]
        [InlineData(ExifOrientationMode.Unknown)]
        [InlineData(ExifOrientationMode.TopLeft)]
        [InlineData(ExifOrientationMode.TopRight)]
        [InlineData(ExifOrientationMode.BottomRight)]
        [InlineData(ExifOrientationMode.BottomLeft)]
        [InlineData(ExifOrientationMode.LeftTop)]
        [InlineData(ExifOrientationMode.RightTop)]
        [InlineData(ExifOrientationMode.RightBottom)]
        [InlineData(ExifOrientationMode.LeftBottom)]
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

        private static AnchorPositionMode GetExpectedAnchor(ushort orientation, AnchorPositionMode anchor)
            => anchor switch
            {
                AnchorPositionMode.Center => anchor,
                AnchorPositionMode.Top => orientation switch
                {
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.BottomRight => AnchorPositionMode.Bottom,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.RightTop => AnchorPositionMode.Left,
                    ExifOrientationMode.LeftBottom or ExifOrientationMode.RightBottom => AnchorPositionMode.Right,
                    _ => anchor,
                },
                AnchorPositionMode.Bottom => orientation switch
                {
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.BottomRight => AnchorPositionMode.Top,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.RightTop => AnchorPositionMode.Right,
                    ExifOrientationMode.LeftBottom or ExifOrientationMode.RightBottom => AnchorPositionMode.Left,
                    _ => anchor,
                },
                AnchorPositionMode.Left => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.BottomRight => AnchorPositionMode.Right,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.LeftBottom => AnchorPositionMode.Bottom,
                    ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom => AnchorPositionMode.Top,
                    _ => anchor,
                },
                AnchorPositionMode.Right => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.BottomRight => AnchorPositionMode.Left,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.LeftBottom => AnchorPositionMode.Top,
                    ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom => AnchorPositionMode.Bottom,
                    _ => anchor,
                },
                AnchorPositionMode.TopLeft => orientation switch
                {
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.LeftTop => AnchorPositionMode.BottomLeft,
                    ExifOrientationMode.TopRight or ExifOrientationMode.RightBottom => AnchorPositionMode.TopRight,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.BottomRight,
                    _ => anchor,
                },
                AnchorPositionMode.TopRight => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.LeftTop => AnchorPositionMode.TopLeft,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.RightBottom => AnchorPositionMode.BottomRight,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.RightTop => AnchorPositionMode.BottomLeft,
                    _ => anchor,
                },
                AnchorPositionMode.BottomRight => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.RightBottom => AnchorPositionMode.BottomLeft,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.LeftTop => AnchorPositionMode.TopRight,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.TopLeft,
                    _ => anchor,
                },
                AnchorPositionMode.BottomLeft => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.LeftTop => AnchorPositionMode.BottomRight,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.RightBottom => AnchorPositionMode.TopLeft,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.RightTop => AnchorPositionMode.TopRight,
                    _ => anchor,
                },
                _ => anchor,
            };
    }
}
