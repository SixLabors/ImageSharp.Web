// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web.Tests.Processors;

public class AutoOrientWebProcessorTests
{
    [Fact]
    public void AutoOrientWebProcessor_UpdatesOrientation()
    {
        CommandParser parser = new(new[] { new SimpleCommandConverter<bool>() });
        CultureInfo culture = CultureInfo.InvariantCulture;

        CommandCollection commands = new()
        {
            { new(AutoOrientWebProcessor.AutoOrient, bool.TrueString) }
        };

        const ushort tl = 1;
        const ushort br = 3;
        using var image = new Image<Rgba32>(1, 1);
        image.Metadata.ExifProfile = new();
        image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, br);

        IExifValue<ushort> orientation = image.Metadata.ExifProfile.GetValue(ExifTag.Orientation);
        Assert.Equal(br, orientation.Value);

        using var formatted = new FormattedImage(image, PngFormat.Instance);
        new AutoOrientWebProcessor().Process(formatted, null, commands, parser, culture);

        orientation = image.Metadata.ExifProfile.GetValue(ExifTag.Orientation);
        Assert.Equal(tl, orientation.Value);
    }
}
