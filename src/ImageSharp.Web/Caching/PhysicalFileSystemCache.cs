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
        /// The depth of the nested cache folders structure to store the images.
        /// </summary>
        private readonly int cacheFolderDepth;

        /// <summary>
        /// The file provider abstraction.
        /// </summary>
        private readonly IFileProvider fileProvider;

        /// <summary>
        /// Contains various format helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities formatUtilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemCache"/> class.
        /// </summary>
        /// <param name="options">The cache configuration options.</param>
        /// <param name="environment">The hosting environment the application is running in.</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public PhysicalFileSystemCache(
            IOptions<PhysicalFileSystemCacheOptions> options,
#if NETCOREAPP2_1
            IHostingEnvironment environment,
#else
            IWebHostEnvironment environment,
#endif
            FormatUtilities formatUtilities)
        {
            Guard.NotNull(environment, nameof(environment));
            Guard.NotNull(options, nameof(options));
            Guard.NotNullOrWhiteSpace(environment.WebRootPath, nameof(environment.WebRootPath));

            // Allow configuration of the cache without having to register everything
            PhysicalFileSystemCacheOptions cacheOptions = options != null ? options.Value : new();
            this.cacheRootPath = GetCacheRoot(cacheOptions, environment.WebRootPath, environment.ContentRootPath);
            this.cacheFolderDepth = (int)cacheOptions.CacheFolderDepth;

            // Ensure cache directory is created before initializing the file provider
            Directory.CreateDirectory(this.cacheRootPath);

            this.fileProvider = new PhysicalFileProvider(this.cacheRootPath);
            this.formatUtilities = formatUtilities;
        }

        /// <summary>
        /// Determine the cache root path
        /// </summary>
        /// <param name="cacheOptions">The cache options.</param>
        /// <param name="webRootPath">The web root path.</param>
        /// <param name="contentRootPath">The content root path.</param>
        /// <returns><see cref="string"/> representing the fully qualified cache root path.</returns>
        internal static string GetCacheRoot(PhysicalFileSystemCacheOptions cacheOptions, string webRootPath, string contentRootPath)
        {
            string cacheRoot = string.IsNullOrWhiteSpace(cacheOptions.CacheRoot)
                ? webRootPath
                : cacheOptions.CacheRoot;

            return Path.IsPathFullyQualified(cacheRoot)
                ? Path.Combine(cacheRoot, cacheOptions.CacheFolder)
                : Path.GetFullPath(Path.Combine(cacheRoot, cacheOptions.CacheFolder), contentRootPath);
        }

        /// <inheritdoc/>
        public Task<IImageCacheResolver> GetAsync(string key)
        {
            string path = ToFilePath(key, this.cacheFolderDepth);

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
            string path = Path.Combine(this.cacheRootPath, ToFilePath(key, this.cacheFolderDepth));
            string imagePath = this.ToImageFilePath(path, metadata);
            string metaPath = this.ToMetaDataFilePath(path);
            string directory = Path.GetDirectoryName(path);

            // Ensure cache directory is created before creating files
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
        /// <param name="cacheFolderDepth">The depth of the nested cache folders structure to store the images.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe string ToFilePath(string key, int cacheFolderDepth)
        {
            if (cacheFolderDepth == 0)
            {
                // Short-circuit when not nesting folders
                return key;
            }

            int length;
            int nameStartIndex;
            if (cacheFolderDepth >= key.Length)
            {
                // Keep all characters in file name (legacy behavior)
                cacheFolderDepth = key.Length;
                length = (cacheFolderDepth * 2) + key.Length;
                nameStartIndex = 0;
            }
            else
            {
                // Remove characters used in folders from file name
                length = cacheFolderDepth + key.Length;
                nameStartIndex = cacheFolderDepth;
            }

            fixed (char* keyPtr = key)
            {
                return string.Create(length, (Ptr: (IntPtr)keyPtr, key.Length), (chars, args) =>
                {
                    const char separator = '/';
                    var keySpan = new ReadOnlySpan<char>((char*)args.Ptr, args.Length);
                    ref char keyRef = ref MemoryMarshal.GetReference(keySpan);
                    ref char charRef = ref MemoryMarshal.GetReference(chars);

                    int index = 0;
                    for (int i = 0; i < cacheFolderDepth; i++)
                    {
                        Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref keyRef, i);
                        Unsafe.Add(ref charRef, index++) = separator;
                    }

                    for (int i = nameStartIndex; i < keySpan.Length; i++)
                    {
                        Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref keyRef, i);
                    }
                });
            }
        }
    }
}
