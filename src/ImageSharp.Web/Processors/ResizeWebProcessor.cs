// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors;

/// <summary>
/// Allows the resizing of images.
/// </summary>
public class ResizeWebProcessor : IImageWebProcessor
{
    /// <summary>
    /// The command constant for the resize width.
    /// </summary>
    public const string Width = "width";

    /// <summary>
    /// The command constant for the resize height.
    /// </summary>
    public const string Height = "height";

    /// <summary>
    /// The command constant for the resize focal point coordinates.
    /// </summary>
    public const string Xy = "rxy";

    /// <summary>
    /// The command constant for the resize mode.
    /// </summary>
    public const string Mode = "rmode";

    /// <summary>
    /// The command constant for the resize sampler.
    /// </summary>
    public const string Sampler = "rsampler";

    /// <summary>
    /// The command constant for the resize anchor position.
    /// </summary>
    public const string Anchor = "ranchor";

    /// <summary>
    /// The command constant for the resize padding background color.
    /// </summary>
    public const string Color = "rcolor";

    /// <summary>
    /// The command constant for the resize orientation handling mode.
    /// </summary>
    public const string Orient = "orient";

    /// <summary>
    /// The command constant for the resize compand mode.
    /// </summary>
    public const string Compand = "compand";

    private static readonly IEnumerable<string> ResizeCommands
        = new[]
        {
            Width,
            Height,
            Xy,
            Mode,
            Sampler,
            Anchor,
            Color,
            Orient,
            Compand
        };

    /// <inheritdoc/>
    public IEnumerable<string> Commands { get; } = ResizeCommands;

    /// <inheritdoc/>
    public FormattedImage Process(
        FormattedImage image,
        ILogger logger,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        ResizeOptions? options = GetResizeOptions(image, commands, parser, culture);

        if (options != null)
        {
            image.Image.Mutate(x => x.Resize(options));
        }

        return image;
    }

    /// <summary>
    /// Parses the command collection returning the resize options.
    /// </summary>
    /// <param name="image">The image to process.</param>
    /// <param name="commands">The ordered collection containing the processing commands.</param>
    /// <param name="parser">The command parser use for parting commands.</param>
    /// <param name="culture">
    /// The <see cref="CultureInfo"/> to use as the current parsing culture.
    /// </param>
    /// <returns>The <see cref="ResizeOptions"/>.</returns>
    internal static ResizeOptions? GetResizeOptions(
        FormattedImage image,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        if (!commands.Contains(Width) && !commands.Contains(Height))
        {
            return null;
        }

        ushort orientation = GetExifOrientation(image, commands, parser, culture);

        Size size = ParseSize(orientation, commands, parser, culture);

        if (size.Width <= 0 && size.Height <= 0)
        {
            return null;
        }

        ResizeMode mode = GetMode(commands, parser, culture);

        return new()
        {
            Size = size,
            CenterCoordinates = GetCenter(image, orientation, commands, parser, culture),
            Position = GetAnchor(orientation, commands, parser, culture),
            Mode = mode,
            Compand = GetCompandMode(commands, parser, culture),
            Sampler = GetSampler(commands),
            PadColor = parser.ParseValue<Color>(commands.GetValueOrDefault(Color), culture),
            TargetRectangle = mode is ResizeMode.Manual ? new Rectangle(0, 0, size.Width, size.Height) : null
        };
    }

