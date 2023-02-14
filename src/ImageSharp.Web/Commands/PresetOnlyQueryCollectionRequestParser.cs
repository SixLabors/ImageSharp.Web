// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.
#nullable disable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace SixLabors.ImageSharp.Web.Commands;

/// <summary>
/// Parses preset name from the request querystring and returns the commands configured for that preset.
/// </summary>
public class PresetOnlyQueryCollectionRequestParser : IRequestParser
{
    private readonly IDictionary<string, CommandCollection> presets;

    /// <summary>
    /// The command constant for the preset query parameter.
    /// </summary>
    public const string QueryKey = "preset";

    /// <summary>
    /// Initializes a new instance of the <see cref="PresetOnlyQueryCollectionRequestParser"/> class.
    /// </summary>
    /// <param name="presetOptions">The preset options.</param>
    public PresetOnlyQueryCollectionRequestParser(IOptions<PresetOnlyQueryCollectionRequestParserOptions> presetOptions) =>
        this.presets = ParsePresets(presetOptions.Value.Presets);

    /// <inheritdoc/>
    public CommandCollection ParseRequestCommands(HttpContext context)
    {
        IQueryCollection queryCollection = context.Request.Query;
        if (queryCollection is null
            || queryCollection.Count == 0
            || !queryCollection.ContainsKey(QueryKey))
        {
            // We return new here and below to ensure the collection is still mutable via events.
            return new();
        }

        StringValues query = queryCollection[QueryKey];
        string requestedPreset = query[query.Count - 1];
        if (this.presets.TryGetValue(requestedPreset, out CommandCollection collection))
        {
            return collection;
        }

        return new();
    }

    private static IDictionary<string, CommandCollection> ParsePresets(
        IDictionary<string, string> unparsedPresets) =>
        unparsedPresets
            .Select(keyValue =>
                new KeyValuePair<string, CommandCollection>(keyValue.Key, ParsePreset(keyValue.Value)))
            .ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value, StringComparer.OrdinalIgnoreCase);

    private static CommandCollection ParsePreset(string unparsedPresetValue)
    {
        // TODO: Investigate skipping the double allocation here.
        // In .NET 6 we can directly use the QueryStringEnumerable type and enumerate stright to our command collection
        Dictionary<string, StringValues> parsed = QueryHelpers.ParseQuery(unparsedPresetValue);
        CommandCollection transformed = new();
        foreach (KeyValuePair<string, StringValues> pair in parsed)
        {
            // Use the indexer for both set and query. This replaces any previously parsed values.
            transformed[pair.Key] = pair.Value[pair.Value.Count - 1];
        }

        return transformed;
    }
}
