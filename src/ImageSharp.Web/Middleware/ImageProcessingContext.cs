// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace SixLabors.ImageSharp.Web.Middleware
{
    /// <summary>
    /// Contains information about the current image request and processed image.
    /// </summary>
    public class ImageProcessingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProcessingContext"/> class.
        /// </summary>
        /// <param name="context">The current HTTP request context</param>
        /// <param name="stream">The stream containing the processed image bytes</param>
        /// <param name="commands">The parsed collection of processing commands</param>
        /// <param name="extension">The file extension for the processed image</param>
        public ImageProcessingContext(HttpContext context, Stream stream, IDictionary<string, string> commands, string extension)
        {
            this.Context = context;
            this.Stream = stream;
            this.Commands = commands;
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
        /// Gets the parsed collection of processing commands
        /// </summary>
        public IDictionary<string, string> Commands { get; }

        /// <summary>
        /// Gets the file extension for the processed image.
        /// </summary>
        public string Extension { get; }
    }
}