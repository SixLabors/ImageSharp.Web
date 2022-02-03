// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
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
        /// The file provider abstraction.
        /// </summary>
        private readonly IFileProvider fileProvider;

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
            Guard.NotNull(environment, nameof(environment));

            // ContentRootPath is never null.
            // https://github.com/dotnet/aspnetcore/blob/b89eba6c3cda331ee98063e3c4a04267ec540315/src/Hosting/Hosting/src/WebHostBuilder.cs#L262
            Guard.NotNullOrWhiteSpace(environment.WebRootPath, nameof(environment.WebRootPath));

            // Allow configuration of the provider without having to register everything
            PhysicalFileSystemProviderOptions providerOptions = options != null ? options.Value : new();
            string cacheRootPath = GetProviderRoot(providerOptions, environment.WebRootPath, environment.ContentRootPath);

            // Ensure provider directory is created before initializing the file provider
            Directory.CreateDirectory(cacheRootPath);
            this.fileProvider = new PhysicalFileProvider(cacheRootPath);
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
            // Path has already been correctly parsed before here.
            IFileInfo fileInfo = this.fileProvider.GetFileInfo(context.Request.Path.Value);

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return Task.FromResult<IImageResolver>(null);
            }

            var metadata = new ImageMetadata(fileInfo.LastModified.UtcDateTime, fileInfo.Length);
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
            string providerRoot = string.IsNullOrWhiteSpace(providerOptions.ProviderRootPath)
                ? webRootPath
                : providerOptions.ProviderRootPath;

            return Path.IsPathFullyQualified(providerRoot)
                ? providerRoot
                : Path.GetFullPath(providerRoot, contentRootPath);
        }
    }
}
