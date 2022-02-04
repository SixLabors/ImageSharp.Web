// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
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
            ResizeOptions options = GetResizeOptions(image.Image, commands, parser, culture);

            if (options != null)
            {
                image.Image.Mutate(x => x.Resize(options));
            }

            return image;
        }

        private static ResizeOptions GetResizeOptions(
            Image image,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
        {
            if (!commands.Contains(Width) && !commands.Contains(Height))
            {
                return null;
            }

            Size size = ParseSize(image, commands, parser, culture);

            if (size.Width <= 0 && size.Height <= 0)
            {
                return null;
            }

            var options = new ResizeOptions
            {
                Size = size,
                CenterCoordinates = GetCenter(commands, parser, culture),
                Position = GetAnchor(commands, parser, culture),
                Mode = GetMode(commands, parser, culture),
                Compand = GetCompandMode(commands, parser, culture),
            };

            // Defaults to Bicubic if not set.
            options.Sampler = GetSampler(commands);

            return options;
        }

        private static Size ParseSize(
            Image image,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
        {
            // The command parser will reject negative numbers as it clamps values to ranges.
            int width = (int)parser.ParseValue<uint>(commands.GetValueOrDefault(Width), culture);
            int height = (int)parser.ParseValue<uint>(commands.GetValueOrDefault(Height), culture);

            // Browsers now implement 'image-orientation: from-image' by default.
            // https://developer.mozilla.org/en-US/docs/web/css/image-orientation
            // This makes orientation handling confusing for users who expect images to be resized in accordance
            // to what they observe rather than pure (and correct) methods.
            //
            // To accomodate this we parse the dimensions to use based upon decoded EXIF orientation values, switching
            // the width/height parameters when images are rotated (not flipped).
            // We default to 'true' for EXIF orientation handling. By passing 'false' it can be turned off.
            if (!commands.Contains(Orient) || parser.ParseValue<bool>(commands.GetValueOrDefault(Orient), culture))
            {
                if (image.Metadata.ExifProfile != null)
                {
                    IExifValue<ushort> orientation = image.Metadata.ExifProfile.GetValue(ExifTag.Orientation);
                    return orientation.Value switch
                    {
                        // LeftTop, RightTop, RightBottom, LeftBottom
                        5 or 6 or 7 or 8 => new Size(height, width),
                        _ => new Size(width, height),
                    };
                }
            }

            return new Size(width, height);
        }

        private static PointF? GetCenter(
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
        {
            float[] coordinates = parser.ParseValue<float[]>(commands.GetValueOrDefault(Xy), culture);

            if (coordinates.Length != 2)
            {
                return null;
            }

            return new PointF(coordinates[0], coordinates[1]);
        }

        private static ResizeMode GetMode(
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
            => parser.ParseValue<ResizeMode>(commands.GetValueOrDefault(Mode), culture);

        private static AnchorPositionMode GetAnchor(
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
            => parser.ParseValue<AnchorPositionMode>(commands.GetValueOrDefault(Anchor), culture);

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
    }
}
