// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Middleware;

/// <summary>
/// Contains information about the current image request and parsed commands.
/// </summary>
public class ImageCommandContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageCommandContext"/> class.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <param name="commands">The dictionary containing the collection of URI derived processing commands.</param>
    /// <param name="parser">The command parser for parsing URI derived processing commands.</param>
    /// <param name="culture">The culture used to parse commands.</param>
    public ImageCommandContext(
        HttpContext context,
        CommandCollection commands,
        CommandParser parser,
        CultureInfo culture)
    {
        this.Context = context;
        this.Commands = commands;
        this.Parser = parser;
        this.Culture = culture;
    }

    /// <summary>
    /// Gets the current HTTP request context.
    /// </summary>
    public HttpContext Context { get; }

    /// <summary>
    /// Gets the collection of URI derived processing commands.
    /// </summary>
    public CommandCollection Commands { get; }

    /// <summary>
    /// Gets the command parser for parsing URI derived processing commands.
    /// </summary>
    public CommandParser Parser { get; }

    /// <summary>
    /// Gets the culture used for parsing commands.
    /// </summary>
    public CultureInfo Culture { get; }
}
