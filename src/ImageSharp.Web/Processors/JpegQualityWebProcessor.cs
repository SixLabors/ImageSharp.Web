// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors
{
    /// <summary>
    /// Allows the setting of quality for the jpeg image format.
    /// </summary>
    public class JpegQualityWebProcessor : IImageWebProcessor
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
            IDictionary<string, string> commands,
            CommandParser parser,
            CultureInfo culture)
        {
            if (commands.ContainsKey(Quality) && image.Format is JpegFormat)
            {
                // The encoder clamps any values so no validation is required.
                int quality = parser.ParseValue<int>(commands.GetValueOrDefault(Quality), culture);
                image.Encoder = new JpegEncoder() { Quality = quality };
            }

            return image;
        }
    }
}
