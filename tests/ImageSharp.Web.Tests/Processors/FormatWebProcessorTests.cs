// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web.Tests.Processors;

public class FormatWebProcessorTests
{
    public static TheoryData<string> FormatNameData { get; }
        = new TheoryData<string>
        {
            { BmpFormat.Instance.Name },
            { GifFormat.Instance.Name },
            { PngFormat.Instance.Name },
            { JpegFormat.Instance.Name },
            { WebpFormat.Instance.Name },
        };

    [Fact]
    public void FormatWebProcessor_UpdatesFormat()
    {
        CommandParser parser = new(Array.Empty<ICommandConverter>());
        CultureInfo culture = CultureInfo.InvariantCulture;

        CommandCollection commands = new()
        {
            { new(FormatWebProcessor.Format, GifFormat.Instance.Name) },
        };

        using var image = new Image<Rgba32>(1, 1);
        using var formatted = new FormattedImage(image, PngFormat.Instance);
        Assert.Equal(formatted.Format, PngFormat.Instance);

        new FormatWebProcessor(Options.Create(new ImageSharpMiddlewareOptions()))
            .Process(formatted, null, commands, parser, culture);

        Assert.Equal(formatted.Format, GifFormat.Instance);
    }

    [Theory]
    [MemberData(nameof(FormatNameData))]
    public void FormatWebProcessor_CanReportAlphaRequirements(string format)
    {
        CommandParser parser = new(Array.Empty<ICommandConverter>());
        CultureInfo culture = CultureInfo.InvariantCulture;

        CommandCollection commands = new()
        {
            { new(FormatWebProcessor.Format, format) },
        };

        FormatWebProcessor processor = new(Options.Create(new ImageSharpMiddlewareOptions()));
        Assert.False(processor.RequiresTrueColorPixelFormat(commands, parser, culture));
    }
}
