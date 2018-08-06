// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.DependencyInjection
{
    /// <summary>
    /// Provides default configuration settings to be consumed by the middleware.
    /// </summary>
    public class ImageSharpConfiguration : IConfigureOptions<ImageSharpMiddlewareOptions>
    {
        /// <inheritdoc/>
        public void Configure(ImageSharpMiddlewareOptions options)
        {
            options.Configuration = Configuration.Default;
            options.MaxCacheDays = 365;
            options.MaxBrowserCacheDays = 7;
            options.CachedNameLength = 12;
        }
    }
}