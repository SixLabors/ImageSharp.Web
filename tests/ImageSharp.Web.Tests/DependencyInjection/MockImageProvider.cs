// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection;

public class MockImageProvider : IImageProvider
{
    public ProcessingBehavior ProcessingBehavior { get; set; } = ProcessingBehavior.All;

    public Func<HttpContext, bool> Match { get; set; } = _ => true;

    public Task<IImageResolver> GetAsync(HttpContext context) => Task.FromResult<IImageResolver>(null);

    public bool IsValidRequest(HttpContext context) => true;
}
