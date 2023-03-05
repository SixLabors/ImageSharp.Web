// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors;

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
    /// <param name="parser">The command parser use for parting commands.</param>
    /// <param name="culture">
    /// The <see cref="CultureInfo"/> to use as the current parsing culture.
    /// </param>
    /// <returns>The <see cref="FormattedImage"/>.</returns>
    public static FormattedImage Process(
        this FormattedImage source,
        ILogger logger,
        IReadOnlyList<(int Index, IImageWebProcessor Processor)> processors,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        foreach ((int Index, IImageWebProcessor Processor) processor in processors)
        {
            source = processor.Processor.Process(source, logger, commands, parser, culture);
        }

        return source;
    }

    /// <summary>
    /// Sorts the processors according to the first supported command and removes processors without supported commands.
    /// </summary>
    /// <param name="processors">The collection of available processors.</param>
    /// <param name="commands">The parsed collection of processing commands.</param>
    /// <returns>
    /// The sorted processors that supports any of the specified commands.
    /// </returns>
    public static IReadOnlyList<(int Index, IImageWebProcessor Processor)> OrderBySupportedCommands(this IEnumerable<IImageWebProcessor> processors, CommandCollection commands)
    {
        List<(int Index, IImageWebProcessor Processor)> indexedProcessors = new();
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
        return indexedProcessors;
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

    /// <summary>
    /// <para>
    /// Returns a value indicating whether the image to be processed should be decoded using a 32 bit True Color pixel format - 8 bits per color component
    /// plus an 8 bit alpha channel <see href="https://en.wikipedia.org/wiki/Color_depth#True_color_(24-bit)"/>.
    /// </para>
    /// <para>This method is used to determine whether optimizations can be enabled to reduce memory consumption during processing.</para>
    /// </summary>
    /// <param name="processors">The collection of ordered processors.</param>
    /// <param name="commands">The ordered collection containing the processing commands.</param>
    /// <param name="parser">The command parser use for parting commands.</param>
    /// <param name="culture">
    /// The <see cref="CultureInfo"/> to use as the current parsing culture.
    /// </param>
    /// <returns>The <see cref="bool"/> indicating whether a 32 bit True Color pixel format is required.</returns>
    public static bool RequiresTrueColorPixelFormat(
        this IReadOnlyList<(int Index, IImageWebProcessor Processor)> processors,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        bool requiresAlpha = false;
        foreach ((int Index, IImageWebProcessor Processor) processor in processors)
        {
            requiresAlpha |= processor.Processor.RequiresTrueColorPixelFormat(commands, parser, culture);
        }

        return requiresAlpha;
    }
}
