// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors
{
    /// <summary>
    /// Extends the image API to allow processing via a URI API.
    /// </summary>
    internal static class WebProcessingExtensions
    {
        /// <summary>
        /// Loops through the available processors and updates the image if any match.
        /// </summary>
        /// <param name="source">The image to resize.</param>
        /// <param name="logger">The type used for performing logging.</param>
        /// <param name="processors">The collection of available processors.</param>
        /// <param name="commands">The parsed collection of processing commands.</param>
        /// <param name="commandParser">The command parser use for parting commands.</param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> to use as the current parsing culture.
        /// </param>
        /// <returns>The <see cref="FormattedImage"/>.</returns>
        public static FormattedImage Process(
            this FormattedImage source,
            ILogger logger,
            IEnumerable<IImageWebProcessor> processors,
            IDictionary<string, string> commands,
            CommandParser commandParser,
            CultureInfo culture)
        {
            if (commands.Count != 0)
            {
                foreach (IImageWebProcessor processor in processors)
                {
                    source = processor.Process(source, logger, commands, commandParser, culture);
                }
            }

            return source;
        }
    }
}
