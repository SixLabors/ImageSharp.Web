// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors
{
    /// <summary>
    /// Allows setting of the background color.
    /// </summary>
    public class BackgroundColorWebProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command for changing the background color.
        /// </summary>
        public const string Color = "bgcolor";

        /// <summary>
        /// The reusable collection of commands.
        /// </summary>
        private static readonly IEnumerable<string> ColorCommands
            = new[] { Color };

        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = ColorCommands;

        /// <inheritdoc/>
        public FormattedImage Process(
            FormattedImage image,
            ILogger logger,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
        {
            Color background = parser.ParseValue<Color>(commands.GetValueOrDefault(Color), culture);

            if (background != default)
            {
                image.Image.Mutate(x => x.BackgroundColor(background));
            }

            return image;
        }
    }
}
