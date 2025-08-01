// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Tests.Helpers;

public class FormatUtilitiesTests
{
    public static IEnumerable<object[]> DefaultExtensions =
        Configuration.Default.ImageFormats.SelectMany(f => f.FileExtensions.Select(e => new object[] { e, e }));

    private static readonly FormatUtilities FormatUtilities = new(Options.Create(new ImageSharpMiddlewareOptions()));

    [Theory]
    [MemberData(nameof(DefaultExtensions))]
    public void GetExtensionShouldMatchDefaultExtensions(string expected, string ext)
    {
        string uri = $"http://www.example.org/some/path/to/image.{ext}?width=300";
        FormatUtilities.TryGetExtensionFromUri(uri, out string actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetExtensionShouldNotMatchExtensionWithoutDotPrefix()
    {
        const string uri = "http://www.example.org/some/path/to/bmpimage";
        Assert.False(FormatUtilities.TryGetExtensionFromUri(uri, out _));
    }

    [Fact]
    public void GetExtensionShouldIgnoreQueryStringWithoutFormatParamter()
    {
        const string uri = "http://www.example.org/some/path/to/image.bmp?width=300&foo=.png";
        FormatUtilities.TryGetExtensionFromUri(uri, out string actual);
        Assert.Equal("bmp", actual);
    }

    [Fact]
    public void GetExtensionShouldAcknowledgeQueryStringFormatParameter()
    {
        const string uri = "http://www.example.org/some/path/to/image.bmp?width=300&format=png";
        FormatUtilities.TryGetExtensionFromUri(uri, out string actual);
        Assert.Equal("png", actual);
    }

    [Fact]
    public void GetExtensionShouldAllowInvalidQueryStringFormatParameterWithValidExtension()
    {
        const string uri = "http://www.example.org/some/path/to/image.bmp?width=300&format=invalid";
        Assert.True(FormatUtilities.TryGetExtensionFromUri(uri, out _));
    }

    [Fact]
    public void GetExtensionShouldRejectInvalidPathWithValidQueryStringFormatParameter()
    {
        const string uri = "http://www.example.org/some/path/to/image.svg?width=300&format=jpg";
        Assert.False(FormatUtilities.TryGetExtensionFromUri(uri, out _));
    }
}
