// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class MockImageCache : IImageCache
    {
        public Task<IImageCacheResolver> GetAsync(string key) => Task.FromResult<IImageCacheResolver>(null);

        public Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata) => Task.CompletedTask;
    }
}
