// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Specifies the contract for returning images from different locations.
    /// </summary>
    public interface IImageProvider
    {
        /// <summary>
        /// Gets or sets the method used by the resolver to identify itself as the correct resolver to use.
        /// </summary>
        Func<HttpContext, bool> Match { get; set; }

        /// <summary>
        /// Gets or sets any additional settings.
        /// </summary>
        IDictionary<string, string> Settings { get; set; }

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
        Task<IImageResolver> GetAsync(HttpContext context);
    }
}