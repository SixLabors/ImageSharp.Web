// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Providers;

namespace SixLabors.ImageSharp.Web.Tests.Providers;

public class PhysicalFileSystemProviderTests
{
    [Theory]
#if OS_LINUX
    [InlineData(null, "wwwroot", "/Users/root/", "/Users/root/wwwroot/")]
    [InlineData(null, "/Users/WebRoot", "/Users/root/", "/Users/WebRoot/")]
    [InlineData("providerFolder", "../Temp", "/Users/this/a/root", "/Users/this/a/root/providerFolder/")]
    [InlineData("../Temp", null, "/Users/this/a/root", "/Users/this/a/Temp/")]
    [InlineData("/Users/WebRoot", null, "/Users/this/a/root", "/Users/WebRoot/")]
#elif OS_OSX
    [InlineData(null, "wwwroot", "/Users/root/", "/Users/root/wwwroot/")]
    [InlineData(null, "/Users/WebRoot", "/Users/root/", "/Users/WebRoot/")]
    [InlineData("providerFolder", "../Temp", "/Users/this/a/root", "/Users/this/a/root/providerFolder/")]
    [InlineData("../Temp", null, "/Users/this/a/root", "/Users/this/a/Temp/")]
    [InlineData("/Users/WebRoot", null, "/Users/this/a/root", "/Users/WebRoot/")]
#elif OS_WINDOWS
    [InlineData(null, "wwwroot", "C:\\root\\", "C:\\root\\wwwroot\\")]
    [InlineData(null, "C:\\WebRoot", "C:\\root\\", "C:\\WebRoot\\")]
    [InlineData("providerFolder", "..\\Temp", "C:\\this\\a\\root", "C:\\this\\a\\root\\providerFolder\\")]
    [InlineData("..\\Temp", null, "C:\\this\\a\\root", "C:\\this\\a\\Temp\\")]
    [InlineData("C:\\WebRoot", null, "C:\\this\\a\\root", "C:\\WebRoot\\")]
#endif
    public void GetProviderRoot(string providerRootPath, string webRootPath, string contentRootPath, string expected)
    {
        var providerOptions = new PhysicalFileSystemProviderOptions
        {
            ProviderRootPath = providerRootPath
        };

        string actual = PhysicalFileSystemProvider.GetProviderRoot(providerOptions, webRootPath, contentRootPath);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null, null, "C:\\root\\")]
    [InlineData("", null, "C:\\root\\")]
    [InlineData(null, "", "C:\\root\\")]
    public void GetProviderRootThrows(string providerRootPath, string webRootPath, string contentRootPath)
    {
        var providerOptions = new PhysicalFileSystemProviderOptions
        {
            ProviderRootPath = providerRootPath
        };

        Assert.Throws<InvalidOperationException>(() => PhysicalFileSystemProvider.GetProviderRoot(providerOptions, webRootPath, contentRootPath));
    }
}
