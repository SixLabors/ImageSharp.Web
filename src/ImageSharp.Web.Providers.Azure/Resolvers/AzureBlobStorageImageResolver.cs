// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Net.Http.Headers;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SixLabors.ImageSharp.Web.Resolvers.Azure;

/// <summary>
/// Provides means to manage image buffers within the Azure Blob file system.
/// </summary>
public class AzureBlobStorageImageResolver : IImageResolver
{
    private readonly BlobClient blob;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobStorageImageResolver"/> class.
    /// </summary>
    /// <param name="blob">The Azure blob.</param>
    public AzureBlobStorageImageResolver(BlobClient blob)
        => this.blob = blob;

    /// <inheritdoc/>
    public async Task<ImageMetadata> GetMetaDataAsync()
    {
        // I've had a good read through the SDK source and I believe we cannot get
        // a 304 here since 'If-Modified-Since' header is not set by default.
        BlobProperties properties = (await this.blob.GetPropertiesAsync()).Value;

        // Try to parse the max age from the source. If it's not zero then we pass it along
        // to set the cache control headers for the response.
        TimeSpan maxAge = TimeSpan.MinValue;
        if (CacheControlHeaderValue.TryParse(properties.CacheControl, out CacheControlHeaderValue? cacheControl))
        {
            // Weirdly passing null to TryParse returns true.
            if (cacheControl?.MaxAge.HasValue == true)
            {
                maxAge = cacheControl.MaxAge.Value;
            }
        }

        return new ImageMetadata(properties.LastModified.UtcDateTime, maxAge, properties.ContentLength);
    }

    /// <inheritdoc/>
    public Task<Stream> OpenReadAsync() => this.blob.OpenReadAsync();
}
