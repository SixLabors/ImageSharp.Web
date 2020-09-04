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
        /// The command constant for the resize sampler.
        /// </summary>
        public const string Anchor = "ranchor";

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
                Compand
            };

        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = ResizeCommands;

        /// <inheritdoc/>
        public FormattedImage Process(
            FormattedImage image,
            ILogger logger,
            IDictionary<string, string> commands,
            CommandParser parser,
            CultureInfo culture)
        {
            ResizeOptions options = GetResizeOptions(commands, parser, culture);

            if (options != null)
            {
                image.Image.Mutate(x => x.Resize(options));
            }

            return image;
        }

        private static ResizeOptions GetResizeOptions(
            IDictionary<string, string> commands,
            CommandParser parser,
            CultureInfo culture)
        {
            if (!commands.ContainsKey(Width) && !commands.ContainsKey(Height))
            {
                return null;
            }

            Size size = ParseSize(commands, parser, culture);

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
            IDictionary<string, string> commands,
            CommandParser parser,
            CultureInfo culture)
        {
            // The command parser will reject negative numbers as it clamps values to ranges.
            uint width = parser.ParseValue<uint>(commands.GetValueOrDefault(Width), culture);
            uint height = parser.ParseValue<uint>(commands.GetValueOrDefault(Height), culture);

            return new Size((int)width, (int)height);
        }

        private static PointF? GetCenter(
            IDictionary<string, string> commands,
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
            IDictionary<string, string> commands,
            CommandParser parser,
            CultureInfo culture)
            => parser.ParseValue<ResizeMode>(commands.GetValueOrDefault(Mode), culture);

        private static AnchorPositionMode GetAnchor(
            IDictionary<string, string> commands,
            CommandParser parser,
            CultureInfo culture)
            => parser.ParseValue<AnchorPositionMode>(commands.GetValueOrDefault(Anchor), culture);

        private static bool GetCompandMode(
            IDictionary<string, string> commands,
            CommandParser parser,
            CultureInfo culture)
            => parser.ParseValue<bool>(commands.GetValueOrDefault(Compand), culture);

        private static IResampler GetSampler(IDictionary<string, string> commands)
        {
            string sampler = commands.GetValueOrDefault(Sampler);

            if (sampler != null)
            {
                switch (sampler.ToLowerInvariant())
                {
                    case "nearest":
                    case "nearestneighbor":
                        return KnownResamplers.NearestNeighbor;
                    case "box": return KnownResamplers.Box;
                    case "mitchell":
                    case "mitchellnetravali":
                        return KnownResamplers.MitchellNetravali;
                    case "catmull":
                    case "catmullrom":
                        return KnownResamplers.CatmullRom;
                    case "lanczos2": return KnownResamplers.Lanczos2;
                    case "lanczos3": return KnownResamplers.Lanczos3;
                    case "lanczos5": return KnownResamplers.Lanczos5;
                    case "lanczos8": return KnownResamplers.Lanczos8;
                    case "welch": return KnownResamplers.Welch;
                    case "robidoux": return KnownResamplers.Robidoux;
                    case "robidouxsharp": return KnownResamplers.RobidouxSharp;
                    case "spline": return KnownResamplers.Spline;
                    case "triangle": return KnownResamplers.Triangle;
                    case "hermite": return KnownResamplers.Hermite;
                }
            }

            return KnownResamplers.Bicubic;
        }
    }
}
