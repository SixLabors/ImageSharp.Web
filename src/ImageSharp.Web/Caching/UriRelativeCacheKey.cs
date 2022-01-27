// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Creates a cache key based on the request path and commands.
    /// </summary>
    public class UriRelativeCacheKey : ICacheKey
    {
        /// <inheritdoc/>
        public virtual string Create(HttpContext context, CommandCollection commands)
            => UriHelper.BuildRelative(context.Request.PathBase, context.Request.Path, QueryString.Create(commands));
    }

    /// <summary>
    /// Creates a cache key based on the lowercased request path and commands.
    /// </summary>
    public class UriRelativeLowercaseCacheKey : UriRelativeCacheKey
    {
        /// <inheritdoc/>
        public override string Create(HttpContext context, CommandCollection commands)
            => base.Create(context, commands).ToLowerInvariant();
    }
}
