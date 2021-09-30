// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

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

            Dictionary<string, StringValues> parsed = QueryHelpers.ParseQuery(context.Request.QueryString.ToUriComponent());
            var transformed = new Dictionary<string, string>(parsed.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, StringValues> pair in parsed)
            {
                transformed[pair.Key] = pair.Value.ToString();
            }

            return transformed;
        }
    }
}
