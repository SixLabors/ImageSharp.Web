// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace SixLabors.ImageSharp.Web.Commands
{
    /// <summary>
    /// Parses commands from the request querystring.
    /// </summary>
    public sealed class QueryCollectionRequestParser : IRequestParser
    {
        /// <inheritdoc/>
        public IDictionary<string, string> ParseRequestCommands(HttpContext context)
        {
            if (context.Request.Query.Count == 0)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var parsed = QueryHelpers.ParseQuery(context.Request.QueryString.ToUriComponent());
            var transformed = new Dictionary<string, string>(parsed.Count);
            foreach (var pair in parsed)
            {
                transformed[pair.Key] = pair.Value.ToString();
            }

            return transformed;
        }
    }
}