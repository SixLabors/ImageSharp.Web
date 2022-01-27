// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors
{
    /// <summary>
    /// Specifies the contract for processing images using a querystring URI API.
    /// </summary>
    public interface IImageWebProcessor
    {
        /// <summary>
        /// Gets the collection of recognized command keys.
        /// </summary>
        IEnumerable<string> Commands { get; }

        /// <summary>
        /// Processes the image based on the given commands.
        /// </summary>
        /// <param name="image">The image to process.</param>
        /// <param name="logger">The type used for performing logging.</param>
        /// <param name="commands">The ordered collection containing the processing commands.</param>
        /// <param name="parser">The command parser use for parting commands.</param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> to use as the current parsing culture.
        /// </param>
        /// <returns>The <see cref="FormattedImage"/>.</returns>
        FormattedImage Process(
            FormattedImage image,
            ILogger logger,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture);
    }
}
