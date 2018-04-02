// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// A class encapsulating an image with a particular file encoding
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class FormattedImage : IDisposable
    {
        private IImageFormat format;
        private Image<Rgba32> image;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedImage"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="format">The format.</param>
        protected FormattedImage(Image<Rgba32> image, IImageFormat format)
        {
            this.format = format;
            this.image = image;
        }

        /// <summary>
        /// Gets the image.
        /// </summary>
        public Image<Rgba32> Image => this.image;

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        public IImageFormat Format
        {
            get => this.format;
            set => this.format = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Saves the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        public void Save(Stream destination)
        {
            this.image.Save(destination, this.format);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.image?.Dispose();
            this.image = null;
        }

        /// <summary>
        /// Loads the specified source
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="source">The source.</param>
        /// <returns>A formatted image</returns>
        public static FormattedImage Load(Configuration configuration, byte[] source)
        {
            var image = ImageSharp.Image.Load(configuration, source, out IImageFormat format);
            return new FormattedImage(image, format);
        }
    }
}
