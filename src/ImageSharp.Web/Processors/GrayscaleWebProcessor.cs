// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors;

/// <summary>
/// Allows to apply grayscale toning to the image
/// </summary>
public class GrayscaleWebProcessor : IImageWebProcessor
{
    /// <summary>
    /// The command for applying the grayscale mode. See <see cref="GrayscaleMode"/> for possible values
    /// </summary>
    public const string Grayscale = "grayscale";

    /// <summary>
    /// The reusable collection of commands.
    /// </summary>
    private static readonly IEnumerable<string> GrayscaleCommands
        = new[] { Grayscale };

    /// <inheritdoc/>
    public IEnumerable<string> Commands { get; } = GrayscaleCommands;

    /// <inheritdoc/>
    public FormattedImage Process(
        FormattedImage image,
        ILogger logger,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        string? grayscaleCommandValue = commands.GetValueOrDefault(Grayscale);

        if (grayscaleCommandValue is null)
        {
            return image;
        }

        GrayscaleMode grayscaleMode = parser.ParseValue<GrayscaleMode>(grayscaleCommandValue, culture);

        image.Image.Mutate(x => x.Grayscale(grayscaleMode));

        return image;
    }

    /// <inheritdoc/>
    public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture) =>
        false;
}
