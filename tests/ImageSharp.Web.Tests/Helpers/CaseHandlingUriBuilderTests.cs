// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Tests.Helpers;

public class CaseHandlingUriBuilderTests
{
    [Theory]
    [InlineData(CaseHandlingUriBuilder.CaseHandling.None, "https://sixlabors.com:443/Path/?query=1")]
    [InlineData(CaseHandlingUriBuilder.CaseHandling.LowerInvariant, "https://sixlabors.com:443/path/?query=1")]
    public void CanEncodeAbsoluteUri(CaseHandlingUriBuilder.CaseHandling value, string expected)
    {
        const string input = "https://sixlabors.com/Path/?Query=1";
        string actual = CaseHandlingUriBuilder.Encode(value, input);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(CaseHandlingUriBuilder.CaseHandling.None, "/Path/?query=1")]
    [InlineData(CaseHandlingUriBuilder.CaseHandling.LowerInvariant, "/path/?query=1")]
    public void CanEncodeRelativeUri(CaseHandlingUriBuilder.CaseHandling value, string expected)
    {
        const string input = "/Path/?Query=1";
        string actual = CaseHandlingUriBuilder.Encode(value, input);
        Assert.Equal(expected, actual);
    }
}
