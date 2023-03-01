// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Providers;

/// <summary>
/// Specifies the contract for returning images from different locations.
/// </summary>
public interface IImageProvider
{
    /// <summary>
    /// Gets the processing behavior.
    /// </summary>
    ProcessingBehavior ProcessingBehavior { get; }

    /// <summary>
    /// Gets or sets the method used by the resolver to identify itself as the correct resolver to use.
    /// </summary>
    Func<HttpContext, bool> Match { get; set; }

    /// <summary>
    /// Gets a value indicating whether the current request passes sanitizing rules.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <returns>
    /// <returns>The <see cref="bool"/></returns>
    /// </returns>
    bool IsValidRequest(HttpContext context);

    /// <summary>
    /// Gets the image resolver associated with the context.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <returns>The <see cref="IImageResolver"/>.</returns>
    Task<IImageResolver?> GetAsync(HttpContext context);
}
