// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SixLabors.ImageSharp.Web.Processors
{
    /// <summary>
    /// Extends the image API to allow processing via a URI API
    /// </summary>
    internal static class WebProcessingExtensions
    {
        /// <summary>
        /// Loops through the available processors and updates the image if any match
        /// </summary>
        /// <param name="source">The image to resize.</param>
        /// <param name="logger">The type used for performing logging</param>
        /// <param name="processors">The collection of available processors</param>
        /// <param name="commands">The parsed collection of processing commands</param>
        /// <returns>The <see cref="Image{Rgba32}"/></returns>
        /// <remarks>Passing zero for one of height or width will automatically preserve the aspect ratio of the original image</remarks>
        public static FormattedImage Process(this FormattedImage source, ILogger logger, IEnumerable<IImageWebProcessor> processors, IDictionary<string, string> commands)
        {
            foreach (IImageWebProcessor processor in processors)
            {
                source = processor.Process(source, logger, commands);
            }

            return source;
        }
    }
}