// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// A class encapsulating an image with a particular file encoding.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public sealed class FormattedImage : IDisposable
    {
        private IImageFormat format;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedImage"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="format">The format.</param>
        public FormattedImage(Image<Rgba32> image, IImageFormat format)
        {
            this.format = format;
            this.Image = image;
        }

        /// <summary>
        /// Gets the image.
        /// </summary>
        public Image<Rgba32> Image { get; private set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        public IImageFormat Format
        {
            get => this.format;
            set => this.format = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Loads the specified source.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="source">The source.</param>
        /// <returns>The <see cref="FormattedImage"/>.</returns>
        public static FormattedImage Load(Configuration configuration, Stream source)
        {
            var image = ImageSharp.Image.Load<Rgba32>(configuration, source, out IImageFormat format);
            return new FormattedImage(image, format);
        }

        /// <summary>
        /// Saves the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        public void Save(Stream destination) => this.Image.Save(destination, this.format);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Image?.Dispose();
            this.Image = null;
        }
    }
}
