// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// A class encapsulating an image with a particular file encoding.
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public sealed class FormattedImage : IDisposable
    {
        private readonly ImageFormatManager imageFormatsManager;
        private IImageFormat format;
        private IImageEncoder encoder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedImage"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="format">The format.</param>
        internal FormattedImage(Image<Rgba32> image, IImageFormat format)
        {
            this.Image = image;
            this.imageFormatsManager = image.GetConfiguration().ImageFormatsManager;
            this.Format = format;
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
            set
            {
                if (value is null)
                {
                    ThrowInvalid(nameof(value));
                }

                this.format = value;
                this.encoder = this.imageFormatsManager.FindEncoder(value);
            }
        }

        /// <summary>
        /// Gets or sets the encoder.
        /// </summary>
        public IImageEncoder Encoder
        {
            get => this.encoder;
            set
            {
                if (value is null)
                {
                    ThrowInvalid(nameof(value));
                }

                // The given type should match the format encoder.
                IImageEncoder reference = this.imageFormatsManager.FindEncoder(this.Format);
                if (reference.GetType() != value.GetType())
                {
                    ThrowInvalid(nameof(value));
                }

                this.encoder = value;
            }
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
        /// Saves image to the specified destination stream.
        /// </summary>
        /// <param name="destination">The destination stream.</param>
        public void Save(Stream destination) => this.Image.Save(destination, this.encoder);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Image?.Dispose();
            this.Image = null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalid(string name) => throw new ArgumentNullException(name);
    }
}
