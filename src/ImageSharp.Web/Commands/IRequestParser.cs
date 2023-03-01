// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;

namespace SixLabors.ImageSharp.Web.Commands;

/// <summary>
/// Defines a contract for parsing commands from image requests.
/// </summary>
public interface IRequestParser
{
    /// <summary>
    /// Returns a collection of commands from the current request.
    /// </summary>
    /// <param name="context">Encapsulates all HTTP-specific information about an individual HTTP request.</param>
    /// <returns>The <see cref="IDictionary{TKey,TValue}"/>.</returns>
    CommandCollection ParseRequestCommands(HttpContext context);
}
