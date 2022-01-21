// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace SixLabors.ImageSharp.Web.Commands
{
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
            if (context.Request.Query.Count == 0 || !context.Request.Query.ContainsKey(QueryKey))
            {
                return new();
            }

            string requestedPreset = context.Request.Query["preset"][0];
            return this.presets.GetValueOrDefault(requestedPreset) ?? new();
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
                transformed.Add(new(pair.Key, pair.Value.ToString()));
            }

            return transformed;
        }
    }
}
