// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.Extensions.FileProviders;

namespace SixLabors.ImageSharp.Web.Resolvers;

/// <summary>
/// Provides means to manage image buffers from an <see cref="IFileInfo"/> instance.
/// </summary>
public sealed class FileProviderImageResolver : IImageResolver
{
    private readonly IFileInfo fileInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileProviderImageResolver"/> class.
    /// </summary>
    /// <param name="fileInfo">The file info.</param>
    public FileProviderImageResolver(IFileInfo fileInfo) => this.fileInfo = fileInfo;

    /// <inheritdoc/>
    public Task<ImageMetadata> GetMetaDataAsync() => Task.FromResult(new ImageMetadata(this.fileInfo.LastModified.UtcDateTime, this.fileInfo.Length));

    /// <inheritdoc/>
    public Task<Stream> OpenReadAsync() => Task.FromResult(this.fileInfo.CreateReadStream());
}
