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
        /// Gets or sets the depth of the nested cache folders structure to store the images.
        /// </summary>
        public uint CacheFolderDepth { get; set; } = 12;

        /// <summary>
        /// Gets or sets the optional cache root folder.
        /// <para>
        /// This value can be <see langword="null"/>, a fully qualified absolute path,
        /// or a path relative to the directory that contains the application
        /// content files.
        /// </para>
        /// <para>
        /// If not set, this will default to the directory that contains the web-servable
        /// application content files; commonly 'wwwroot'.
        /// </para>
        /// </summary>
        public string CacheRoot { get; set; }
    }
}
