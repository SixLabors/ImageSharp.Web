// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Providers;

/// <summary>
/// Returns images from an <see cref="IFileProvider"/> abstraction.
/// </summary>
public abstract class FileProviderImageProvider : IImageProvider
{
    /// <summary>
    /// The file provider abstraction.
    /// </summary>
    private readonly IFileProvider fileProvider;

    /// <summary>
    /// Contains various format helper methods based on the current configuration.
    /// </summary>
    private readonly FormatUtilities formatUtilities;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileProviderImageProvider"/> class.
    /// </summary>
    /// <param name="fileProvider">The file provider.</param>
    /// <param name="processingBehavior">The processing behavior.</param>
    /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
    protected FileProviderImageProvider(IFileProvider fileProvider, ProcessingBehavior processingBehavior, FormatUtilities formatUtilities)
    {
        Guard.NotNull(fileProvider, nameof(fileProvider));
        Guard.NotNull(formatUtilities, nameof(formatUtilities));

        this.fileProvider = fileProvider;
        this.formatUtilities = formatUtilities;
        this.ProcessingBehavior = processingBehavior;
    }

    /// <inheritdoc/>
    public ProcessingBehavior ProcessingBehavior { get; }

    /// <inheritdoc/>
    public virtual Func<HttpContext, bool> Match { get; set; } = _ => true;

    /// <inheritdoc/>
    public virtual bool IsValidRequest(HttpContext context)
        => this.formatUtilities.TryGetExtensionFromUri(context.Request.GetDisplayUrl(), out _);

    /// <inheritdoc/>
    public Task<IImageResolver?> GetAsync(HttpContext context)
    {
        string? path = context.Request.Path.Value;
        if (path is not null)
        {
            IFileInfo fileInfo = this.fileProvider.GetFileInfo(path);
            if (fileInfo.Exists)
            {
                return Task.FromResult<IImageResolver?>(new FileProviderImageResolver(fileInfo));
            }
        }

        return Task.FromResult<IImageResolver?>(null);
    }
}
