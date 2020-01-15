// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class MockImageProvider : IImageProvider
    {
        public bool ProcessWhenNoCommands { get; set; } = true;

        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        public Task<IImageResolver> GetAsync(HttpContext context) => Task.FromResult<IImageResolver>(null);

        public bool IsValidRequest(HttpContext context) => true;
    }
}
