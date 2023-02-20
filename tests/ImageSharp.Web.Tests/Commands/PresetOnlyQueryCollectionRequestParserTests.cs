// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Tests.Commands;

public class PresetOnlyQueryCollectionRequestParserTests
{
    [Fact]
    public void PresetOnlyQueryCollectionRequestParserExtractsCommands()
    {
        IDictionary<string, string> expected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "width", "400" },
            { "height", "200" }
        };

        HttpContext context = CreateHttpContext("?preset=Preset1");
        CommandCollection actual = new PresetOnlyQueryCollectionRequestParser(Options.Create(new PresetOnlyQueryCollectionRequestParserOptions
        {
            Presets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Preset1", "width=400&height=200" }
            }
        })).ParseRequestCommands(context);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void PresetOnlyQueryCollectionRequestParserExtractsCommandsWithOtherCasing()
    {
        IDictionary<string, string> expected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "width", "400" },
            { "height", "200" }
        };

        HttpContext context = CreateHttpContext("?PRESET=PRESET1");
        CommandCollection actual = new PresetOnlyQueryCollectionRequestParser(Options.Create(new PresetOnlyQueryCollectionRequestParserOptions
        {
            Presets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Preset1", "width=400&height=200" }
            }
        })).ParseRequestCommands(context);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void PresetOnlyQueryCollectionRequestParserCommandsWithoutPresetParam()
    {
        IDictionary<string, string> expected = new Dictionary<string, string>();

        HttpContext context = CreateHttpContext("?test=test");
        CommandCollection actual = new PresetOnlyQueryCollectionRequestParser(Options.Create(new PresetOnlyQueryCollectionRequestParserOptions
        {
            Presets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Preset1", "width=400&height=200" }
            }
        })).ParseRequestCommands(context);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void PresetOnlyQueryCollectionRequestParserCommandsWithoutMatchingPreset()
    {
        IDictionary<string, string> expected = new Dictionary<string, string>();

        HttpContext context = CreateHttpContext("?preset=Preset2");
        CommandCollection actual = new PresetOnlyQueryCollectionRequestParser(Options.Create(new PresetOnlyQueryCollectionRequestParserOptions
        {
            Presets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Preset1", "width=400&height=200" }
            }
        })).ParseRequestCommands(context);

        Assert.Equal(expected, actual);
    }

    private static HttpContext CreateHttpContext(string query)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/testwebsite.com/image-12345.jpeg";
        httpContext.Request.QueryString = new QueryString(query);
        return httpContext;
    }
}
