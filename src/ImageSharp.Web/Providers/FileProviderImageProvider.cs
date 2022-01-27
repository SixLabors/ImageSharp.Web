// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Returns images from an <see cref="IFileProvider"/> abstraction.
    /// </summary>
    public class FileProviderImageProvider : IImageProvider
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
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public FileProviderImageProvider(IFileProvider fileProvider, FormatUtilities formatUtilities)
        {
            Guard.NotNull(fileProvider, nameof(fileProvider));

            this.fileProvider = fileProvider;
            this.formatUtilities = formatUtilities;
        }

        /// <inheritdoc/>
        public ProcessingBehavior ProcessingBehavior { get; } = ProcessingBehavior.CommandOnly;

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public virtual bool IsValidRequest(HttpContext context)
            => this.formatUtilities.TryGetExtensionFromUri(context.Request.GetDisplayUrl(), out _);

        /// <inheritdoc/>
        public virtual Task<IImageResolver> GetAsync(HttpContext context)
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
