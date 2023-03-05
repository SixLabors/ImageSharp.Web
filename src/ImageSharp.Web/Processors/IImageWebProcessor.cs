// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors;

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

    /// <summary>
    /// <para>
    /// Returns a value indicating whether the image to be processed should be decoded using a 32 bit True Color pixel format - 8 bits per color component
    /// plus an 8 bit alpha channel <see href="https://en.wikipedia.org/wiki/Color_depth#True_color_(24-bit)"/>.
    /// </para>
    /// <para>This method is used to determine whether optimizations can be enabled to reduce memory consumption during processing.</para>
    /// </summary>
    /// <param name="commands">The ordered collection containing the processing commands.</param>
    /// <param name="parser">The command parser use for parting commands.</param>
    /// <param name="culture">
    /// The <see cref="CultureInfo"/> to use as the current parsing culture.
    /// </param>
    /// <returns>The <see cref="bool"/> indicating whether a 32 bit True Color pixel format is required.</returns>
    bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture);
}
