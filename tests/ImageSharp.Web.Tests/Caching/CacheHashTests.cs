// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Text;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace SixLabors.ImageSharp.Web.Tests.Caching;

public class CacheHashTests
{
    private static readonly IOptions<ImageSharpMiddlewareOptions> Options = MSOptions.Create(new ImageSharpMiddlewareOptions());
    private static readonly ICacheHash CacheHash = new SHA256CacheHash(Options);

    [Fact]
    public void CacheHashProducesIdenticalResults()
    {
        const string Input = "http://testwebsite.com/image-12345.jpeg?width=400";
        string expected = CacheHash.Create(Input, 8);
        string actual = CacheHash.Create(Input, 8);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CacheHashProducesDifferentResults()
    {
        const string Input = "http://testwebsite.com/image-12345.jpeg?width=400";
        const string Input2 = "http://testwebsite.com/image-23456.jpeg?width=400";
        string expected = CacheHash.Create(Input, 8);
        string actual = CacheHash.Create(Input2, 8);

        Assert.NotEqual(expected, actual);
    }

    [Fact]
    public void CacheHashLengthIsIdentical()
    {
        const int Length = 12;
        const string Input = "http://testwebsite.com/image-12345.jpeg?width=400";
        const string Input2 = "http://testwebsite.com/image-12345.jpeg";
        string input3 = CreateLongString();

        int expected = CacheHash.Create(Input, Length).Length;
        int actual = CacheHash.Create(Input2, Length).Length;
        int actual2 = CacheHash.Create(input3, Length).Length;

        Assert.Equal(expected, actual);
        Assert.Equal(Length, actual);
        Assert.Equal(Length, actual2);
    }

    private static string CreateLongString()
    {
        const int Length = 2048;
        var sb = new StringBuilder(Length);

        for (int i = 0; i < Length; i++)
        {
            sb.Append(i.ToString());
        }

        return sb.ToString();
    }
}
