// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Caching;

/// <summary>
/// Defines a contract that allows the creation of hashed file names for storing cached images.
/// </summary>
public interface ICacheHash
{
    /// <summary>
    /// Returns the hashed file name for the cached image file.
    /// </summary>
    /// <param name="value">The input value to hash.</param>
    /// <param name="length">The length of the returned hash without any extensions.</param>
    /// <returns>The <see cref="string"/>.</returns>
    string Create(string value, uint length);
}
