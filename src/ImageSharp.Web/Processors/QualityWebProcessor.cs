// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors
{
    /// <summary>
    /// Allows the setting of quality for the jpeg image format.
    /// </summary>
    public class QualityWebProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command constant for quality.
        /// </summary>
        public const string Quality = "quality";

        /// <summary>
        /// The reusable collection of commands.
        /// </summary>
        private static readonly IEnumerable<string> QualityCommands
            = new[] { Quality };

        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = QualityCommands;

        /// <inheritdoc/>
        public FormattedImage Process(
            FormattedImage image,
            ILogger logger,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
        {
            if (commands.Contains(Quality))
            {
                // The encoders clamp any values so no validation is required.
                int quality = parser.ParseValue<int>(commands.GetValueOrDefault(Quality), culture);

                if (image.Format is JpegFormat)
                {
                    var reference =
                        (JpegEncoder)image.Image
                        .GetConfiguration()
                        .ImageFormatsManager
                        .FindEncoder(image.Format);

                    if (quality != reference.Quality)
                    {
                        image.Encoder = new JpegEncoder() { Quality = quality, ColorType = reference.ColorType };
                    }
                }
                else if (image.Format is WebpFormat)
                {
                    var reference =
                        (WebpEncoder)image.Image
                        .GetConfiguration()
                        .ImageFormatsManager
                        .FindEncoder(image.Format);

                    image.Encoder = new WebpEncoder()
                    {
                        FileFormat = quality < 100 ? WebpFileFormatType.Lossy : reference.FileFormat,
                        Quality = quality,
                        Method = reference.Method,
                        UseAlphaCompression = reference.UseAlphaCompression,
                        EntropyPasses = reference.EntropyPasses,
                        SpatialNoiseShaping = reference.SpatialNoiseShaping,
                        FilterStrength = reference.FilterStrength,
                        TransparentColorMode = reference.TransparentColorMode,
                        NearLossless = reference.NearLossless,
                        NearLosslessQuality = reference.NearLosslessQuality,
                    };
                }
            }

            return image;
        }

        /// <inheritdoc/>
        public bool RequiresAlphaComponent(CommandCollection commands, CommandParser parser, CultureInfo culture) => false;
    }
}
