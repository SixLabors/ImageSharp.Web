// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace SixLabors.ImageSharp.Web.Providers;

/// <summary>
/// Returns images stored in the local physical file system.
/// </summary>
public sealed class PhysicalFileSystemProvider : FileProviderImageProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalFileSystemProvider"/> class.
    /// </summary>
    /// <param name="options">The provider configuration options.</param>
    /// <param name="environment">The environment used by this middleware.</param>
    /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
    public PhysicalFileSystemProvider(
        IOptions<PhysicalFileSystemProviderOptions> options,
        IWebHostEnvironment environment,
        FormatUtilities formatUtilities)
        : base(GetProvider(options, environment), options.Value.ProcessingBehavior, formatUtilities)
    {
    }

    /// <summary>
    /// Determine the provider root path
    /// </summary>
    /// <param name="options">The provider options.</param>
    /// <param name="webRootPath">The web root path.</param>
    /// <param name="contentRootPath">The content root path.</param>
    /// <returns><see cref="string"/> representing the fully qualified provider root path.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the provider root path cannot be determined.
    /// </exception>
    internal static string GetProviderRoot(PhysicalFileSystemProviderOptions options, string webRootPath, string contentRootPath)
    {
        string providerRootPath = options.ProviderRootPath ?? webRootPath;
        if (string.IsNullOrEmpty(providerRootPath))
        {
            throw new InvalidOperationException("The provider root path cannot be determined, make sure it's explicitly configured or the webroot is set.");
        }

        if (!Path.IsPathFullyQualified(providerRootPath))
        {
            // Ensure this is an absolute path (resolved to the content root path)
            providerRootPath = Path.GetFullPath(providerRootPath, contentRootPath);
        }

        return PathUtilities.EnsureTrailingSlash(providerRootPath);
    }

    private static PhysicalFileProvider GetProvider(
        IOptions<PhysicalFileSystemProviderOptions> options,
        IWebHostEnvironment environment)
    {
        Guard.NotNull(options, nameof(options));
        Guard.NotNull(environment, nameof(environment));
        return new(GetProviderRoot(options.Value, environment.WebRootPath, environment.ContentRootPath));
    }
}
