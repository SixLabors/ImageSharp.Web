// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Processors;

/// <summary>
/// Allows the auto-orientation to ensure that EXIF defined orientation is
/// reflected in the final image.
/// </summary>
public class AutoOrientWebProcessor : IImageWebProcessor
{
    /// <summary>
    /// The command for changing the orientation according to the EXIF information.
    /// </summary>
    public const string AutoOrient = "autoorient";

    /// <summary>
    /// The reusable collection of commands.
    /// </summary>
    private static readonly IEnumerable<string> AutoOrientCommands
        = new[] { AutoOrient };

    /// <inheritdoc/>
    public IEnumerable<string> Commands { get; } = AutoOrientCommands;

    /// <inheritdoc/>
    public FormattedImage Process(
        FormattedImage image,
        ILogger logger,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        if (parser.ParseValue<bool>(commands.GetValueOrDefault(AutoOrient), culture))
        {
            image.Image.Mutate(x => x.AutoOrient());
        }

        return image;
    }

    /// <inheritdoc/>
    public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture) => false;
}
