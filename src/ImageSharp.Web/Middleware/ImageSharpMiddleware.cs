// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.Memory;

namespace SixLabors.ImageSharp.Web.Middleware
{
    /// <summary>
    /// Middleware for handling the processing of images via image requests.
    /// </summary>
    public class ImageSharpMiddleware
    {
        /// <summary>
        /// The key-lock used for limiting identical requests.
        /// </summary>
        private readonly IAsyncKeyLock asyncKeyLock;

        /// <summary>
        /// The function processing the Http request.
        /// </summary>
        private readonly RequestDelegate next;

        /// <summary>
        /// The configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// The type used for performing logging.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The buffer data pool.
        /// </summary>
        private readonly MemoryAllocator memoryAllocator;

        /// <summary>
        /// The parser for parsing commands from the current request.
        /// </summary>
        private readonly IRequestParser requestParser;

        /// <summary>
        /// The collection of image resolvers.
        /// </summary>
        private readonly IEnumerable<IImageProvider> resolvers;

        /// <summary>
        /// The collection of image processors.
        /// </summary>
        private readonly IEnumerable<IImageWebProcessor> processors;

        /// <summary>
        /// The image cache.
        /// </summary>
        private readonly IImageCache cache;

        /// <summary>
        /// The hashing implementation to use when generating cached file names.
        /// </summary>
        private readonly ICacheHash cacheHash;

        /// <summary>
        /// The collection of known commands gathered from the processors.
        /// </summary>
        private readonly IEnumerable<string> knownCommands;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="options">The middleware configuration options.</param>
        /// <param name="loggerFactory">An <see cref="ILoggerFactory"/> instance used to create loggers.</param>
        /// <param name="memoryAllocator">An <see cref="MemoryAllocator"/> instance used to allocate arrays transporting encoded image data.</param>
        /// <param name="requestParser">An <see cref="IRequestParser"/> instance used to parse image requests for commands.</param>
        /// <param name="resolvers">A collection of <see cref="IImageProvider"/> instances used to resolve images.</param>
        /// <param name="processors">A collection of <see cref="IImageWebProcessor"/> instances used to process images.</param>
        /// <param name="cache">An <see cref="IImageCache"/> instance used for caching images.</param>
        /// <param name="cacheHash">An <see cref="ICacheHash"/>instance used for calculating cached file names.</param>
        /// <param name="asyncKeyLock">An <see cref="IAsyncKeyLock"/> instance used for providing locking during processing.</param>
        public ImageSharpMiddleware(
            RequestDelegate next,
            IOptions<ImageSharpMiddlewareOptions> options,
            ILoggerFactory loggerFactory,
            MemoryAllocator memoryAllocator,
            IRequestParser requestParser,
            IEnumerable<IImageProvider> resolvers,
            IEnumerable<IImageWebProcessor> processors,
            IImageCache cache,
            ICacheHash cacheHash,
            IAsyncKeyLock asyncKeyLock)
        {
            Guard.NotNull(next, nameof(next));
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            Guard.NotNull(memoryAllocator, nameof(memoryAllocator));
            Guard.NotNull(requestParser, nameof(requestParser));
            Guard.NotNull(resolvers, nameof(resolvers));
            Guard.NotNull(processors, nameof(processors));
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(cache, nameof(cacheHash));
            Guard.NotNull(asyncKeyLock, nameof(asyncKeyLock));

            this.next = next;
            this.options = options.Value;
            this.memoryAllocator = memoryAllocator;
            this.requestParser = requestParser;
            this.resolvers = resolvers;
            this.processors = processors;
            this.cache = cache;
            this.cacheHash = cacheHash;
            this.asyncKeyLock = asyncKeyLock;

            var commands = new List<string>();
            foreach (IImageWebProcessor processor in this.processors)
            {
                commands.AddRange(processor.Commands);
            }

            this.knownCommands = commands;

            this.logger = loggerFactory.CreateLogger<ImageSharpMiddleware>();
        }

