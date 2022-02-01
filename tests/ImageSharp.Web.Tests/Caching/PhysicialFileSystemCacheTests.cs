// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Web.Caching;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class PhysicialFileSystemCacheTests
    {
        [Theory]
        [InlineData("abcdefghijkl", 0, false, "abcdefghijkl")]
        [InlineData("abcdefghijkl", 0, true, "abcdefghijkl")]
        [InlineData("abcdefghijkl", 4, false, "a/b/c/d/efghijkl")]
        [InlineData("abcdefghijkl", 4, true, "a/b/c/d/abcdefghijkl")]
        [InlineData("abcdefghijkl", 8, false, "a/b/c/d/e/f/g/h/ijkl")]
        [InlineData("abcdefghijkl", 8, true, "a/b/c/d/e/f/g/h/abcdefghijkl")]
        [InlineData("abcdefghijkl", 12, false, "a/b/c/d/e/f/g/h/i/j/k/l")]
        [InlineData("abcdefghijkl", 12, true, "a/b/c/d/e/f/g/h/i/j/k/l/abcdefghijkl")]
        [InlineData("abcdefghijkl", 16, false, "a/b/c/d/e/f/g/h/i/j/k/l")]
        [InlineData("abcdefghijkl", 16, true, "a/b/c/d/e/f/g/h/i/j/k/l/abcdefghijkl")]
        public void FilePathMatchesReference(string key, int cacheFolderDepth, bool legacyName, string expected)
        {
            string actual = PhysicalFileSystemCache.ToFilePath(key, cacheFolderDepth, legacyName);

            Assert.Equal(expected, actual);
        }

        [Theory]
#if OS_LINUX
        [InlineData("cacheFolder", "/Users/username", null, null, "/Users/username/cacheFolder")]
        [InlineData("cacheFolder", null, "/Users/WebRoot", null, "/Users/WebRoot/cacheFolder")]
        [InlineData("cacheFolder", "../Temp", null, "/Users/this/a/root", "/Users/this/a/Temp/cacheFolder")]
#elif OS_OSX
        [InlineData("cacheFolder", "/Users/username", null, null, "/Users/username/cacheFolder")]
        [InlineData("cacheFolder", null, "/Users/WebRoot", null, "/Users/WebRoot/cacheFolder")]
        [InlineData("cacheFolder", "../Temp", null, "/Users/this/a/root", "/Users/this/a/Temp/cacheFolder")]
#elif OS_WINDOWS
        [InlineData("cacheFolder", "C:/Temp", null, null, "C:/Temp\\cacheFolder")]
        [InlineData("cacheFolder", null, "C:/WebRoot", null, "C:/WebRoot\\cacheFolder")]
        [InlineData("cacheFolder", "../Temp", null, "C:/this/a/root", "C:\\this\\a\\Temp\\cacheFolder")]
#endif
        public void CacheRootFromOptions(string cacheFolder, string cacheRoot, string webRootPath, string contentRootPath, string expected)
        {
            var cacheOptions = new PhysicalFileSystemCacheOptions
            {
                CacheFolder = cacheFolder,
                CacheRoot = cacheRoot
            };

            var cacheRootResult = PhysicalFileSystemCache.GetCacheRoot(cacheOptions, webRootPath, contentRootPath);

            Assert.Equal(expected, cacheRootResult);
        }
    }
}
