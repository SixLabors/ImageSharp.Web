// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
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
            var commands = new Dictionary<string, string>
            {
                { FormatWebProcessor.Format, GifFormat.Instance.Name },
            };

            using var image = new Image<Rgba32>(1, 1);
            using var formatted = new FormattedImage(image, PngFormat.Instance);
            Assert.Equal(formatted.Format, PngFormat.Instance);

            new FormatWebProcessor(Options.Create(new ImageSharpMiddlewareOptions()))
                .Process(formatted, null, commands);

            Assert.Equal(formatted.Format, GifFormat.Instance);
        }
    }
}
