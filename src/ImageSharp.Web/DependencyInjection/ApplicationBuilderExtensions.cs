// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Builder;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IApplicationBuilder"/> to simplify middleware registration.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Registers the ImageSharp middleware.
    /// </summary>
    /// <param name="app">The application with the mechanism to configure a request pipeline.</param>
    /// <returns><see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseImageSharp(this IApplicationBuilder app)
    {
        Guard.NotNull(app, nameof(app));

        return app.UseMiddleware<ImageSharpMiddleware>();
    }
}
