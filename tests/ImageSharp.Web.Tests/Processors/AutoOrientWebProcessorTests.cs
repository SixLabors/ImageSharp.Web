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
        using Image<Rgba32> image = new(1, 1);
        image.Metadata.ExifProfile = new();
        image.Metadata.ExifProfile.SetValue(ExifTag.Orientation, br);

        Assert.True(image.Metadata.ExifProfile.TryGetValue(ExifTag.Orientation, out IExifValue<ushort> orientation));
        Assert.Equal(br, orientation.Value);

        using FormattedImage formatted = new(image, PngFormat.Instance);
        new AutoOrientWebProcessor().Process(formatted, null, commands, parser, culture);

        Assert.True(image.Metadata.ExifProfile.TryGetValue(ExifTag.Orientation, out orientation));
        Assert.Equal(tl, orientation.Value);
    }
}
