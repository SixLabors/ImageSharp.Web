// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.Memory;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Implements a physical file system based cache.
    /// </summary>
    public class PhysicalFileSystemCache : IImageCache
    {
        /// <summary>
        /// The configuration key for determining the cache folder.
        /// </summary>
        public const string Folder = "CacheFolder";

        /// <summary>
        /// The default cache folder name.
        /// </summary>
        public const string DefaultCacheFolder = "is-cache";

        /// <summary>
        /// The default value for determining whether to check for changes in the source.
        /// </summary>
        public const string DefaultCheckSourceChanged = "false";

        /// <summary>
        /// The hosting environment the application is running in.
        /// </summary>
        private readonly IHostingEnvironment environment;

        /// <summary>
        /// The file provider abstraction.
        /// </summary>
        private readonly IFileProvider fileProvider;

        /// <summary>
        /// The buffer manager.
        /// </summary>
        private readonly MemoryAllocator memoryAllocator;

        /// <summary>
        /// The middleware configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemCache"/> class.
        /// </summary>
        /// <param name="environment">The hosting environment the application is running in.</param>
        /// <param name="memoryAllocator">An <see cref="MemoryAllocator"/> instance used to allocate arrays transporting encoded image data.</param>
        /// <param name="options">The middleware configuration options.</param>
        public PhysicalFileSystemCache(IHostingEnvironment environment, MemoryAllocator memoryAllocator, IOptions<ImageSharpMiddlewareOptions> options)
        {
            Guard.NotNull(environment, nameof(environment));
            Guard.NotNull(memoryAllocator, nameof(memoryAllocator));
            Guard.NotNull(options, nameof(options));

            Guard.NotNullOrWhiteSpace(
              environment.WebRootPath,
              nameof(environment.WebRootPath),
              "The folder 'wwwroot' that contains the web-servable application content files is missing. Please add this folder to the application root to allow caching.");

            this.environment = environment;
            this.fileProvider = this.environment.WebRootFileProvider;
            this.memoryAllocator = memoryAllocator;
            this.options = options.Value;
        }

        /// <inheritdoc/>
        public IDictionary<string, string> Settings { get; }
            = new Dictionary<string, string>
            {
                { Folder, DefaultCacheFolder }
            };

        /// <inheritdoc/>
        public IImageResolver Get(string key)
        {
            IFileInfo fileInfo = this.fileProvider.GetFileInfo(this.ToFilePath(key));

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return null;
            }

            return new PhysicalFileSystemResolver(fileInfo);
        }

        /// <inheritdoc/>
        public Task<CachedInfo> IsExpiredAsync(HttpContext context, string key, DateTime lastWriteTimeUtc, DateTime minDateUtc)
        {
            IFileInfo cachedFileInfo = this.fileProvider.GetFileInfo(this.ToFilePath(key));
            if (!cachedFileInfo.Exists)
            {
                return Task.FromResult(new CachedInfo(true, DateTime.MinValue));
            }

            DateTime lastCacheModifiedUtc = cachedFileInfo.LastModified.UtcDateTime;
            bool expired = true;

            // Check whether the last modified date is less than the min date.
            // If it's newer than the cached file then it must be an update.
            if (lastCacheModifiedUtc > minDateUtc && lastWriteTimeUtc < lastCacheModifiedUtc)
            {
                expired = false;
            }

            return Task.FromResult(new CachedInfo(expired, lastCacheModifiedUtc));
        }

        /// <inheritdoc/>
        public async Task<DateTimeOffset> SetAsync(string key, Stream stream)
        {
            string path = Path.Combine(this.environment.WebRootPath, this.ToFilePath(key));
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fileStream = File.Create(path))
            {
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            return File.GetLastWriteTimeUtc(path);
        }

        /// <summary>
        /// Converts the key into a nested file path.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ToFilePath(string key) => $"{this.Settings[Folder]}/{string.Join("/", key.Substring(0, (int)this.options.CachedNameLength).ToCharArray())}/{key}";
    }
}