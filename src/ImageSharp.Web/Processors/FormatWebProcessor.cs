// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Processors;

/// <summary>
/// Allows the changing of image formats.
/// </summary>
public class FormatWebProcessor : IImageWebProcessor
{
    /// <summary>
    /// The command constant for format.
    /// </summary>
    public const string Format = "format";

    /// <summary>
    /// The reusable collection of commands.
    /// </summary>
    private static readonly IEnumerable<string> FormatCommands
        = new[] { Format };

    /// <summary>
    /// The middleware configuration options.
    /// </summary>
    private readonly ImageSharpMiddlewareOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatWebProcessor"/> class.
    /// </summary>
    /// <param name="options">The middleware configuration options.</param>
    public FormatWebProcessor(IOptions<ImageSharpMiddlewareOptions> options)
        => this.options = options.Value;

    /// <inheritdoc/>
    public IEnumerable<string> Commands { get; } = FormatCommands;

    /// <inheritdoc/>
    public FormattedImage Process(
        FormattedImage image,
        ILogger logger,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        string? extension = commands.GetValueOrDefault(Format);

        if (!string.IsNullOrWhiteSpace(extension)
            && this.options.Configuration.ImageFormatsManager.TryFindFormatByFileExtension(extension, out IImageFormat? format))
        {
            image.Format = format;
        }

        return image;
    }

    /// <inheritdoc/>
    public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture) => false;
}
