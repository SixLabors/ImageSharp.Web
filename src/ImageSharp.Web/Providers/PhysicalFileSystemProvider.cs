// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.Memory;

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
        /// The memory allocator.
        /// </summary>
        private readonly MemoryAllocator memoryAllocator;

        /// <summary>
        /// The middleware configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// Contains various helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities formatUtilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemProvider"/> class.
        /// </summary>
        /// <param name="options">The middleware configuration options.</param>
        /// <param name="environment">The <see cref="IHostingEnvironment"/> used by this middleware.</param>
        /// <param name="memoryAllocator">An <see cref="MemoryAllocator"/> instance used to allocate arrays transporting encoded image data.</param>
        public PhysicalFileSystemProvider(IOptions<ImageSharpMiddlewareOptions> options, IHostingEnvironment environment, MemoryAllocator memoryAllocator)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(environment, nameof(environment));
            Guard.NotNull(memoryAllocator, nameof(memoryAllocator));

            this.options = options.Value;
            this.fileProvider = environment.WebRootFileProvider;
            this.memoryAllocator = memoryAllocator;
            this.formatUtilities = new FormatUtilities(this.options.Configuration);
        }

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

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

            // We don't care about the content type nor cache control max age here.
            var metadata = new ImageMetaData(fileInfo.LastModified.UtcDateTime);
            return Task.FromResult<IImageResolver>(new PhysicalFileSystemResolver(fileInfo, metadata));
        }
    }
}