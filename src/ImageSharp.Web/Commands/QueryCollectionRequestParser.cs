// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace SixLabors.ImageSharp.Web.Commands
{
    /// <summary>
    /// Parses commands from the request querystring.
    /// </summary>
    public class QueryCollectionRequestParser : IRequestParser
    {
        /// <inheritdoc/>
        public IDictionary<string, string> ParseRequestCommands(HttpContext context)
        {
            if (!context.Request.Query.Any())
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            return QueryHelpers.ParseQuery(context.Request.QueryString.ToUriComponent())
                               .ToDictionary(k => k.Key, v => v.Value.ToString());
        }
    }
}