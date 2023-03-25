// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Contains reusable static instances of known image formats.
/// </summary>
public static class Format
{
    /// <summary>
    /// Gets the Bmp encoding format.
    /// </summary>
    public static FormatCommand Bmp { get; } = new("bmp");

    /// <summary>
    /// Gets the Gif encoding format.
    /// </summary>
    public static FormatCommand Gif { get; } = new("gif");

    /// <summary>
    /// Gets the Jpg encoding format.
    /// </summary>
    public static FormatCommand Jpg { get; } = new("jpg");

    /// <summary>
    /// Gets the Png encoding format.
    /// </summary>
    public static FormatCommand Png { get; } = new("png");

    /// <summary>
    /// Gets the Tga encoding format.
    /// </summary>
    public static FormatCommand Tga { get; } = new("tga");

    /// <summary>
    /// Gets the WebP encoding format.
    /// </summary>
    public static FormatCommand WebP { get; } = new("webp");
}
