// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Processors;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Processors
{
    public class ResizeWebProcessorTests
    {
        [Fact]
        public void ResizeWebProcessor_UpdatesSize()
        {
            const int Width = 4;
            const int Height = 6;
            var commands = new Dictionary<string, string>
            {
                { ResizeWebProcessor.Sampler, nameof(KnownResamplers.NearestNeighbor) },
                { ResizeWebProcessor.Width, Width.ToString() },
                { ResizeWebProcessor.Height, Height.ToString() },
                { ResizeWebProcessor.Xy, "0,0" }
            };

            using var image = new Image<Rgba32>(1, 1);
            using var formatted = new FormattedImage(image, PngFormat.Instance);
            new ResizeWebProcessor().Process(formatted, null, commands);

            Assert.Equal(Width, image.Width);
            Assert.Equal(Height, image.Height);
        }
    }
}
