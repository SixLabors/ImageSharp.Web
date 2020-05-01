// Copyright (c) Six Labors and contributors.
// Licensed under the GNU Affero General Public License, Version 3.

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Configuration options for the <see cref="PhysicalFileSystemCache"/>.
    /// </summary>
    public class PhysicalFileSystemCacheOptions
    {
        /// <summary>
        /// Gets or sets the cache folder name.
        /// </summary>
        public string CacheFolder { get; set; } = "is-cache";
    }
}
