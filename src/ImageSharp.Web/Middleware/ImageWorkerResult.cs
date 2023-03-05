// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Middleware;

/// <summary>
/// Provides an asynchronous worker result.
/// </summary>
internal readonly struct ImageWorkerResult
{
    public ImageWorkerResult(ImageMetadata sourceImageMetadata)
    {
        this.IsNewOrUpdated = true;
        this.SourceImageMetadata = sourceImageMetadata;
        this.CacheImageMetadata = default;
        this.Resolver = default;
    }

    public ImageWorkerResult(ImageMetadata sourceImageMetadata, ImageCacheMetadata cacheImageMetadata, IImageCacheResolver resolver)
    {
        this.IsNewOrUpdated = false;
        this.SourceImageMetadata = sourceImageMetadata;
        this.CacheImageMetadata = cacheImageMetadata;
        this.Resolver = resolver;
    }

    public ImageWorkerResult(ImageCacheMetadata cacheImageMetadata, IImageCacheResolver? resolver)
    {
        this.IsNewOrUpdated = false;
        this.SourceImageMetadata = default;
        this.CacheImageMetadata = cacheImageMetadata;
        this.Resolver = resolver;
    }

    public bool IsNewOrUpdated { get; }

    public ImageMetadata SourceImageMetadata { get; }

    public ImageCacheMetadata CacheImageMetadata { get; }

    public IImageCacheResolver? Resolver { get; }
}
