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
            this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            this.formatUtilities = formatUtilities ?? throw new ArgumentNullException(nameof(formatUtilities));
        }

        /// <inheritdoc/>
        public ProcessingBehavior ProcessingBehavior { get; protected set; } = ProcessingBehavior.CommandOnly;

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context)
            => this.formatUtilities.TryGetExtensionFromUri(context.Request.GetDisplayUrl(), out _);

        /// <inheritdoc/>
        public Task<IImageResolver> GetAsync(HttpContext context)
        {
            IFileInfo fileInfo = this.fileProvider.GetFileInfo(context.Request.Path);
            if (!fileInfo.Exists)
            {
                return Task.FromResult<IImageResolver>(null);
            }

            return Task.FromResult<IImageResolver>(new FileProviderImageResolver(fileInfo));
        }
    }
}
