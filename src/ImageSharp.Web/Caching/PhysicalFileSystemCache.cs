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
using SixLabors.ImageSharp.Web.Memory;
using SixLabors.ImageSharp.Web.Middleware;

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
        /// The buffer data pool.
        /// </summary>
        private readonly IBufferDataPool bufferDataPool;

        /// <summary>
        /// The middleware configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemCache"/> class.
        /// </summary>
        /// <param name="environment">The hosting environment the application is running in</param>
        /// <param name="bufferDataPool">An <see cref="IBufferDataPool"/> instance used to enable reusing arrays transporting encoded image data</param>
        /// <param name="options">The middleware configuration options</param>
        public PhysicalFileSystemCache(IHostingEnvironment environment, IBufferDataPool bufferDataPool, IOptions<ImageSharpMiddlewareOptions> options)
        {
            Guard.NotNull(environment, nameof(environment));
            Guard.NotNull(bufferDataPool, nameof(bufferDataPool));
            Guard.NotNull(options, nameof(options));

            this.environment = environment;
            this.fileProvider = this.environment.WebRootFileProvider;
            this.bufferDataPool = bufferDataPool;
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
        public async Task<CachedBuffer> GetAsync(string key)
        {
            IFileInfo fileInfo = this.fileProvider.GetFileInfo(this.ToFilePath(key));

            byte[] buffer;

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return default(CachedBuffer);
            }

            long length;
            using (Stream stream = fileInfo.CreateReadStream())
            {
                length = stream.Length;

                // Buffer is returned to the pool in the middleware
                buffer = this.bufferDataPool.Rent((int)length);
                await stream.ReadAsync(buffer, 0, (int)length);
            }

            return new CachedBuffer(buffer, length);
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
        public async Task<DateTimeOffset> SetAsync(string key, byte[] value, int length)
        {
            Guard.NotNullOrEmpty(
                this.environment.WebRootPath,
                nameof(this.environment.WebRootPath),
                "The folder 'wwwroot' that contains the web-servable application content files is missing. Please add this folder to the application root to allow caching.");

            string path = Path.Combine(this.environment.WebRootPath, this.ToFilePath(key));
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fileStream = File.Create(path))
            {
                await fileStream.WriteAsync(value, 0, length);
            }

            return File.GetLastWriteTimeUtc(path);
        }

        /// <summary>
        /// Converts the key into a nested file path.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The <see cref="string"/></returns>
        private string ToFilePath(string key)
        {
            return $"{this.Settings[Folder]}/{string.Join("/", key.Substring(0, (int)this.options.CachedNameLength).ToCharArray())}/{key}";
        }
    }
}