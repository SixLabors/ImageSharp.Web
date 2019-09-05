// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class MockWebProcessor : IImageWebProcessor
    {
        public IEnumerable<string> Commands => Array.Empty<string>();

        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands) => image;
    }
}
