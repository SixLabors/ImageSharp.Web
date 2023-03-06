// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web.Tests.Processors;

public class GrayscaleWebProcessorTests
{
    [Theory]
    [InlineData(GrayscaleMode.Bt601)]
    [InlineData(GrayscaleMode.Bt709)]
    public void GrayscaleWebProcessor_Test(GrayscaleMode mode)
    {
        CommandParser parser = new(new[] { new EnumConverter() });
        CultureInfo culture = CultureInfo.InvariantCulture;

        CommandCollection commands = new()
        {
            { new(GrayscaleWebProcessor.Grayscale, mode.ToString()) }
        };

        using Image<Rgba32> image = new(1, 1);

        using FormattedImage formatted = new(image, PngFormat.Instance);
        new GrayscaleWebProcessor().Process(formatted, null, commands, parser, culture);
    }
}
