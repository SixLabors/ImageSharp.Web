// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Contains reusable static instances of known image formats.
    /// </summary>
    public static class Format
    {
        /// <summary>
        /// Gets the Bmp encoding format.
        /// </summary>
        public static FormatCommand Bmp { get; } = new(nameof(Bmp));

        /// <summary>
        /// Gets the Gif encoding format.
        /// </summary>
        public static FormatCommand Gif { get; } = new(nameof(Gif));

        /// <summary>
        /// Gets the Jpg encoding format.
        /// </summary>
        public static FormatCommand Jpg { get; } = new(nameof(Jpg));

        /// <summary>
        /// Gets the Gif encoding format.
        /// </summary>
        public static FormatCommand Png { get; } = new(nameof(Png));

        /// <summary>
        /// Gets the Bmp encoding format.
        /// </summary>
        public static FormatCommand Tga { get; } = new(nameof(Tga));

        /// <summary>
        /// Gets the Gif encoding format.
        /// </summary>
        public static FormatCommand WebP { get; } = new(nameof(WebP));
    }
}