        /// <summary>
        /// Performs operations upon the current request.
        /// </summary>
        /// <param name="context">The current HTTP request context.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task Invoke(HttpContext context)
        {
            IDictionary<string, string> commands = this.requestParser.ParseRequestCommands(context)
                .Where(kvp => this.knownCommands.Contains(kvp.Key))
                .ToDictionary(p => p.Key, p => p.Value);

            this.options.OnParseCommands?.Invoke(new ImageCommandContext(context, commands, CommandParser.Instance));

            // Get the correct service for the request.
            IImageProvider provider = this.resolvers.FirstOrDefault(r => r.Match(context));

            if (provider == null || !await provider.IsValidRequestAsync(context).ConfigureAwait(false))
            {
                // Nothing to do. call the next delegate/middleware in the pipeline
                await this.next(context).ConfigureAwait(false);
                return;
            }

            // Create a cache key based on all the components of the requested url
            string uri = GetUri(context, commands);
            string key = this.cacheHash.Create(uri, this.options.CachedNameLength);

            // Prevent identical requests from running at the same time
            // This reduces the overheads of unnecessary processing plus avoids file locks
            bool processRequest = true;
            using (await this.asyncKeyLock.LockAsync(key).ConfigureAwait(false))
            {
                var imageContext = new ImageContext(context, this.options);
                IImageResolver resolvedImage = provider.Get(context);

                if (resolvedImage == null)
                {
                    // Log the error but let the pipeline handle the 404
                    this.logger.LogImageResolveFailed(imageContext.GetDisplayUrl());
                    processRequest = false;
                }

                if (processRequest)
                {
                    DateTime lastWriteTimeUtc = await resolvedImage.GetLastWriteTimeUtcAsync().ConfigureAwait(false);
                    CachedInfo info = await this.cache.IsExpiredAsync(context, key, lastWriteTimeUtc, DateTime.UtcNow.AddDays(-this.options.MaxCacheDays)).ConfigureAwait(false);

                    if (!info.Expired)
                    {
                        // We're pulling the image from the cache.
                        IImageResolver cachedImage = this.cache.Get(key);
                        using (Stream cachedBuffer = await cachedImage.OpenReadAsync().ConfigureAwait(false))
                        {
                            // Image is a cached image. Return the correct response now.
                            await this.SendResponse(imageContext, key, info.LastModifiedUtc, cachedBuffer).ConfigureAwait(false);
                        }

                        return;
                    }

                    // Not cached? Let's get it from the image resolver.
                    Stream outStream = null;
                    try
                    {
                        if (processRequest)
                        {
                            // No allocations here for inStream since we are passing the raw input stream.
                            // outStream allocation depends on the stream used.
                            outStream = resolvedImage.OpenWrite();
                            using (Stream inStream = await resolvedImage.OpenReadAsync().ConfigureAwait(false))
                            using (var image = FormattedImage.Load(this.options.Configuration, inStream))
                            {
                                image.Process(this.logger, this.processors, commands);
                                this.options.OnBeforeSave?.Invoke(image);
                                image.Save(outStream);
                            }

                            // Allow for any further optimization of the image. Always reset the position just in case.
                            if (outStream.CanSeek)
                            {
                                outStream.Position = 0;
                            }

                            this.options.OnProcessed?.Invoke(new ImageProcessingContext(context, outStream, commands, Path.GetExtension(key)));
                            if (outStream.CanSeek)
                            {
                                outStream.Position = 0;
                            }

                            DateTimeOffset cachedDate = await this.cache.SetAsync(key, outStream).ConfigureAwait(false);
                            await this.SendResponse(imageContext, key, cachedDate, outStream).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error internally then rethrow.
                        // We don't call next here, the pipeline will automatically handle it
                        this.logger.LogImageProcessingFailed(imageContext.GetDisplayUrl(), ex);
                        throw;
                    }
                    finally
                    {
                        outStream?.Dispose();
                    }
                }
            }

            if (!processRequest)
            {
                // Call the next delegate/middleware in the pipeline
                await this.next(context).ConfigureAwait(false);
            }
        }

        private async Task SendResponse(ImageContext imageContext, string key, DateTimeOffset lastModified, Stream stream)
        {
            imageContext.ComprehendRequestHeaders(lastModified, stream.Length);

            string contentType = FormatHelpers.GetContentType(this.options.Configuration, key);

            switch (imageContext.GetPreconditionState())
            {
                case ImageContext.PreconditionState.Unspecified:
                case ImageContext.PreconditionState.ShouldProcess:
                    if (imageContext.IsHeadRequest())
                    {
                        await imageContext.SendStatusAsync(ResponseConstants.Status200Ok, contentType).ConfigureAwait(false);
                    }

                    this.logger.LogImageServed(imageContext.GetDisplayUrl(), key);
                    await imageContext.SendAsync(contentType, stream).ConfigureAwait(false);

                    break;

                case ImageContext.PreconditionState.NotModified:
                    this.logger.LogImageNotModified(imageContext.GetDisplayUrl());
                    await imageContext.SendStatusAsync(ResponseConstants.Status304NotModified, contentType).ConfigureAwait(false);
                    break;
                case ImageContext.PreconditionState.PreconditionFailed:
                    this.logger.LogImagePreconditionFailed(imageContext.GetDisplayUrl());
                    await imageContext.SendStatusAsync(ResponseConstants.Status412PreconditionFailed, contentType).ConfigureAwait(false);
                    break;
                default:
                    var exception = new NotImplementedException(imageContext.GetPreconditionState().ToString());
                    Debug.Fail(exception.ToString());
                    throw exception;
            }
        }

        private static string GetUri(HttpContext context, IDictionary<string, string> commands)
        {
            var sb = new StringBuilder(context.Request.Host.ToString());

            string pathBase = context.Request.PathBase.ToString();
            if (!string.IsNullOrWhiteSpace(pathBase))
            {
                sb.AppendFormat("{0}/", pathBase);
            }

            string path = context.Request.Path.ToString();
            if (!string.IsNullOrWhiteSpace(path))
            {
                sb.Append(path);
            }

            sb.Append(QueryString.Create(commands));

            return sb.ToString().ToLowerInvariant();
        }
    }
}