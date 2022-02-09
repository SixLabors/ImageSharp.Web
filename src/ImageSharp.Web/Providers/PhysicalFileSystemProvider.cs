// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Returns images stored in the local physical file system.
    /// </summary>
    public class PhysicalFileSystemProvider : IImageProvider
    {
        /// <summary>
        /// The root path for the provider.
        /// </summary>
        private readonly string providerRootPath;

        /// <summary>
        /// Contains various format helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities formatUtilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemProvider"/> class.
        /// </summary>
        /// <param name="options">The provider configuration options.</param>
        /// <param name="environment">The environment used by this middleware.</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public PhysicalFileSystemProvider(
            IOptions<PhysicalFileSystemProviderOptions> options,
#if NETCOREAPP2_1
            IHostingEnvironment environment,
#else
            IWebHostEnvironment environment,
#endif
            FormatUtilities formatUtilities)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(environment, nameof(environment));

            this.providerRootPath = GetProviderRoot(options.Value, environment.WebRootPath, environment.ContentRootPath);
            this.formatUtilities = formatUtilities;
        }

        /// <inheritdoc/>
        public ProcessingBehavior ProcessingBehavior { get; } = ProcessingBehavior.CommandOnly;

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context)
            => this.formatUtilities.TryGetExtensionFromUri(context.Request.GetDisplayUrl(), out _);

        /// <inheritdoc/>
        public Task<IImageResolver> GetAsync(HttpContext context)
        {
            // Use Join because request path starts with a slash
            string fullPath = Path.GetFullPath(Path.Join(this.providerRootPath, context.Request.Path.Value));
            if (PathUtils.IsUnderneathRoot(fullPath, this.providerRootPath))
            {
                // Check to see if the file exists
                var fileInfo = new FileInfo(fullPath);
                if (fileInfo.Exists)
                {
                    var metadata = new ImageMetadata(fileInfo.LastWriteTimeUtc, fileInfo.Length);
                    return Task.FromResult<IImageResolver>(new PhysicalFileSystemResolver(fileInfo, metadata));
                }
            }

            return Task.FromResult<IImageResolver>(null);
        }

        /// <summary>
        /// Determine the provider root path
        /// </summary>
        /// <param name="providerOptions">The provider options.</param>
        /// <param name="webRootPath">The web root path.</param>
        /// <param name="contentRootPath">The content root path.</param>
        /// <returns><see cref="string"/> representing the fully qualified provider root path.</returns>
        internal static string GetProviderRoot(PhysicalFileSystemProviderOptions providerOptions, string webRootPath, string contentRootPath)
        {
            string providerRootPath = providerOptions.ProviderRootPath ?? webRootPath;
            if (string.IsNullOrEmpty(providerRootPath))
            {
                throw new InvalidOperationException("The provider root path can't be determined, make sure it's explicitly configured or the webroot is set.");
            }

            if (!Path.IsPathFullyQualified(providerRootPath))
            {
                // Ensure this is an absolute path (resolved to the content root path)
                providerRootPath = Path.GetFullPath(providerRootPath, contentRootPath);
            }

            return PathUtils.EnsureTrailingSlash(providerRootPath);
        }
    }
}
