// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Linq;
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
                new JpegQualityWebProcessor(),
                new ResizeWebProcessor(),
                new BackgroundColorWebProcessor(),
                new MockWebProcessor()
            };

            var commands = new List<string>
            {
                ResizeWebProcessor.Width,
                JpegQualityWebProcessor.Quality,
                ResizeWebProcessor.Height
            };

            IImageWebProcessor[] supportedProcessors = WebProcessingExtensions.GetBySupportedCommands(processors, commands).ToArray();

            Assert.Equal(2, supportedProcessors.Length);
            Assert.IsType<ResizeWebProcessor>(supportedProcessors[0]);
            Assert.IsType<JpegQualityWebProcessor>(supportedProcessors[1]);
        }

        [Fact]
        public void WebProcessingExtensions_GetBySupportedCommands_Empty()
        {
            var processors = new IImageWebProcessor[]
            {
                new MockWebProcessor()
            };

            var commands = new List<string>();

            IImageWebProcessor[] supportedProcessors = WebProcessingExtensions.GetBySupportedCommands(processors, commands).ToArray();

            Assert.Empty(supportedProcessors);
        }
    }
}
