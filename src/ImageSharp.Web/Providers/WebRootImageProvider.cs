// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Returns images from the web root file provider.
    /// </summary>
    public class WebRootImageProvider : IImageProvider
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
        /// Initializes a new instance of the <see cref="WebRootImageProvider"/> class.
        /// </summary>
        /// <param name="environment">The environment used by this middleware.</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public WebRootImageProvider(
#if NETCOREAPP2_1
            IHostingEnvironment environment,
#else
            IWebHostEnvironment environment,
#endif
            FormatUtilities formatUtilities)
        {
            Guard.NotNull(environment, nameof(environment));
            Guard.NotNull(environment.WebRootFileProvider, nameof(environment.WebRootFileProvider));

            this.fileProvider = environment.WebRootFileProvider;
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
            // Path has already been correctly parsed before here
            IFileInfo fileInfo = this.fileProvider.GetFileInfo(context.Request.Path.Value);

            // Check to see if the file exists
            if (!fileInfo.Exists)
            {
                return Task.FromResult<IImageResolver>(null);
            }

            var metadata = new ImageMetadata(fileInfo.LastModified.UtcDateTime, fileInfo.Length);

            return Task.FromResult<IImageResolver>(new FileInfoImageResolver(fileInfo, metadata));
        }
    }
}
