// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Linq;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Tests.DependencyInjection;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Processors
{
    public class WebProcessingExtensionsTests
    {
        [Fact]
        public void WebProcessingExtensions_GetBySupportedCommands()
        {
            var processors = new IImageWebProcessor[]
            {
                new QualityWebProcessor(),
                new ResizeWebProcessor(),
                new BackgroundColorWebProcessor(),
                new MockWebProcessor()
            };

            CommandCollection commands = new()
            {
                new(ResizeWebProcessor.Width, null),
                new(QualityWebProcessor.Quality, null),
                new(ResizeWebProcessor.Height, null)
            };

            IImageWebProcessor[] supportedProcessors = processors.GetBySupportedCommands(commands).ToArray();

            Assert.Equal(2, supportedProcessors.Length);
            Assert.IsType<ResizeWebProcessor>(supportedProcessors[0]);
            Assert.IsType<QualityWebProcessor>(supportedProcessors[1]);
        }

        [Fact]
        public void WebProcessingExtensions_GetBySupportedCommands_Empty()
        {
            var processors = new IImageWebProcessor[]
            {
                new MockWebProcessor()
            };

            CommandCollection commands = new();

            IImageWebProcessor[] supportedProcessors = processors.GetBySupportedCommands(commands).ToArray();

            Assert.Empty(supportedProcessors);
        }
    }
}
