// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Caching;

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
        IWebHostEnvironment environment,
        FormatUtilities formatUtilities)
    {
        Guard.NotNull(options, nameof(options));
        Guard.NotNull(environment, nameof(environment));

        this.cacheRootPath = GetCacheRoot(options.Value, environment.WebRootPath, environment.ContentRootPath);
        this.cacheFolderDepth = (int)options.Value.CacheFolderDepth;
        this.formatUtilities = formatUtilities;
    }

    /// <summary>
    /// Determine the cache root path
    /// </summary>
    /// <param name="cacheOptions">The cache options.</param>
    /// <param name="webRootPath">The web root path.</param>
    /// <param name="contentRootPath">The content root path.</param>
    /// <returns><see cref="string"/> representing the fully qualified cache root path.</returns>
    /// <exception cref="InvalidOperationException">The cache root path cannot be determined.</exception>
    internal static string GetCacheRoot(PhysicalFileSystemCacheOptions cacheOptions, string webRootPath, string contentRootPath)
    {
        string cacheRootPath = cacheOptions.CacheRootPath ?? webRootPath;
        if (string.IsNullOrEmpty(cacheRootPath))
        {
            throw new InvalidOperationException("The cache root path cannot be determined, make sure it's explicitly configured or the webroot is set.");
        }

        if (!Path.IsPathFullyQualified(cacheRootPath))
        {
            // Ensure this is an absolute path (resolved to the content root path)
            cacheRootPath = Path.GetFullPath(cacheRootPath, contentRootPath);
        }

        string cacheFolderPath = Path.Combine(cacheRootPath, cacheOptions.CacheFolder);

        return PathUtilities.EnsureTrailingSlash(cacheFolderPath);
    }

    /// <inheritdoc/>
    public Task<IImageCacheResolver?> GetAsync(string key)
    {
        string path = Path.Combine(this.cacheRootPath, ToFilePath(key, this.cacheFolderDepth));

        FileInfo metaFileInfo = new(ToMetaDataFilePath(path));
        if (!metaFileInfo.Exists)
        {
            return Task.FromResult<IImageCacheResolver?>(null);
        }

        return Task.FromResult<IImageCacheResolver?>(new PhysicalFileSystemCacheResolver(metaFileInfo, this.formatUtilities));
    }

    /// <inheritdoc/>
    public async Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata)
    {
        string path = Path.Combine(this.cacheRootPath, ToFilePath(key, this.cacheFolderDepth));
        string imagePath = this.ToImageFilePath(path, metadata);
        string metaPath = ToMetaDataFilePath(path);
        string? directory = Path.GetDirectoryName(path);

        // Ensure cache directory is created before creating files
        if (!Directory.Exists(directory) && directory is not null)
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
    private static string ToMetaDataFilePath(string path) => $"{path}.meta";

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
                ReadOnlySpan<char> keySpan = new((char*)args.Ptr, args.Length);
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
