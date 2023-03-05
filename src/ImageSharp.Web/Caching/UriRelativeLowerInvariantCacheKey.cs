// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Caching;

/// <summary>
/// Creates a case insensitive cache key based on the request path and commands.
/// </summary>
public class UriRelativeLowerInvariantCacheKey : ICacheKey
{
    /// <inheritdoc/>
    public string Create(HttpContext context, CommandCollection commands)
        => CaseHandlingUriBuilder.BuildRelative(CaseHandlingUriBuilder.CaseHandling.LowerInvariant, context.Request.PathBase, context.Request.Path, QueryString.Create(commands));
}
