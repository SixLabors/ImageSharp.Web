// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace SixLabors.ImageSharp.Web.Commands
{
    /// <summary>
    /// Configuration options for the <see cref="PresetRequestParser"/>.
    /// </summary>
    public class PresetRequestParserOptions
    {
        /// <summary>
        /// Gets or sets the presets, which is a Dictionary of preset names to command query strings.
        /// </summary>
        public IDictionary<string, string> Presets { get; set; } = new Dictionary<string, string>();
    }
}
