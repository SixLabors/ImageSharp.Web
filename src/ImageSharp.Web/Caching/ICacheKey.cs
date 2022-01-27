// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Defines a contract that allows the creation of cache keys (used by <see cref="ICacheHash"/> to create hashed file names for storing cached images).
    /// </summary>
    public interface ICacheKey
    {
        /// <summary>
        /// Creates the cache key based on the specified context and commands.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="commands">The commands.</param>
        /// <param name="caseSensitive">If set to <c>true</c> the cache key should be generated in a case sensitive manner.</param>
        /// <returns>
        /// The cache key.
        /// </returns>
        string Create(HttpContext context, CommandCollection commands, bool caseSensitive);
    }
}
