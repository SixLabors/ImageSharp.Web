// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace SixLabors.ImageSharp.Web.Commands
{
    public class PresetRequestParserOptions
    {
        public IDictionary<string, string> Presets { get; set; } = new Dictionary<string, string>();
    }
}
