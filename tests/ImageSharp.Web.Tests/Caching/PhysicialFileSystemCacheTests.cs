// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Tests.Caching;

public class PhysicialFileSystemCacheTests
{
    [Theory]
    [InlineData("abcdefghijkl", 0, "abcdefghijkl")]
    [InlineData("abcdefghijkl", 4, "a/b/c/d/efghijkl")]
    [InlineData("abcdefghijkl", 8, "a/b/c/d/e/f/g/h/ijkl")]
    [InlineData("abcdefghijkl", 12, "a/b/c/d/e/f/g/h/i/j/k/l/abcdefghijkl")]
    [InlineData("abcdefghijkl", 16, "a/b/c/d/e/f/g/h/i/j/k/l/abcdefghijkl")]
    public void FilePathMatchesReference(string key, int cacheFolderDepth, string expected)
    {
        string actual = PhysicalFileSystemCache.ToFilePath(key, cacheFolderDepth);

        Assert.Equal(expected, actual);
    }

    [Theory]
#if OS_LINUX
    [InlineData("cacheFolder", "/Users/username", null, null, "/Users/username/cacheFolder/")]
    [InlineData("cacheFolder", null, "/Users/WebRoot", null, "/Users/WebRoot/cacheFolder/")]
    [InlineData("cacheFolder", "../Temp", null, "/Users/this/a/root", "/Users/this/a/Temp/cacheFolder/")]
#elif OS_OSX
    [InlineData("cacheFolder", "/Users/username", null, null, "/Users/username/cacheFolder/")]
    [InlineData("cacheFolder", null, "/Users/WebRoot", null, "/Users/WebRoot/cacheFolder/")]
    [InlineData("cacheFolder", "../Temp", null, "/Users/this/a/root", "/Users/this/a/Temp/cacheFolder/")]
#elif OS_WINDOWS
    [InlineData("cacheFolder", "C:\\Temp", null, null, "C:\\Temp\\cacheFolder\\")]
    [InlineData("cacheFolder", null, "C:\\WebRoot", null, "C:\\WebRoot\\cacheFolder\\")]
    [InlineData("cacheFolder", "..\\Temp", null, "C:\\this\\a\\root", "C:\\this\\a\\Temp\\cacheFolder\\")]
#endif
    public void GetCacheRoot(string cacheFolder, string cacheRootPath, string webRootPath, string contentRootPath, string expected)
    {
        var cacheOptions = new PhysicalFileSystemCacheOptions
        {
            CacheFolder = cacheFolder,
            CacheRootPath = cacheRootPath
        };

        string actual = PhysicalFileSystemCache.GetCacheRoot(cacheOptions, webRootPath, contentRootPath);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("cacheFolder", null, null, "C:\\root\\")]
    [InlineData("cacheFolder", "", null, "C:\\root\\")]
    [InlineData("cacheFolder", null, "", "C:\\root\\")]
    public void GetCacheRootThrows(string cacheFolder, string cacheRootPath, string webRootPath, string contentRootPath)
    {
        var cacheOptions = new PhysicalFileSystemCacheOptions
        {
            CacheFolder = cacheFolder,
            CacheRootPath = cacheRootPath
        };

        Assert.Throws<InvalidOperationException>(() => PhysicalFileSystemCache.GetCacheRoot(cacheOptions, webRootPath, contentRootPath));
    }
}
