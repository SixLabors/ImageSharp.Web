// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class MockCacheHash : ICacheHash
    {
        public string Create(string value, uint length) => null;
    }
}
