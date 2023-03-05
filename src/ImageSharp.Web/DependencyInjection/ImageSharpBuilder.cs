// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.Extensions.DependencyInjection;

namespace SixLabors.ImageSharp.Web.DependencyInjection;

/// <summary>
/// Allows fine grained configuration of ImageSharp services.
/// </summary>
internal class ImageSharpBuilder : IImageSharpBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSharpBuilder"/> class.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    public ImageSharpBuilder(IServiceCollection services) => this.Services = services;

    /// <inheritdoc/>
    public IServiceCollection Services { get; }
}
