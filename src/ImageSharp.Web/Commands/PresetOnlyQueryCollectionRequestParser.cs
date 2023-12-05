// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

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
        string? requestedPreset = query[^1];
        if (requestedPreset is not null && this.presets.TryGetValue(requestedPreset, out CommandCollection? collection))
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
        CommandCollection transformed = new();
        foreach (QueryStringEnumerable.EncodedNameValuePair pair in new QueryStringEnumerable(unparsedPresetValue))
        {
            // Last value wins.
            if (pair.DecodeValue().Length > 0)
            {
                transformed[pair.DecodeName().ToString()] = pair.DecodeValue().ToString();
            }
        }

        return transformed;
    }
}
