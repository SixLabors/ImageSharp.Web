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
            string path = Path.Join(this.providerRootPath, context.Request.Path.Value);

            // Check to see if the file exists
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                return Task.FromResult<IImageResolver>(null);
            }

            var metadata = new ImageMetadata(fileInfo.LastWriteTimeUtc, fileInfo.Length);
            return Task.FromResult<IImageResolver>(new PhysicalFileSystemResolver(fileInfo, metadata));
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
            string providerRoot = providerOptions.ProviderRootPath ?? webRootPath ?? "wwwroot";

            return Path.IsPathFullyQualified(providerRoot)
                ? providerRoot
                : Path.GetFullPath(providerRoot, contentRootPath);
        }
    }
}
