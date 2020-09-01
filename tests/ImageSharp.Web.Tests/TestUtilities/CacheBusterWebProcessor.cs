// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public class CacheBusterWebProcessor : IImageWebProcessor
    {
        public const string Command = "v";

        public IEnumerable<string> Commands { get; } = new[] { Command };

        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands)
        {
            return image;
        }
    }
}
