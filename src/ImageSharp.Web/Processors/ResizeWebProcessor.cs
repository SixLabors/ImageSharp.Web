// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors
{
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
                Compand,
                Orient
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
            ResizeOptions options = GetResizeOptions(image, commands, parser, culture);

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
        internal static ResizeOptions GetResizeOptions(
            FormattedImage image,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
        {
            if (!commands.Contains(Width) && !commands.Contains(Height))
            {
                return null;
            }

            bool rotated = ShouldHandleExifRotation(image, commands, parser, culture, out ushort orientation);

            Size size = ParseSize(rotated, commands, parser, culture);

            if (size.Width <= 0 && size.Height <= 0)
            {
                return null;
            }

            ResizeOptions options = new()
            {
                Size = size,
                CenterCoordinates = GetCenter(image, orientation, commands, parser, culture),
                Position = GetAnchor(orientation, commands, parser, culture),
                Mode = GetMode(commands, parser, culture),
                Compand = GetCompandMode(commands, parser, culture),
            };

            // Defaults to Bicubic if not set.
            options.Sampler = GetSampler(commands);

            return options;
        }

        private static Size ParseSize(
            bool rotated,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
        {
            // The command parser will reject negative numbers as it clamps values to ranges.
            int width = (int)parser.ParseValue<uint>(commands.GetValueOrDefault(Width), culture);
            int height = (int)parser.ParseValue<uint>(commands.GetValueOrDefault(Height), culture);
            return rotated ? new Size(height, width) : new Size(width, height);
        }

        private static PointF? GetCenter(
            FormattedImage image,
            ushort orientation,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
        {
            float[] coordinates = parser.ParseValue<float[]>(commands.GetValueOrDefault(Xy), culture);

            if (coordinates.Length != 2)
            {
                return null;
            }

            PointF center = new(coordinates[0], coordinates[1]);
            if (orientation is >= ExifOrientationMode.Unknown and <= ExifOrientationMode.TopLeft)
            {
                return center;
            }

            AffineTransformBuilder builder = new();
            Size size = image.Image.Size();

            // New XY is calculated based on flipping and rotating the input XY.
            // Operations are based upon AutoOrientProcessor implementation.
            switch (orientation)
            {
                case ExifOrientationMode.TopRight:
                    builder.AppendTranslation(new PointF(size.Width - center.X, 0));
                    break;
                case ExifOrientationMode.BottomRight:
                    builder.AppendRotationDegrees(180);
                    builder.AppendTranslation(new PointF(0, -(size.Height - center.Y)));
                    break;
                case ExifOrientationMode.BottomLeft:
                    builder.AppendRotationDegrees(180);
                    builder.AppendTranslation(new PointF(size.Width - center.X, -(size.Height - center.Y)));
                    break;
                case ExifOrientationMode.LeftTop:
                    builder.AppendRotationDegrees(90);
                    builder.AppendTranslation(new PointF(size.Width - center.X, 0));
                    break;
                case ExifOrientationMode.RightTop:
                    builder.AppendRotationDegrees(270);
                    break;
                case ExifOrientationMode.RightBottom:
                    builder.AppendRotationDegrees(270);
                    builder.AppendTranslation(new PointF(-(size.Width - center.X), -(size.Height - center.Y)));
                    break;
                case ExifOrientationMode.LeftBottom:
                    builder.AppendRotationDegrees(90);
                    builder.AppendTranslation(new PointF(-(size.Width - center.X), 0));
                    break;
                default:
                    return center;
            }

            Matrix3x2 matrix = builder.BuildMatrix(size);
            return Vector2.Transform(center, matrix);
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
            if (orientation is >= ExifOrientationMode.Unknown and <= ExifOrientationMode.TopLeft)
            {
                return anchor;
            }

            /*
                New anchor position is determined by calculating the direction of the anchor relative to the source image.
                In the following example, the TopRight anchor becomes BottomRight

                            T                              L
                +-----------------------+           +--------------+
                |                      *|           |              |
                |                       |           |              |
              L |           TL          | R         |              |
                |                       |           |              |
                |                       |         B |      LB      | T
                |                       |           |              |
                +-----------------------+           |              |
                            B                       |              |
                                                    |              |
                                                    |             *|
                                                    +--------------+
                                                            R
          */
            return anchor switch
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
                    ExifOrientationMode.LeftTop or ExifOrientationMode.LeftBottom => AnchorPositionMode.Top,
                    ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom => AnchorPositionMode.Bottom,
                    _ => anchor,
                },
                AnchorPositionMode.Right => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.BottomRight => AnchorPositionMode.Left,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.LeftBottom => AnchorPositionMode.Bottom,
                    ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom => AnchorPositionMode.Top,
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
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.RightTop => AnchorPositionMode.TopRight,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.RightBottom => AnchorPositionMode.TopLeft,
                    _ => anchor,
                },
                AnchorPositionMode.BottomLeft => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.RightTop => AnchorPositionMode.BottomRight,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.LeftBottom => AnchorPositionMode.TopLeft,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.LeftTop => AnchorPositionMode.TopRight,
                    _ => anchor,
                },
                _ => anchor,
            };
        }

        private static bool GetCompandMode(
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
            => parser.ParseValue<bool>(commands.GetValueOrDefault(Compand), culture);

        private static IResampler GetSampler(CommandCollection commands)
        {
            string sampler = commands.GetValueOrDefault(Sampler);

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

        private static bool ShouldHandleExifRotation(FormattedImage image, CommandCollection commands, CommandParser parser, CultureInfo culture, out ushort orientation)
        {
            // Browsers now implement 'image-orientation: from-image' by default.
            // https://developer.mozilla.org/en-US/docs/web/css/image-orientation
            // This makes orientation handling confusing for users who expect images to be resized in accordance
            // to what they observe rather than pure (and correct) methods.
            //
            // To accomodate this we parse the dimensions to use based upon decoded EXIF orientation values, switching
            // the width/height parameters when images are rotated (not flipped).
            // We default to 'true' for EXIF orientation handling. By passing 'false' it can be turned off.
            if (commands.Contains(Orient) && !parser.ParseValue<bool>(commands.GetValueOrDefault(Orient), culture))
            {
                orientation = ExifOrientationMode.Unknown;
                return false;
            }

            return image.IsExifRotated(out orientation);
        }
    }
}
