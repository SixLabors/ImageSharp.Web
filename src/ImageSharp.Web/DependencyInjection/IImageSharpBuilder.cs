// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.Extensions.DependencyInjection;

namespace SixLabors.ImageSharp.Web.DependencyInjection;

/// <summary>
/// Defines a contract for adding ImageSharp services.
/// </summary>
public interface IImageSharpBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> where ImageSharp services are configured.
    /// </summary>
    IServiceCollection Services { get; }
}
