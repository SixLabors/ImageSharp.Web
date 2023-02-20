// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Numerics;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web.Tests.Processors;

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
            new SimpleCommandConverter<float>(),
            new ColorConverter()
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
            new SimpleCommandConverter<float>(),
            new ColorConverter()
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
        const float x = .25F;
        const float y = .5F;

        var converters = new List<ICommandConverter>
        {
            new IntegralNumberConverter<uint>(),
            new ArrayConverter<float>(),
            new EnumConverter(),
            new SimpleCommandConverter<bool>(),
            new SimpleCommandConverter<float>(),
            new ColorConverter()
        };

        var parser = new CommandParser(converters);
        CultureInfo culture = CultureInfo.InvariantCulture;

        CommandCollection commands = new()
        {
            { new(ResizeWebProcessor.Width, width.ToString()) },
            { new(ResizeWebProcessor.Height, height.ToString()) },
            { new(ResizeWebProcessor.Xy, FormattableString.Invariant($"{x},{y}")) },
            { new(ResizeWebProcessor.Mode, nameof(ResizeMode.Stretch)) }
        };

        using var image = new Image<Rgba32>(3, 3);
        image.Metadata.ExifProfile = new();
        image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, orientation);
        using var formatted = new FormattedImage(image, PngFormat.Instance);

        PointF expected = ExifOrientationUtilities.Transform(new Vector2(x, y), Vector2.Zero, Vector2.One, orientation);
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
            new SimpleCommandConverter<float>(),
            new ColorConverter()
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

            AnchorPositionMode expected = ExifOrientationUtilities.Transform(anchor, orientation);
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
            new SimpleCommandConverter<float>(),
            new ColorConverter()
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

    [Theory]
    [InlineData(ResizeMode.Crop, false)]
    [InlineData(ResizeMode.Pad, true)]
    [InlineData(ResizeMode.BoxPad, true)]
    [InlineData(ResizeMode.Max, false)]
    [InlineData(ResizeMode.Min, false)]
    [InlineData(ResizeMode.Stretch, false)]
    [InlineData(ResizeMode.Manual, false)]
    public void ResizeWebProcessor_CanReportAlphaRequirements(ResizeMode resizeMode, bool requiresAlpha)
    {
        var converters = new List<ICommandConverter>
        {
            new EnumConverter(),
        };

        var parser = new CommandParser(converters);
        CultureInfo culture = CultureInfo.InvariantCulture;

        CommandCollection commands = new()
        {
            { new(ResizeWebProcessor.Mode, resizeMode.ToString()) },
        };

        Assert.Equal(requiresAlpha, new ResizeWebProcessor().RequiresTrueColorPixelFormat(commands, parser, culture));
    }
}
