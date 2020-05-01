// Copyright (c) Six Labors and contributors.
// Licensed under the GNU Affero General Public License, Version 3.

using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Resolvers;

// TODO: Do we add cleanup to this? Scalable caches probably shouldn't do so.
namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Specifies the contract for caching images.
    /// </summary>
    public interface IImageCache
    {
        /// <summary>
        /// Gets the image resolver associated with the specified key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The <see cref="IImageResolver"/>.</returns>
        Task<IImageCacheResolver> GetAsync(string key);

        /// <summary>
        /// Sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="stream">The stream containing the image to store.</param>
        /// <param name="metadata">The <see cref="ImageCacheMetadata"/> associated with the image to store.</param>
        /// <returns>The task.</returns>
        Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata);
    }
}
