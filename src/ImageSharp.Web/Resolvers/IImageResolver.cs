// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SixLabors.Memory;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Specifies the contract for returning images from different locations.
    /// </summary>
    public interface IImageResolver
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
        /// <returns>The <see cref="Task{Boolean}"/></returns>
        /// </returns>
        Task<bool> IsValidRequestAsync(HttpContext context);

        /// <summary>
        /// Resolves the image in an asynchronous manner.
        /// </summary>
        /// <param name="context">The current HTTP request context.</param>
        /// <returns>The <see cref="T:Task{IByteBuffer}"/>.</returns>
        Task<IManagedByteBuffer> ResolveImageAsync(HttpContext context);
    }
}