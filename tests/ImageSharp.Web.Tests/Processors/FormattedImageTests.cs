// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Web.Tests.Processors;

public class FormattedImageTests
{
    [Fact]
    public void ConstructorSetsProperties()
    {
        using Image<Rgba32> image = new(1, 1);
        using FormattedImage formatted = new(image, JpegFormat.Instance);

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
        using Image<Rgba32> image = new(1, 1);
        using FormattedImage formatted = new(image, JpegFormat.Instance);

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
        using Image<Rgba32> image = new(1, 1);
        using FormattedImage formatted = new(image, PngFormat.Instance);

        Assert.NotNull(formatted.Format);
        Assert.Equal(PngFormat.Instance, formatted.Format);

        Assert.Throws<ArgumentNullException>(() => formatted.Encoder = null);
        Assert.Throws<ArgumentException>(() => formatted.Encoder = new JpegEncoder());

        formatted.Format = JpegFormat.Instance;
        Assert.Equal(typeof(JpegEncoder), formatted.Encoder.GetType());

        JpegEncodingColor current = ((JpegEncoder)formatted.Encoder).ColorType.GetValueOrDefault();

        Assert.Equal(JpegEncodingColor.YCbCrRatio420, current);
        formatted.Encoder = new JpegEncoder { ColorType = JpegEncodingColor.YCbCrRatio444 };

        JpegEncodingColor replacement = ((JpegEncoder)formatted.Encoder).ColorType.GetValueOrDefault();

        Assert.NotEqual(current, replacement);
        Assert.Equal(JpegEncodingColor.YCbCrRatio444, replacement);
    }
}
