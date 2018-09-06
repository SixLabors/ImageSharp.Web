// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SixLabors.ImageSharp.Web.Commands
{
    /// <summary>
    /// Defines a contract for parsing commands from image requests.
    /// </summary>
    public interface IRequestParser
    {
        /// <summary>
        /// Returns a collection of commands from the current request.
        /// </summary>
        /// <param name="context">Encapsulates all HTTP-specific information about an individual HTTP request.</param>
        /// <returns>The <see cref="IDictionary{TKey,TValue}"/>.</returns>
        IDictionary<string, string> ParseRequestCommands(HttpContext context);
    }
}