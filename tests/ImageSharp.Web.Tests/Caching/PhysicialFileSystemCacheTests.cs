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

        [Fact]
        public void CacheRootFromOptions()
        {
            var cacheOptions = new PhysicalFileSystemCacheOptions();
            cacheOptions.CacheFolder = "cacheFolder";
            cacheOptions.CacheRoot = "C:\\Temp";

            var cacheRoot = PhysicalFileSystemCache.GetCacheRoot(cacheOptions, null);

            Assert.Equal(Path.Combine(cacheOptions.CacheRoot, cacheOptions.CacheFolder), cacheRoot);
        }

        [Fact]
        public void CacheRootFromEnvironment()
        {
            var cacheOptions = new PhysicalFileSystemCacheOptions();
            cacheOptions.CacheFolder = "cacheFolder";

            var cacheRoot = PhysicalFileSystemCache.GetCacheRoot(cacheOptions, "C:\\WebRoot");

            Assert.Equal(Path.Combine("C:\\WebRoot", cacheOptions.CacheFolder), cacheRoot);
        }
    }
}
