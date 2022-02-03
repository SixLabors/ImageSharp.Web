// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Web.Providers;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Providers
{
    public class PhysicalFileSystemProviderTests
    {
        [Theory]
#if OS_LINUX
        [InlineData(null, "wwwroot", "/Users/root/", "/Users/root/wwwroot")]
        [InlineData(null, "/Users/WebRoot", "/Users/root/", "/Users/WebRoot")]
        [InlineData("providerFolder", "../Temp", "/Users/this/a/root", "/Users/this/a/root/providerFolder")]
        [InlineData("../Temp", null, "/Users/this/a/root", "/Users/this/a/Temp")]
        [InlineData("/Users/WebRoot", null, "/Users/this/a/root", "/Users/WebRoot")]
#elif OS_OSX
        [InlineData(null, "wwwroot", "/Users/root/", "/Users/root/wwwroot")]
        [InlineData(null, "/Users/WebRoot", "/Users/root/", "/Users/WebRoot")]
        [InlineData("providerFolder", "../Temp", "/Users/this/a/root", "/Users/this/a/root/providerFolder")]
        [InlineData("../Temp", null, "/Users/this/a/root", "/Users/this/a/Temp")]
        [InlineData("/Users/WebRoot", null, "/Users/this/a/root", "/Users/WebRoot")]
#elif OS_WINDOWS
        [InlineData(null, "wwwroot", "C:/root\\", "C:\\root\\wwwroot")]
        [InlineData(null, "C:/WebRoot", "C:/root\\", "C:/WebRoot")]
        [InlineData("providerFolder", "../Temp", "C:/this/a/root", "C:\\this\\a\\root\\providerFolder")]
        [InlineData("../Temp", null, "C:/this/a/root", "C:\\this\\a\\Temp")]
        [InlineData("C:/WebRoot", null, "C:/this/a/root", "C:/WebRoot")]
#endif
        public void ProvidersRootFromOptions(string providerRoot, string webRootPath, string contentRootPath, string expected)
        {
            var providerOptions = new PhysicalFileSystemProviderOptions
            {
                ProviderRootPath = providerRoot,
            };

            string providerRootResult = PhysicalFileSystemProvider.GetProviderRoot(providerOptions, webRootPath, contentRootPath);

            Assert.Equal(expected, providerRootResult);
        }
    }
}
