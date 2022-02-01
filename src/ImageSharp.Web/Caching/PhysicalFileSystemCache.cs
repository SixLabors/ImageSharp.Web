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
        /// If set to <c>true</c> uses the legacy/full length file name.
        /// </summary>
        private readonly bool useLegacyName;

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
            PhysicalFileSystemCacheOptions cacheOptions = options != null ? options.Value : new PhysicalFileSystemCacheOptions();
            this.cacheRootPath = GetCacheRoot(cacheOptions, environment.WebRootPath, environment.ContentRootPath);
            this.cacheFolderDepth = (int)cacheOptions.CacheFolderDepth;
            this.useLegacyName = cacheOptions.UseLegacyName;

            // Ensure cache directory is created before initializing the file provider
            Directory.CreateDirectory(this.cacheRootPath);

            this.fileProvider = new PhysicalFileProvider(this.cacheRootPath);
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
            string path = ToFilePath(key, this.cacheFolderDepth, this.useLegacyName);

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
            string path = Path.Combine(this.cacheRootPath, ToFilePath(key, this.cacheFolderDepth, this.useLegacyName));
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
        /// <param name="legacyName">If set to <c>true</c> uses the legacy/full length file name.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe string ToFilePath(string key, int cacheFolderDepth, bool legacyName)
        {
            int length;
            int nameStartIndex;
            if (legacyName)
            {
                // Depth cannot exceed key length
                if (cacheFolderDepth > key.Length)
                {
                    cacheFolderDepth = key.Length;
                }

                length = (cacheFolderDepth * 2) + key.Length;
                nameStartIndex = 0;
            }
            else
            {
                // Ensure we keep at least 1 character for the file name
                if (cacheFolderDepth >= key.Length)
                {
                    cacheFolderDepth = key.Length - 1;
                }

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
