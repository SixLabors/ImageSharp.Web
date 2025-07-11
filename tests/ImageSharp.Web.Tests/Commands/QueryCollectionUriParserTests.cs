// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Tests.Commands;

public class QueryCollectionUriParserTests
{
    [Fact]
    public void QueryCollectionParserExtractsCommands()
    {
        CommandCollection expected = new()
        {
            { new KeyValuePair<string, string>("width", "400") },
            { new KeyValuePair<string, string>("height", "200") }
        };

        HttpContext context = CreateHttpContext();
        CommandCollection actual = new QueryCollectionRequestParser().ParseRequestCommands(context);

        Assert.Equal(expected, actual);
    }

    private static HttpContext CreateHttpContext()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = "/testwebsite.com/image-12345.jpeg";

        // Duplicate height param to test replacements
        httpContext.Request.QueryString = new QueryString("?width=400&height=100&height=200");
        return httpContext;
    }
}
