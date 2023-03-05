// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace SixLabors.ImageSharp.Web.Commands;

/// <summary>
/// Parses commands from the request querystring.
/// </summary>
public sealed class QueryCollectionRequestParser : IRequestParser
{
    /// <inheritdoc/>
    public CommandCollection ParseRequestCommands(HttpContext context)
    {
        IQueryCollection query = context.Request.Query;
        if (query is null || query.Count == 0)
        {
            // We return new to ensure the collection is still mutable via events.
            return new();
        }

        CommandCollection transformed = new();
        foreach (KeyValuePair<string, StringValues> pair in query)
        {
            // Use the indexer for both set and query. This replaces any previously parsed values.
            string? value = pair.Value[^1];
            if (value is not null)
            {
                transformed[pair.Key] = value;
            }
        }

        return transformed;
    }
}
