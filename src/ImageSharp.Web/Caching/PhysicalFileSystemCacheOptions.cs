// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

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

        /// <summary>
        /// Gets or sets the cache root folder.
        /// </summary>
        public string CacheRoot { get; set; }
    }
}
