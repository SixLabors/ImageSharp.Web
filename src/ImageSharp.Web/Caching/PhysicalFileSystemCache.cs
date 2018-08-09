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
        /// The configuration key for checking whether changes in source images should be accounted for when checking the cache.
        /// </summary>
        public const string CheckSourceChanged = "CheckSourceChanged";

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
        private readonly MemoryAllocator bufferManager;

        /// <summary>
        /// The middleware configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemCache"/> class.
        /// </summary>
        /// <param name="environment">The hosting environment the application is running in.</param>
        /// <param name="bufferManager">An <see cref="MemoryAllocator"/> instance used to allocate arrays transporting encoded image data.</param>
        /// <param name="options">The middleware configuration options.</param>
        public PhysicalFileSystemCache(IHostingEnvironment environment, MemoryAllocator bufferManager, IOptions<ImageSharpMiddlewareOptions> options)
        {
            Guard.NotNull(environment, nameof(environment));
            Guard.NotNull(bufferManager, nameof(bufferManager));
            Guard.NotNull(options, nameof(options));

            Guard.NotNullOrWhiteSpace(
              environment.WebRootPath,
              nameof(environment.WebRootPath),
              "The folder 'wwwroot' that contains the web-servable application content files is missing. Please add this folder to the application root to allow caching.");

            this.environment = environment;
            this.fileProvider = this.environment.WebRootFileProvider;
            this.bufferManager = bufferManager;
            this.options = options.Value;
        }

        /// <inheritdoc/>
        public IDictionary<string, string> Settings { get; }
            = new Dictionary<string, string>
            {
                { Folder, DefaultCacheFolder },
                { CheckSourceChanged, DefaultCheckSourceChanged }
            };

        /// <inheritdoc/>
        public async Task<IManagedByteBuffer> GetAsync(string key)
        {
            IFileInfo fileInfo = this.fileProvider.GetFileInfo(this.ToFilePath(key));

            IManagedByteBuffer buffer;

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return default;
            }

            using (Stream stream = fileInfo.CreateReadStream())
            {
                int length = (int)stream.Length;

                // Buffer is disposed of in the middleware
                buffer = this.bufferManager.AllocateManagedByteBuffer(length);
                await stream.ReadAsync(buffer.Array, 0, length);
            }

            return buffer;
        }

        /// <inheritdoc/>
        public Task<CachedInfo> IsExpiredAsync(HttpContext context, string key, DateTime minDateUtc)
        {
            bool.TryParse(this.Settings[CheckSourceChanged], out bool checkSource);

            IFileInfo cachedFileInfo = this.fileProvider.GetFileInfo(this.ToFilePath(key));
            bool exists = cachedFileInfo.Exists;
            DateTimeOffset lastModified = exists ? cachedFileInfo.LastModified : DateTimeOffset.MinValue;
            long length = exists ? cachedFileInfo.Length : 0;
            bool expired = true;

            // Checking the source adds overhead but is configurable. Defaults to false
            if (checkSource)
            {
                IFileInfo sourceFileInfo = this.fileProvider.GetFileInfo(context.Request.Path.Value);

                if (!sourceFileInfo.Exists)
                {
                    return Task.FromResult(default(CachedInfo));
                }

                // Check if the file exists and whether the last modified date is less than the min date.
                if (exists && lastModified.UtcDateTime > minDateUtc)
                {
                    // If it's newer than the cached file then it must be an update.
                    if (sourceFileInfo.LastModified.UtcDateTime < lastModified.UtcDateTime)
                    {
                        expired = false;
                    }
                }
            }
            else
            {
                if (exists && lastModified.UtcDateTime > minDateUtc)
                {
                    expired = false;
                }
            }

            return Task.FromResult(new CachedInfo(expired, lastModified, length));
        }

        /// <inheritdoc/>
        public async Task<DateTimeOffset> SetAsync(string key, IManagedByteBuffer value)
        {
            string path = Path.Combine(this.environment.WebRootPath, this.ToFilePath(key));
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fileStream = File.Create(path))
            {
                // TODO: Do buffered write here!
                await fileStream.WriteAsync(value.Array, 0, value.Memory.Length);
            }

            return File.GetLastWriteTimeUtc(path);
        }

        /// <summary>
        /// Converts the key into a nested file path.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ToFilePath(string key)
        {
            return $"{this.Settings[Folder]}/{string.Join("/", key.Substring(0, (int)this.options.CachedNameLength).ToCharArray())}/{key}";
        }
    }
}