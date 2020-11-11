// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using SixLabors.ImageSharp.Web.Caching;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class PhysicialFileSystemCacheTests
    {
        [Fact]
        public void FilePathMatchesReference()
        {
            const string Key = "abcdefghijkl";
            const int CachedNameLength = 12;

            string expected = $"{string.Join("/", Key.Substring(0, CachedNameLength).ToCharArray())}/{Key}";
            string actual = PhysicalFileSystemCache.ToFilePath(Key, CachedNameLength);

            Assert.Equal(expected, actual);
        }

        [Theory]
#if Linux
        [InlineData("cacheFolder", "/Users/username", null, null, "/Users/username/cacheFolder")]
        [InlineData("cacheFolder", null, "/Users/WebRoot", null, "/Users/WebRoot/cacheFolder")]
        [InlineData("cacheFolder", "../Temp", null, "/Users/this/a/root", "/Users/this/a/Temp/cacheFolder")]
#elif OSX
        [InlineData("cacheFolder", "/Users/username", null, null, "/Users/username/cacheFolder")]
        [InlineData("cacheFolder", null, "/Users/WebRoot", null, "/Users/WebRoot/cacheFolder")]
        [InlineData("cacheFolder", "../Temp", null, "/Users/this/a/root", "/Users/this/a/Temp/cacheFolder")]
#elif Windows
        [InlineData("cacheFolder", "C:/Temp", null, null, "C:\\Temp\\cacheFolder")]
        [InlineData("cacheFolder", null, "C:/WebRoot", null, "C:\\WebRoot\\cacheFolder")]
        [InlineData("cacheFolder", "../Temp", null, "C:/this/a/root", "C:\\this\\a\\Temp\\cacheFolder")]
#endif
        public void CacheRootFromOptions(string cacheFolder, string cacheRoot, string webRootPath, string contentRootPath, string expected)
        {
            var cacheOptions = new PhysicalFileSystemCacheOptions();
            cacheOptions.CacheFolder = cacheFolder;
            cacheOptions.CacheRoot = cacheRoot;

            var cacheRootResult = PhysicalFileSystemCache.GetCacheRoot(cacheOptions, webRootPath, contentRootPath);

            Assert.Equal(expected, cacheRootResult);
        }
    }
}
