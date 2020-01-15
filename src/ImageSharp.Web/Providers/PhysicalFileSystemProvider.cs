// Copyright (c) Six Labors and contributors.
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
        /// <param name="environment">The <see cref="IHostingEnvironment"/> used by this middleware.</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public PhysicalFileSystemProvider(
            IHostingEnvironment environment,
            FormatUtilities formatUtilities)
        {
            Guard.NotNull(environment, nameof(environment));

            this.fileProvider = environment.WebRootFileProvider;
            this.formatUtilities = formatUtilities;
        }

        /// <inheritdoc/>
        public bool ProcessWhenNoCommands { get; set; } = false;

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context) => this.formatUtilities.GetExtensionFromUri(context.Request.GetDisplayUrl()) != null;

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

            var metadata = new ImageMetadata(fileInfo.LastModified.UtcDateTime);
            return Task.FromResult<IImageResolver>(new PhysicalFileSystemResolver(fileInfo, metadata));
        }
    }
}
