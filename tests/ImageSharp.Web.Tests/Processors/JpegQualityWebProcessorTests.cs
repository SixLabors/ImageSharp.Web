// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Processors;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Processors
{
    public class JpegQualityWebProcessorTests
    {
        [Fact]
        public void JpegQualityWebProcessor_UpdatesQuality()
        {
            var parser = new CommandParser(new[] { new IntegralNumberConverter<int>() });
            CultureInfo culture = CultureInfo.InvariantCulture;

            var commands = new Dictionary<string, string>
            {
                { JpegQualityWebProcessor.Quality, "42" },
            };

            using var image = new Image<Rgba32>(1, 1);
            using var formatted = new FormattedImage(image, JpegFormat.Instance);
            Assert.Equal(JpegFormat.Instance, formatted.Format);
            Assert.Equal(typeof(JpegEncoder), formatted.Encoder.GetType());

            new JpegQualityWebProcessor()
                .Process(formatted, null, commands, parser, culture);

            Assert.Equal(JpegFormat.Instance, formatted.Format);
            Assert.Equal(42, ((JpegEncoder)formatted.Encoder).Quality);
        }
    }
}
