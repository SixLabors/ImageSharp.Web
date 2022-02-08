// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
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
        /// The command constant for the resize compand mode.
        /// </summary>
        public const string Compand = "compand";

        private static readonly IEnumerable<string> ResizeCommands = new[]
        {
            Width,
            Height,
            Xy,
            Mode,
            Sampler,
            Anchor,
            Compand,
            OrientationHelper.Command
        };

        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = ResizeCommands;

        /// <inheritdoc/>
        public FormattedImage Process(FormattedImage image, ILogger logger, CommandCollection commands, CommandParser parser, CultureInfo culture)
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
        internal static ResizeOptions GetResizeOptions(FormattedImage image, CommandCollection commands, CommandParser parser, CultureInfo culture)
        {
            if (!commands.Contains(Width) && !commands.Contains(Height))
            {
                return null;
            }

            Size size = ParseSize(commands, parser, culture);
            if (size.Width <= 0 && size.Height <= 0)
            {
                return null;
            }

            ResizeOptions options = new()
            {
                Mode = GetMode(commands, parser, culture),
                Position = GetAnchor(commands, parser, culture),
                CenterCoordinates = GetCenter(commands, parser, culture),
                Size = size,
                Sampler = GetSampler(commands),
                Compand = GetCompandMode(commands, parser, culture),
            };

            // Add support for EXIF orientation aware resizing
            if (OrientationHelper.GetOrientation(image, commands, parser, culture, out ushort orientation))
            {
                OrientationHelper.Transform(ref options, orientation);
            }

            return options;
        }

        private static Size ParseSize(CommandCollection commands, CommandParser parser, CultureInfo culture)
        {
            // The command parser will reject negative numbers as it clamps values to ranges
            int width = (int)parser.ParseValue<uint>(commands.GetValueOrDefault(Width), culture);
            int height = (int)parser.ParseValue<uint>(commands.GetValueOrDefault(Height), culture);

            return new Size(width, height);
        }

        private static PointF? GetCenter(CommandCollection commands, CommandParser parser, CultureInfo culture)
        {
            float[] coordinates = parser.ParseValue<float[]>(commands.GetValueOrDefault(Xy), culture);
            if (coordinates.Length != 2)
            {
                return null;
            }

            return new(coordinates[0], coordinates[1]);
        }

        private static ResizeMode GetMode(CommandCollection commands, CommandParser parser, CultureInfo culture)
            => parser.ParseValue<ResizeMode>(commands.GetValueOrDefault(Mode), culture);

        private static AnchorPositionMode GetAnchor(CommandCollection commands, CommandParser parser, CultureInfo culture)
            => parser.ParseValue<AnchorPositionMode>(commands.GetValueOrDefault(Anchor), culture);

        private static bool GetCompandMode(CommandCollection commands, CommandParser parser, CultureInfo culture)
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
