// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
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
        /// The cache configuration options.
        /// </summary>
        private readonly PhysicalFileSystemCacheOptions cacheOptions;

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
            this.cacheRootPath = GetCacheRoot(this.cacheOptions, environment.WebRootPath, environment.ContentRootPath);

            this.cachedNameLength = (int)options.Value.CachedNameLength;
            this.formatUtilities = formatUtilities;
        }

        /// <summary>
        /// Determine the cache root path
        /// </summary>
        /// <param name="cacheOptions">the cache options.</param>
        /// <param name="webRootPath">the webRootPath.</param>
        /// <param name="contentRootPath">the contentRootPath.</param>
        /// <returns>root path.</returns>
        internal static string GetCacheRoot(PhysicalFileSystemCacheOptions cacheOptions, string webRootPath, string contentRootPath)
        {
            string cacheRoot = string.IsNullOrEmpty(cacheOptions.CacheRoot)
                ? webRootPath
                : cacheOptions.CacheRoot;

            return Path.IsPathFullyQualified(cacheRoot)
                ? Path.Combine(cacheRoot, cacheOptions.CacheFolder)
                : Path.GetFullPath(Path.Combine(cacheRoot, cacheOptions.CacheFolder), contentRootPath);
        }

        /// <inheritdoc/>
        public Task<IImageCacheResolver> GetAsync(string key)
        {
            string path = ToFilePath(key, this.cachedNameLength);

            var metaFileInfo = new FileInfo(this.ToMetaDataFilePath(path));
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

            // Safe to always create
            Directory.CreateDirectory(directory);

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
