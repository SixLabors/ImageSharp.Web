// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web;

internal static class PathUtilities
{
    /// <summary>
    /// Ensures the path ends with a trailing slash (directory separator).
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    /// The path with a trailing slash.
    /// </returns>
    internal static string EnsureTrailingSlash(string path)
    {
        if (!string.IsNullOrEmpty(path) &&
            path[path.Length - 1] != Path.DirectorySeparatorChar)
        {
            return path + Path.DirectorySeparatorChar;
        }

        return path;
    }
}
