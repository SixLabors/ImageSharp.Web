// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Processors
{
    public class FormatWebProcessorTests
    {
        [Fact]
        public void FormatWebProcessor_UpdatesFormat()
        {
            var parser = new CommandParser(Array.Empty<ICommandConverter>());
            CultureInfo culture = CultureInfo.InvariantCulture;

            var commands = new Dictionary<string, string>
            {
                { FormatWebProcessor.Format, GifFormat.Instance.Name },
            };

            using var image = new Image<Rgba32>(1, 1);
            using var formatted = new FormattedImage(image, PngFormat.Instance);
            Assert.Equal(formatted.Format, PngFormat.Instance);

            new FormatWebProcessor(Options.Create(new ImageSharpMiddlewareOptions()))
                .Process(formatted, null, commands, parser, culture);

            Assert.Equal(formatted.Format, GifFormat.Instance);
        }
    }
}
