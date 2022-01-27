// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Creates a cache key based on the request scheme, host, path and commands.
    /// </summary>
    public class UriAbsoluteCacheKey : ICacheKey
    {
        /// <inheritdoc/>
        public string Create(HttpContext context, CommandCollection commands, bool caseSensitive)
        {
            string cacheKey = UriHelper.BuildAbsolute(context.Request.Scheme, context.Request.Host, context.Request.PathBase, context.Request.Path, QueryString.Create(commands));

            return caseSensitive ? cacheKey : cacheKey.ToLowerInvariant();
        }
    }
}
