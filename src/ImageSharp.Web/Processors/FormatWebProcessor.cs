// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Processors
{
    /// <summary>
    /// Allows the changing of image formats.
    /// </summary>
    public class FormatWebProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command constant for format
        /// </summary>
        public const string Format = "format";

        /// <summary>
        /// The reusable collection of commands
        /// </summary>
        private static readonly IEnumerable<string> FormatCommands
            = new[] { Format };

        /// <summary>
        /// The middleware configuration options
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatWebProcessor"/> class.
        /// </summary>
        /// <param name="options">The middleware configuration options</param>
        public FormatWebProcessor(IOptions<ImageSharpMiddlewareOptions> options)
        {
            this.options = options.Value;
        }

        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = FormatCommands;

        /// <inheritdoc/>
        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands)
        {
            string extension = commands.GetValueOrDefault(Format);

            if (!string.IsNullOrWhiteSpace(extension))
            {
                IImageFormat format = this.options.Configuration.ImageFormatsManager.FindFormatByFileExtension(extension);

                if (format != null)
                {
                    image.Format = format;
                }
            }

            return image;
        }
    }
}