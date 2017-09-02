// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Middleware
{
    /// <summary>
    /// Contains information about the current image request and parsed commands.
    /// </summary>
    public class ImageValidationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageValidationContext"/> class.
        /// </summary>
        /// <param name="context">The current HTTP request context</param>
        /// <param name="commands">The dictionary containing the collection of URI derived processing commands.</param>
        /// <param name="parser">The command parser for parsing URI derived processing commands.</param>
        public ImageValidationContext(HttpContext context, IDictionary<string, string> commands, CommandParser parser)
        {
            this.Context = context;
            this.Commands = commands;
            this.Parser = parser;
        }

        /// <summary>
        /// Gets the current HTTP request context
        /// </summary>
        public HttpContext Context { get; }

        /// <summary>
        /// Gets the dictionary containing the collection of URI derived processing commands.
        /// </summary>
        public IDictionary<string, string> Commands { get; }

        /// <summary>
        /// Gets the command parser for parsing URI derived processing commands.
        /// </summary>
        public CommandParser Parser { get; }
    }
}