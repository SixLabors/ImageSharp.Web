// Copyright (c) Six Labors and contributors.
// Licensed under the GNU Affero General Public License, Version 3.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class MockImageProvider : IImageProvider
    {
        public ProcessingBehavior ProcessingBehavior { get; set; } = ProcessingBehavior.All;

        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        public Task<IImageResolver> GetAsync(HttpContext context) => Task.FromResult<IImageResolver>(null);

        public bool IsValidRequest(HttpContext context) => true;
    }
}
