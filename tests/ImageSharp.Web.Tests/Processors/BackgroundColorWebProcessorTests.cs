// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web.Processors;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Processors
{
    public class BackgroundColorWebProcessorTests
    {
        [Fact]
        public void BackgroundColorWebProcessor_UpdatesColor()
        {
            var commands = new Dictionary<string, string>
            {
                { BackgroundColorWebProcessor.Color, nameof(Color.Orange) }
            };

            using var image = new Image<Rgba32>(1, 1);
            Assert.True(Color.Transparent.Equals(image[0, 0]));

            using var formatted = new FormattedImage(image, PngFormat.Instance);
            new BackgroundColorWebProcessor().Process(formatted, null, commands);

            Assert.True(Color.Orange.Equals(image[0, 0]));
        }
    }
}
