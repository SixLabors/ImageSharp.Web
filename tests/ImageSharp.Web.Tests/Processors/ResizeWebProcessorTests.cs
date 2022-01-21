// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Processors;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Processors
{
    public class ResizeWebProcessorTests
    {
        [Theory]
        [InlineData(nameof(KnownResamplers.Bicubic))]
        [InlineData(nameof(KnownResamplers.Box))]
        [InlineData(nameof(KnownResamplers.CatmullRom))]
        [InlineData(nameof(KnownResamplers.Hermite))]
        [InlineData(nameof(KnownResamplers.Lanczos2))]
        [InlineData(nameof(KnownResamplers.Lanczos3))]
        [InlineData(nameof(KnownResamplers.Lanczos5))]
        [InlineData(nameof(KnownResamplers.Lanczos8))]
        [InlineData(nameof(KnownResamplers.MitchellNetravali))]
        [InlineData(nameof(KnownResamplers.NearestNeighbor))]
        [InlineData(nameof(KnownResamplers.Robidoux))]
        [InlineData(nameof(KnownResamplers.RobidouxSharp))]
        [InlineData(nameof(KnownResamplers.Spline))]
        [InlineData(nameof(KnownResamplers.Triangle))]
        [InlineData(nameof(KnownResamplers.Welch))]
        public void ResizeWebProcessor_UpdatesSize(string resampler)
        {
            const int width = 4;
            const int height = 6;

            var converters = new List<ICommandConverter>
            {
                new IntegralNumberConverter<uint>(),
                new ArrayConverter<float>(),
                new EnumConverter(),
                new SimpleCommandConverter<bool>(),
                new SimpleCommandConverter<float>()
            };

            var parser = new CommandParser(converters);
            CultureInfo culture = CultureInfo.InvariantCulture;

            CommandCollection commands = new()
            {
                { new(ResizeWebProcessor.Sampler, resampler) },
                { new(ResizeWebProcessor.Width, width.ToString()) },
                { new(ResizeWebProcessor.Height, height.ToString()) },
                { new(ResizeWebProcessor.Xy, "0,0") },
                { new(ResizeWebProcessor.Mode, nameof(ResizeMode.Stretch)) }
            };

            using var image = new Image<Rgba32>(1, 1);
            using var formatted = new FormattedImage(image, PngFormat.Instance);
            new ResizeWebProcessor().Process(formatted, null, commands, parser, culture);

            Assert.Equal(width, image.Width);
            Assert.Equal(height, image.Height);
        }
    }
}
