// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Commands;

/// <summary>
/// Extension methods for <see cref="CommandCollectionExtensions"/>.
/// </summary>
public static class CommandCollectionExtensions
{
    /// <summary>
    /// Gets the value associated with the specified key or the default value.
    /// </summary>
    /// <param name="collection">The collection instance.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key or the default value.</returns>
    public static string? GetValueOrDefault(this CommandCollection collection, string key)
    {
        collection.TryGetValue(key, out KeyValuePair<string, string?> result);
        return result.Value;
    }
}
