// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Memory;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Returns images stored in the local physical file system.
    /// </summary>
    public class PhysicalFileSystemResolver : IImageResolver
    {
        /// <summary>
        /// The file provider abstraction.
        /// </summary>
        private readonly IFileProvider fileProvider;

        /// <summary>
        /// The buffer data pool.
        /// </summary>
        private readonly IBufferDataPool bufferDataPool;

        /// <summary>
        /// The middleware configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemResolver"/> class.
        /// </summary>
        /// <param name="environment">The <see cref="IHostingEnvironment"/> used by this middleware</param>
        /// <param name="bufferDataPool">An <see cref="IBufferDataPool"/> instance used to enable reusing arrays transporting encoded image data</param>
        /// <param name="options">The middleware configuration options</param>
        public PhysicalFileSystemResolver(IHostingEnvironment environment, IBufferDataPool bufferDataPool, IOptions<ImageSharpMiddlewareOptions> options)
        {
            Guard.NotNull(environment, nameof(environment));
            Guard.NotNull(bufferDataPool, nameof(bufferDataPool));
            Guard.NotNull(options, nameof(options));

            this.fileProvider = environment.WebRootFileProvider;
            this.bufferDataPool = bufferDataPool;
            this.options = options.Value;
        }

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc/>
        public Task<bool> IsValidRequestAsync(HttpContext context, ILogger logger)
        {
            return Task.FromResult(FormatHelpers.GetExtension(this.options.Configuration, context.Request.GetDisplayUrl()) != null);
        }

        /// <inheritdoc/>
        public async Task<byte[]> ResolveImageAsync(HttpContext context, ILogger logger)
        {
            // Path has already been correctly parsed before here.
            IFileInfo fileInfo = this.fileProvider.GetFileInfo(context.Request.Path.Value);
            byte[] buffer;

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return null;
            }

            using (Stream stream = fileInfo.CreateReadStream())
            {
                // Buffer is returned to the pool in the middleware
                buffer = this.bufferDataPool.Rent((int)stream.Length);
                await stream.ReadAsync(buffer, 0, (int)stream.Length);
            }

            return buffer;
        }
    }
}