    /// <inheritdoc/>
    public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture)
    {
        ResizeMode mode = parser.ParseValue<ResizeMode>(commands.GetValueOrDefault(Mode), culture);
        return mode is ResizeMode.Pad or ResizeMode.BoxPad;
    }

    private static Size ParseSize(
        ushort orientation,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        // The command parser will reject negative numbers as it clamps values to ranges.
        int width = (int)parser.ParseValue<uint>(commands.GetValueOrDefault(Width), culture);
        int height = (int)parser.ParseValue<uint>(commands.GetValueOrDefault(Height), culture);

        return ExifOrientationUtilities.Transform(new Size(width, height), orientation);
    }

    private static PointF? GetCenter(
        FormattedImage image,
        ushort orientation,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        float[]? coordinates = parser.ParseValue<float[]>(commands.GetValueOrDefault(Xy), culture);

        if (coordinates is null)
        {
            return null;
        }

        if (coordinates.Length != 2)
        {
            return null;
        }

        // Coordinates for the center point are given as a percentage.
        // We must convert these to pixel values for transformation then convert back.
        //
        // Get the display size of the image after orientation is applied.
        Size size = ExifOrientationUtilities.Transform(new Size(image.Image.Width, image.Image.Height), orientation);
        Vector2 min = Vector2.Zero;
        Vector2 max = new(size.Width, size.Height);

        // Scale pixel values up to image height and transform.
        Vector2 center = DeScale(new Vector2(coordinates[0], coordinates[1]), min, max);
        Vector2 transformed = ExifOrientationUtilities.Transform(center, min, max, orientation);

        // Now scale pixel values down as percentage of real image height.
        max = new Vector2(image.Image.Width, image.Image.Height);
        return Scale(transformed, min, max);
    }

    private static ResizeMode GetMode(
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
        => parser.ParseValue<ResizeMode>(commands.GetValueOrDefault(Mode), culture);

    private static AnchorPositionMode GetAnchor(
        ushort orientation,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        AnchorPositionMode anchor = parser.ParseValue<AnchorPositionMode>(commands.GetValueOrDefault(Anchor), culture);
        return ExifOrientationUtilities.Transform(anchor, orientation);
    }

    private static bool GetCompandMode(
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
        => parser.ParseValue<bool>(commands.GetValueOrDefault(Compand), culture);

    private static IResampler GetSampler(CommandCollection commands)
    {
        string? sampler = commands.GetValueOrDefault(Sampler);

        if (sampler != null)
        {
            // No need to do a case test here. Parsed commands are automatically converted to lowercase.
            return sampler switch
            {
                "nearest" or "nearestneighbor" => KnownResamplers.NearestNeighbor,
                "box" => KnownResamplers.Box,
                "mitchell" or "mitchellnetravali" => KnownResamplers.MitchellNetravali,
                "catmull" or "catmullrom" => KnownResamplers.CatmullRom,
                "lanczos2" => KnownResamplers.Lanczos2,
                "lanczos3" => KnownResamplers.Lanczos3,
                "lanczos5" => KnownResamplers.Lanczos5,
                "lanczos8" => KnownResamplers.Lanczos8,
                "welch" => KnownResamplers.Welch,
                "robidoux" => KnownResamplers.Robidoux,
                "robidouxsharp" => KnownResamplers.RobidouxSharp,
                "spline" => KnownResamplers.Spline,
                "triangle" => KnownResamplers.Triangle,
                "hermite" => KnownResamplers.Hermite,
                _ => KnownResamplers.Bicubic,
            };
        }

        return KnownResamplers.Bicubic;
    }

    private static ushort GetExifOrientation(FormattedImage image, CommandCollection commands, CommandParser parser, CultureInfo culture)
    {
        // Browsers now implement 'image-orientation: from-image' by default.
        // https://developer.mozilla.org/en-US/docs/web/css/image-orientation
        // This makes orientation handling confusing for users who expect images to be resized in accordance
        // to what they observe rather than pure (and correct) methods.
        //
        // To accomodate this we parse the dimensions to use based upon decoded EXIF orientation values.
        // We default to 'true' for EXIF orientation handling. By passing 'false' it can be turned off.
        if (commands.Contains(Orient) && !parser.ParseValue<bool>(commands.GetValueOrDefault(Orient), culture))
        {
            return ExifOrientationMode.Unknown;
        }

        image.TryGetExifOrientation(out ushort orientation);
        return orientation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 Scale(Vector2 x, Vector2 min, Vector2 max) => (x - min) / (max - min);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 DeScale(Vector2 x, Vector2 min, Vector2 max) => min + (x * (max - min));
}
