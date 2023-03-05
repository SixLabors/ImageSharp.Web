// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Commands;

/// <summary>
/// Configuration options for the <see cref="PresetOnlyQueryCollectionRequestParser"/>.
/// </summary>
public class PresetOnlyQueryCollectionRequestParserOptions
{
    /// <summary>
    /// Gets or sets the presets, which is a Dictionary of preset names to command query strings.
    /// </summary>
    public IDictionary<string, string> Presets { get; set; } = new Dictionary<string, string>();
}
