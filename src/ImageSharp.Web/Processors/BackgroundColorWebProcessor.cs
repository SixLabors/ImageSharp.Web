// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Overlays;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors
{
    /// <summary>
    /// Allows setting of the background color
    /// </summary>
    public class BackgroundColorWebProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command for changing the background color
        /// </summary>
        public const string Color = "bgcolor";

        /// <summary>
        /// The reusable collection of commands
        /// </summary>
        private static readonly IEnumerable<string> ColorCommands
            = new[] { Color };

        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = ColorCommands;

        /// <inheritdoc/>
        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands)
        {
            Rgba32 background = CommandParser.Instance.ParseValue<Rgba32>(commands.GetValueOrDefault(Color));

            if (background != default)
            {
                image.Image.Mutate(x => x.BackgroundColor(background));
            }

            return image;
        }
    }
}