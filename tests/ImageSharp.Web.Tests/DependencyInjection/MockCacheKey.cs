// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class MockCacheKey : ICacheKey
    {
        public string Create(HttpContext context, CommandCollection commands) => null;
    }
}
