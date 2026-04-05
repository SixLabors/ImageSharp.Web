// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace SixLabors.ImageSharp.Web.Tests.Caching;

public class CacheHashTests
{
    private static readonly IOptions<ImageSharpMiddlewareOptions> Options = MSOptions.Create(new ImageSharpMiddlewareOptions());
    private static readonly SHA256CacheHash CacheHash = new(Options);

    [Fact]
    public void CacheHashProducesIdenticalResults()
    {
        const string input = "http://testwebsite.com/image-12345.jpeg?width=400";
        string expected = CacheHash.Create(input, 8);
        string actual = CacheHash.Create(input, 8);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CacheHashProducesDifferentResults()
    {
        const string input = "http://testwebsite.com/image-12345.jpeg?width=400";
        const string input2 = "http://testwebsite.com/image-23456.jpeg?width=400";
        string expected = CacheHash.Create(input, 8);
        string actual = CacheHash.Create(input2, 8);

        Assert.NotEqual(expected, actual);
    }

    [Fact]
    public void CacheHashLengthIsIdentical()
    {
        const int length = 12;
        const string input = "http://testwebsite.com/image-12345.jpeg?width=400";
        const string input2 = "http://testwebsite.com/image-12345.jpeg";
        string input3 = CreateLongString();

        int expected = CacheHash.Create(input, length).Length;
        int actual = CacheHash.Create(input2, length).Length;
        int actual2 = CacheHash.Create(input3, length).Length;

        Assert.Equal(expected, actual);
        Assert.Equal(length, actual);
        Assert.Equal(length, actual2);
    }

    private static string CreateLongString()
    {
        const int length = 2048;
        StringBuilder sb = new(length);

        for (int i = 0; i < length; i++)
        {
            sb.Append(i.ToString(CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }
}
