// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class MockWebProcessor : IImageWebProcessor
    {
        public IEnumerable<string> Commands => Array.Empty<string>();

        public FormattedImage Process(
            FormattedImage image,
            ILogger logger,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
            => image;

        public bool RequiresAlphaAwarePixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture) => false;
    }
}
