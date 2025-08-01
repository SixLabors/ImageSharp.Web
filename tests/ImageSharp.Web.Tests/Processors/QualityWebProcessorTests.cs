// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web.Tests.Processors;

public class QualityWebProcessorTests
{
    private readonly Random random = new();

    [Fact]
    public void QualityWebProcessor_UpdatesJpegQuality()
    {
        CommandParser parser = new(new[] { new IntegralNumberConverter<int>() });
        CultureInfo culture = CultureInfo.InvariantCulture;

        CommandCollection commands = new()
        {
            { new KeyValuePair<string, string>(QualityWebProcessor.Quality, "42") },
        };

        using Image<Rgba32> image = new(1, 1);
        using FormattedImage formatted = new(image, JpegFormat.Instance);
        Assert.Equal(JpegFormat.Instance, formatted.Format);
        Assert.Equal(typeof(JpegEncoder), formatted.Encoder.GetType());

        new QualityWebProcessor()
            .Process(formatted, null, commands, parser, culture);

        Assert.Equal(JpegFormat.Instance, formatted.Format);
        Assert.Equal(42, ((JpegEncoder)formatted.Encoder).Quality);
    }

    [Fact]
    public void QualityWebProcessor_UpdatesWebpQuality()
    {
        CommandParser parser = new(new[] { new IntegralNumberConverter<int>() });
        CultureInfo culture = CultureInfo.InvariantCulture;

        CommandCollection commands = new()
        {
            {
                new KeyValuePair<string, string>(QualityWebProcessor.Quality, "42")
            },
        };

        using Image<Rgba32> image = new(1, 1);
        using FormattedImage formatted = new(image, WebpFormat.Instance);
        Assert.Equal(WebpFormat.Instance, formatted.Format);
        Assert.Equal(typeof(WebpEncoder), formatted.Encoder.GetType());

        new QualityWebProcessor()
            .Process(formatted, null, commands, parser, culture);

        Assert.Equal(WebpFormat.Instance, formatted.Format);
        Assert.Equal(42, ((WebpEncoder)formatted.Encoder).Quality);
        Assert.Equal(WebpFileFormatType.Lossy, ((WebpEncoder)formatted.Encoder).FileFormat);
    }

    [Fact]
    public void QualityWebProcessor_CanReportAlphaRequirements()
    {
        List<ICommandConverter> converters = new()
        {
            new IntegralNumberConverter<int>(),
        };

        CommandParser parser = new(converters);
        CultureInfo culture = CultureInfo.InvariantCulture;

        CommandCollection commands = new()
        {
            { new KeyValuePair<string, string>(QualityWebProcessor.Quality, this.random.Next(1, 100).ToString()) },
        };

        Assert.False(new QualityWebProcessor().RequiresTrueColorPixelFormat(commands, parser, culture));
    }
}
