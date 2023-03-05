// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Middleware;

/// <summary>
/// Contains information about the current image request and processed image.
/// </summary>
public class ImageProcessingContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageProcessingContext"/> class.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <param name="stream">The stream containing the processed image bytes.</param>
    /// <param name="commands">The parsed collection of processing commands.</param>
    /// <param name="contentType">The content type for the processed image.</param>
    /// <param name="extension">The file extension for the processed image.</param>
    public ImageProcessingContext(
        HttpContext context,
        Stream stream,
        CommandCollection commands,
        string contentType,
        string extension)
    {
        this.Context = context;
        this.Stream = stream;
        this.Commands = commands;
        this.ContentType = contentType;
        this.Extension = extension;
    }

    /// <summary>
    /// Gets the current HTTP request context.
    /// </summary>
    public HttpContext Context { get; }

    /// <summary>
    /// Gets the stream containing the processed image bytes.
    /// </summary>
    public Stream Stream { get; }

    /// <summary>
    /// Gets the parsed collection of processing commands.
    /// </summary>
    public CommandCollection Commands { get; }

    /// <summary>
    /// Gets the content type for the processed image.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the file extension for the processed image.
    /// </summary>
    public string Extension { get; }
}
