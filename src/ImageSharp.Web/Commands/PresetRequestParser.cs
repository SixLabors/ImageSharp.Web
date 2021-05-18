// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace SixLabors.ImageSharp.Web.Commands
{
    /// <summary>
    /// Parses ImageSharp.Web image processing requests to ImageSharp.Web commands based on statically defined presets.
    /// The advantages of this are:
    /// 1. Image processing for certain scenarios can be adjusted in a single place, by adjusting the preset definition.
    /// 2. Security. Only known presets are applied. Limits DOS opportunities.
    /// </summary>
    public class PresetRequestParser : IRequestParser
    {
        private readonly IDictionary<string, IDictionary<string, string>> presets;

        public PresetRequestParser(IOptions<PresetRequestParserOptions> options) => this.presets = ParsePresets(options.Value.Presets);

        public IDictionary<string, string> ParseRequestCommands(HttpContext context)
        {
            var requestedPreset = context.Request.Query["preset"].ToString();
            return this.presets.GetValueOrDefault(requestedPreset) ?? new Dictionary<string, string>();
        }

        private static IDictionary<string, IDictionary<string, string>> ParsePresets(IDictionary<string, string> unparsedPresets) =>
            unparsedPresets
                .Select(keyValue => new KeyValuePair<string, IDictionary<string, string>>(keyValue.Key, ParsePreset(keyValue.Value)))
                .ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value);

        private static IDictionary<string, string> ParsePreset(string unparsedPresetValue)
        {
            Dictionary<string, StringValues> parsed = QueryHelpers.ParseQuery(unparsedPresetValue);
            var transformed = new Dictionary<string, string>(parsed.Count);
            foreach (KeyValuePair<string, StringValues> keyValue in parsed)
            {
                transformed[keyValue.Key] = keyValue.Value.ToString();
            }
            return transformed;
        }
    }
}
