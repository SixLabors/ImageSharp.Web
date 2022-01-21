// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

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
        public CommandCollection ParseRequestCommands(HttpContext context)
        {
            if (context.Request.Query.Count == 0)
            {
                return new();
            }

            // TODO: Investigate skipping the double allocation here.
            // In .NET 6 we can directly use the QueryStringEnumerable type and enumerate stright to our command collection
            Dictionary<string, StringValues> parsed = QueryHelpers.ParseQuery(context.Request.QueryString.ToUriComponent());
            CommandCollection transformed = new();
            foreach (KeyValuePair<string, StringValues> pair in parsed)
            {
                transformed.Add(new(pair.Key, pair.Value.ToString()));
            }

            return transformed;
        }
    }
}
