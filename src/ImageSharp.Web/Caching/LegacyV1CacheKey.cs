// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Caching;

/// <summary>
/// Maintained for compatibility purposes only this cache key implementation generates the same
/// out as the V1 middleware. If possible, it is recommended to use the <see cref="UriRelativeLowerInvariantCacheKey"/>.
/// </summary>
public class LegacyV1CacheKey : ICacheKey
{
    /// <inheritdoc/>
    public string Create(HttpContext context, CommandCollection commands)
    {
        StringBuilder sb = new(context.Request.Host.ToString());

        string pathBase = context.Request.PathBase.ToString();
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}/", pathBase);
        }

        string path = context.Request.Path.ToString();
        if (!string.IsNullOrWhiteSpace(path))
        {
            sb.Append(path);
        }

        sb.Append(QueryString.Create(commands));

        return sb.ToString().ToLowerInvariant();
    }
}
