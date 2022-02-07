// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
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
        [InlineData(ExifOrientationMode.Unknown)]
        [InlineData(ExifOrientationMode.TopLeft)]
        [InlineData(ExifOrientationMode.TopRight)]
        [InlineData(ExifOrientationMode.BottomRight)]
        [InlineData(ExifOrientationMode.BottomLeft)]
        [InlineData(ExifOrientationMode.LeftTop)]
        [InlineData(ExifOrientationMode.RightTop)]
        [InlineData(ExifOrientationMode.RightBottom)]
        [InlineData(ExifOrientationMode.LeftBottom)]
        public void ResizeWebProcessor_RespectsOrientation_Center(ushort orientation)
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

            using var image = new Image<Rgba32>(3, 3);
            image.Metadata.ExifProfile = new();
            image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, orientation);
            using var formatted = new FormattedImage(image, PngFormat.Instance);

            PointF expected = GetExpectedCenter(orientation, image.Size(), new PointF(x, y));
            ResizeOptions options = ResizeWebProcessor.GetResizeOptions(formatted, commands, parser, culture);
            Assert.Equal(expected, options.CenterCoordinates);
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

                AnchorPositionMode expected = GetExpectedAnchor(orientation, anchor);
                ResizeOptions options = ResizeWebProcessor.GetResizeOptions(formatted, commands, parser, culture);
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

        private static PointF GetExpectedCenter(ushort orientation, Size size, PointF center)
        {
            AffineTransformBuilder builder = new();
            switch (orientation)
            {
                case ExifOrientationMode.TopRight:
                    builder.AppendTranslation(new PointF(size.Width - center.X, 0));
                    break;
                case ExifOrientationMode.BottomRight:
                    builder.AppendRotationDegrees(180);
                    break;
                case ExifOrientationMode.BottomLeft:
                    builder.AppendTranslation(new PointF(0, size.Height - center.Y));
                    break;
                case ExifOrientationMode.LeftTop:
                    builder.AppendRotationDegrees(90);
                    builder.AppendTranslation(new PointF(size.Width - center.X, 0));
                    break;
                case ExifOrientationMode.RightTop:
                    builder.AppendRotationDegrees(90);
                    break;
                case ExifOrientationMode.RightBottom:
                    builder.AppendTranslation(new PointF(0, size.Height - center.Y));
                    builder.AppendRotationDegrees(270);
                    break;
                case ExifOrientationMode.LeftBottom:
                    builder.AppendRotationDegrees(270);
                    break;
                default:
                    return center;
            }

            Matrix3x2 matrix = builder.BuildMatrix(size);
            return Vector2.Transform(center, matrix);
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
                    ExifOrientationMode.TopRight or ExifOrientationMode.BottomLeft => AnchorPositionMode.Right,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.LeftBottom => AnchorPositionMode.Top,
                    ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom => AnchorPositionMode.Bottom,
                    _ => anchor,
                },
                AnchorPositionMode.Right => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.BottomLeft => AnchorPositionMode.Left,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.LeftBottom => AnchorPositionMode.Top,
                    ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom => AnchorPositionMode.Bottom,
                    _ => anchor,
                },
                AnchorPositionMode.TopLeft => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.TopRight,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.RightTop => AnchorPositionMode.BottomLeft,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.RightBottom => AnchorPositionMode.BottomRight,
                    _ => anchor,
                },
                AnchorPositionMode.TopRight => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.RightTop => AnchorPositionMode.TopLeft,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.BottomRight,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.LeftTop => AnchorPositionMode.BottomLeft,
                    _ => anchor,
                },
                AnchorPositionMode.BottomRight => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.BottomLeft,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.RightTop => AnchorPositionMode.TopRight,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.RightBottom => AnchorPositionMode.TopLeft,
                    _ => anchor,
                },
                AnchorPositionMode.BottomLeft => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.RightTop => AnchorPositionMode.BottomRight,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.TopLeft,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.LeftTop => AnchorPositionMode.TopRight,
                    _ => anchor,
                },
                _ => anchor,
            };
    }
}
