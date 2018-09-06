// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace SixLabors.ImageSharp.Web.Middleware
{
    /// <summary>
    /// Configuration options for the ImageSharp middleware.
    /// </summary>
    public class ImageSharpMiddlewareOptions
    {
        /// <summary>
        /// Gets or sets the base library configuration.
        /// </summary>
        public Configuration Configuration { get; set; } = Configuration.Default;

        /// <summary>
        /// Gets or sets the number of days to store images in the browser cache.
        /// </summary>
        public int MaxBrowserCacheDays { get; set; } = 7;

        /// <summary>
        /// Gets or sets the number of days to store images in the image cache.
        /// </summary>
        public int MaxCacheDays { get; set; } = 365;

        /// <summary>
        /// Gets or sets the length of the filename to use (minus the extension) when storing images in the image cache.
        /// </summary>
        public uint CachedNameLength { get; set; } = 12;

        /// <summary>
        /// Gets or sets the additional command parsing method that can be used to used to augment commands.
        /// This is called once the commands have been gathered and before an <see cref="IImageProvider"/> has been assigned.
        /// </summary>
        public Action<ImageCommandContext> OnParseCommands { get; set; } = c =>
            {
                // It's a good idea to have this to provide very basic security.
                // We can safely use the static resize processor properties.
                uint width = c.Parser.ParseValue<uint>(c.Commands.GetValueOrDefault(ResizeWebProcessor.Width));
                uint height = c.Parser.ParseValue<uint>(c.Commands.GetValueOrDefault(ResizeWebProcessor.Height));

                if (width > 4000 && height > 4000)
                {
                    c.Commands.Remove(ResizeWebProcessor.Width);
                    c.Commands.Remove(ResizeWebProcessor.Height);
                }
            };

        /// <summary>
        /// Gets or sets the additional method that can be used for final manipulation before the image is saved.
        /// This is called after image has been processed, but before the image has been saved to the output stream for caching.
        /// This can be used to alter the metadata of the resultant image.
        /// </summary>
        public Action<FormattedImage> OnBeforeSave { get; set; } = _ => { };

        /// <summary>
        /// Gets or sets the additional processing method.
        /// This is called after image has been processed, but before the result has been cached.
        /// This can be used to further optimize the resultant image.
        /// </summary>
        public Action<ImageProcessingContext> OnProcessed { get; set; } = _ => { };

        /// <summary>
        /// Gets or sets the additional response method.
        /// This is called after the status code and headers have been set, but before the body has been written.
        /// This can be used to add or change the response headers.
        /// </summary>
        public Action<HttpContext> OnPrepareResponse { get; set; } = _ => { };
    }
}