// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

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
    }
}
