// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
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
            CommandCollection commands,
            CommandParser commandParser,
            CultureInfo culture)
        {
            var commandKeys = new List<string>(commands.Keys);

            foreach (IImageWebProcessor processor in processors.GetBySupportedCommands(commandKeys))
            {
                source = processor.Process(source, logger, commands, commandParser, culture);
            }

            return source;
        }

        /// <summary>
        /// Sorts the processors according to the first supported command and removes processors without supported commands.
        /// </summary>
        /// <param name="processors">The collection of available processors.</param>
        /// <param name="commands">The parsed collection of processing commands.</param>
        /// <returns>
        /// The sorted proccessors that supports any of the specified commands.
        /// </returns>
        public static IEnumerable<IImageWebProcessor> GetBySupportedCommands(this IEnumerable<IImageWebProcessor> processors, List<string> commands)
        {
            var indexedProcessors = new List<(int Index, IImageWebProcessor Processor)>();

            foreach (IImageWebProcessor processor in processors)
            {
                // Get index of first supported command
                int index = commands.FindIndex(c => processor.IsSupportedCommand(c));
                if (index != -1)
                {
                    indexedProcessors.Add((index, processor));
                }
            }

            indexedProcessors.Sort((x, y) => x.Index.CompareTo(y.Index));

            // Return sorted processors
            foreach ((int _, IImageWebProcessor processor) in indexedProcessors)
            {
                yield return processor;
            }
        }

        /// <summary>
        /// Determines whether the specified command is supported by the processor
        /// </summary>
        /// <param name="processor">The processor.</param>
        /// <param name="command">The command.</param>
        /// <returns>
        ///   <c>true</c> if the specified command is supported by the processor; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSupportedCommand(this IImageWebProcessor processor, string command)
        {
            foreach (string processorCommand in processor.Commands)
            {
                if (processorCommand.Equals(command, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
