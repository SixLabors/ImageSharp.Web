// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.DependencyInjection;

namespace SixLabors.ImageSharp.Web.DependencyInjection
{
    /// <summary>
    /// Defines a contract for adding ImsgeSharp services.
    /// </summary>
    public interface IImageSharpBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> where ImageSharp services are configured.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
