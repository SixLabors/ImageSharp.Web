// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;

namespace SixLabors.ImageSharp.Web
{
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

        /// <summary>
        /// Determines whether the <paramref name="path" /> is located underneath the specified <paramref name="rootPath" />.
        /// </summary>
        /// <param name="path">The fully qualified path to test.</param>
        /// <param name="rootPath">The root path (needs to end with a directory separator).</param>
        /// <returns>
        ///   <c>true</c> if the path is located underneath the specified root path; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsUnderneathRoot(string path, string rootPath)
            => path.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase);
    }
}
