// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Implements a physical file system based cache.
    /// </summary>
    public class PhysicalFileSystemCache : IImageCache
    {
        /// <summary>
        /// The root path for the cache.
        /// </summary>
        private readonly string cacheRootPath;

        /// <summary>
        /// The length of the filename to use (minus the extension) when storing images in the image cache.
        /// </summary>
        private readonly int cachedNameLength;

        /// <summary>
        /// The file provider abstraction.
        /// </summary>
        private readonly IFileProvider fileProvider;

        /// <summary>
        /// The cache configuration options.
        /// </summary>
        private readonly PhysicalFileSystemCacheOptions cacheOptions;

        /// <summary>
        /// The middleware configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// Contains various format helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities formatUtilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemCache"/> class.
        /// </summary>
        /// <param name="cacheOptions">The cache configuration options.</param>
        /// <param name="environment">The hosting environment the application is running in.</param>
        /// <param name="options">The middleware configuration options.</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public PhysicalFileSystemCache(
            IOptions<PhysicalFileSystemCacheOptions> cacheOptions,
#if NETCOREAPP2_1
            IHostingEnvironment environment,
#else
            IWebHostEnvironment environment,
#endif
            IOptions<ImageSharpMiddlewareOptions> options,
            FormatUtilities formatUtilities)
        {
            Guard.NotNull(environment, nameof(environment));
            Guard.NotNull(options, nameof(options));
            Guard.NotNullOrWhiteSpace(environment.WebRootPath, nameof(environment.WebRootPath));

            // Allow configuration of the cache without having to register everything.
            this.cacheOptions = cacheOptions != null ? cacheOptions.Value : new PhysicalFileSystemCacheOptions();
            var cacheRoot = string.IsNullOrEmpty(this.cacheOptions.CacheRoot)
                ? environment.WebRootPath
                : this.cacheOptions.CacheRoot;
            this.cacheRootPath = Path.Combine(cacheRoot, this.cacheOptions.CacheFolder);
            if (!Directory.Exists(this.cacheRootPath))
            {
                Directory.CreateDirectory(this.cacheRootPath);
            }

            this.fileProvider = new PhysicalFileProvider(this.cacheRootPath);
            this.options = options.Value;
            this.cachedNameLength = (int)this.options.CachedNameLength;
            this.formatUtilities = formatUtilities;
        }

        /// <inheritdoc/>
        public Task<IImageCacheResolver> GetAsync(string key)
        {
            string path = ToFilePath(key, this.cachedNameLength);

            IFileInfo metaFileInfo = this.fileProvider.GetFileInfo(this.ToMetaDataFilePath(path));
            if (!metaFileInfo.Exists)
            {
                return Task.FromResult<IImageCacheResolver>(null);
            }

            return Task.FromResult<IImageCacheResolver>(new PhysicalFileSystemCacheResolver(metaFileInfo, this.formatUtilities));
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata)
        {
            string path = Path.Combine(this.cacheRootPath, ToFilePath(key, this.cachedNameLength));
            string imagePath = this.ToImageFilePath(path, metadata);
            string metaPath = this.ToMetaDataFilePath(path);
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fileStream = File.Create(imagePath))
            {
                await stream.CopyToAsync(fileStream);
            }

            using (FileStream fileStream = File.Create(metaPath))
            {
                await metadata.WriteAsync(fileStream);
            }
        }

        /// <summary>
        /// Gets the path to the image file based on the supplied root and metadata.
        /// </summary>
        /// <param name="path">The root path.</param>
        /// <param name="metaData">The image metadata.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ToImageFilePath(string path, in ImageCacheMetadata metaData)
            => $"{path}.{this.formatUtilities.GetExtensionFromContentType(metaData.ContentType)}";

        /// <summary>
        /// Gets the path to the image file based on the supplied root.
        /// </summary>
        /// <param name="path">The root path.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ToMetaDataFilePath(string path) => $"{path}.meta";

        /// <summary>
        /// Converts the key into a nested file path.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cachedNameLength">The length of the cached file name minus the extension.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe string ToFilePath(string key, int cachedNameLength)
        {
            // Each key substring char + separator + key
            int length = (cachedNameLength * 2) + key.Length;
            fixed (char* keyPtr = key)
            {
                return string.Create(length, (Ptr: (IntPtr)keyPtr, key.Length), (chars, args) =>
                {
                    const char separator = '/';
                    var keySpan = new ReadOnlySpan<char>((char*)args.Ptr, args.Length);
                    ref char keyRef = ref MemoryMarshal.GetReference(keySpan);
                    ref char charRef = ref MemoryMarshal.GetReference(chars);

                    int index = 0;
                    for (int i = 0; i < cachedNameLength; i++)
                    {
                        Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref keyRef, i);
                        Unsafe.Add(ref charRef, index++) = separator;
                    }

                    for (int i = 0; i < keySpan.Length; i++)
                    {
                        Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref keyRef, i);
                    }
                });
            }
        }
    }
}
