// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Tests.DependencyInjection;

namespace SixLabors.ImageSharp.Web.Tests.Processors;

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

        IReadOnlyList<(int Index, IImageWebProcessor Processor)> supportedProcessors = processors.OrderBySupportedCommands(commands);

        Assert.Equal(2, supportedProcessors.Count);
        Assert.IsType<ResizeWebProcessor>(supportedProcessors[0].Processor);
        Assert.IsType<QualityWebProcessor>(supportedProcessors[1].Processor);
    }

    [Fact]
    public void WebProcessingExtensions_GetBySupportedCommands_Empty()
    {
        var processors = new IImageWebProcessor[]
        {
            new MockWebProcessor()
        };

        CommandCollection commands = new();

        IReadOnlyList<(int Index, IImageWebProcessor Processor)> supportedProcessors = processors.OrderBySupportedCommands(commands);

        Assert.Empty(supportedProcessors);
    }
}
