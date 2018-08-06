// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Memory;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Middleware
{
    /// <summary>
    /// Middleware for handling the processing of images via image requests.
    /// </summary>
    public class ImageSharpMiddleware
    {
        /// <summary>
        /// The key-lock used for limiting identical requests
        /// </summary>
        private readonly IAsyncKeyLock asyncKeyLock;

        /// <summary>
        /// The function processing the Http request.
        /// </summary>
        private readonly RequestDelegate next;

        /// <summary>
        /// The configuration options
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// The type used for performing logging.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The buffer data pool.
        /// </summary>
        private readonly IBufferManager bufferManager;

        /// <summary>
        /// The parser for parsing commands from the current request.
        /// </summary>
        private readonly IRequestParser requestParser;

        /// <summary>
        /// The collection of image resolvers.
        /// </summary>
        private readonly IEnumerable<IImageResolver> resolvers;

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
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="options">The middleware configuration options</param>
        /// <param name="loggerFactory">An <see cref="ILoggerFactory"/> instance used to create loggers</param>
        /// <param name="bufferManager">An <see cref="IBufferManager"/> instance used to allocate arrays transporting encoded image data</param>
        /// <param name="requestParser">An <see cref="IRequestParser"/> instance used to parse image requests for commands</param>
        /// <param name="resolvers">A collection of <see cref="IImageResolver"/> instances used to resolve images</param>
        /// <param name="processors">A collection of <see cref="IImageWebProcessor"/> instances used to process images</param>
        /// <param name="cache">An <see cref="IImageCache"/> instance used for caching images</param>
        /// <param name="cacheHash">An <see cref="ICacheHash"/>instance used for calculating cached file names</param>
        /// <param name="asyncKeyLock">An <see cref="IAsyncKeyLock"/> instance used for providing locking during processing</param>
        public ImageSharpMiddleware(
            RequestDelegate next,
            IOptions<ImageSharpMiddlewareOptions> options,
            ILoggerFactory loggerFactory,
            IBufferManager bufferManager,
            IRequestParser requestParser,
            IEnumerable<IImageResolver> resolvers,
            IEnumerable<IImageWebProcessor> processors,
            IImageCache cache,
            ICacheHash cacheHash,
            IAsyncKeyLock asyncKeyLock)
        {
            Guard.NotNull(next, nameof(next));
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            Guard.NotNull(bufferManager, nameof(bufferManager));
            Guard.NotNull(requestParser, nameof(requestParser));
            Guard.NotNull(resolvers, nameof(resolvers));
            Guard.NotNull(processors, nameof(processors));
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(cache, nameof(cacheHash));
            Guard.NotNull(asyncKeyLock, nameof(asyncKeyLock));

            this.next = next;
            this.options = options.Value;
            this.bufferManager = bufferManager;
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
        /// <param name="context">The current HTTP request context</param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Invoke(HttpContext context)
        {
            IDictionary<string, string> commands = this.requestParser.ParseRequestCommands(context)
                .Where(kvp => this.knownCommands.Contains(kvp.Key))
                .ToDictionary(p => p.Key, p => p.Value);

            this.options.OnValidate?.Invoke(new ImageValidationContext(context, commands, CommandParser.Instance));

            if (!commands.Any())
            {
                // Nothing to do. call the next delegate/middleware in the pipeline
                await this.next(context);
                return;
            }

            // Create a cache key based on all the components of the requested url
            string uri = $"{context.Request.Host.ToString().ToLowerInvariant()}/{context.Request.PathBase.ToString().ToLowerInvariant()}/{context.Request.Path}{QueryString.Create(commands)}";
            string key = this.cacheHash.Create(uri, this.options.CachedNameLength);

            // Prevent identical requests from running at the same time
            // This reduces the overheads of unnecessary processing plus avoids file locks
            bool processRequest = true;
            using (await this.asyncKeyLock.LockAsync(key))
            {
                // Get the correct service for the request.
                IImageResolver resolver = this.resolvers.FirstOrDefault(r => r.Match(context));

                if (resolver == null || !await resolver.IsValidRequestAsync(context))
                {
                    // Nothing to do. Call the next delegate/middleware in the pipeline
                    processRequest = false;
                }

                if (processRequest)
                {
                    CachedInfo info = await this.cache.IsExpiredAsync(context, key, DateTime.UtcNow.AddDays(-this.options.MaxCacheDays));

                    var imageContext = new ImageContext(context, this.options);

                    if (info.Equals(default))
                    {
                        // Cache has tried to resolve the source image and failed
                        // Log the error but let the pipeline handle the 404
                        this.logger.LogImageResolveFailed(imageContext.GetDisplayUrl());
                        processRequest = false;
                    }

                    if (processRequest)
                    {
                        if (!info.Expired)
                        {
                            // We're pulling the buffer from the cache. This should be cleaned up after.
                            using (IByteBuffer cachedBuffer = await this.cache.GetAsync(key))
                            {
                                // Image is a cached image. Return the correct response now.
                                await this.SendResponse(imageContext, key, info.LastModifiedUtc, cachedBuffer);
                            }

                            return;
                        }

                        // Not cached? Let's get it from the image resolver.
                        IByteBuffer inBuffer = null;
                        IByteBuffer outBuffer = null;
                        MemoryStream outStream = null;
                        try
                        {
                            inBuffer = await resolver.ResolveImageAsync(context);
                            if (inBuffer == null || inBuffer.Array.Length == 0)
                            {
                                // Log the error but let the pipeline handle the 404
                                this.logger.LogImageResolveFailed(imageContext.GetDisplayUrl());
                                processRequest = false;
                            }

                            if (processRequest)
                            {
                                // No allocations here for inStream since we are passing the buffer.
                                // TODO: How to prevent the allocation in outStream? Passing a pooled buffer won't let stream grow if needed.
                                outStream = new MemoryStream();
                                using (var image = FormattedImage.Load(this.options.Configuration, inBuffer.Array))
                                {
                                    image.Process(this.logger, this.processors, commands);
                                    this.options.OnBeforeSave?.Invoke(image);
                                    image.Save(outStream);
                                }

                                // Allow for any further optimization of the image. Always reset the position just in case.
                                outStream.Position = 0;
                                this.options.OnProcessed?.Invoke(new ImageProcessingContext(context, outStream, commands, Path.GetExtension(key)));
                                outStream.Position = 0;
                                int outLength = (int)outStream.Length;

                                // Copy the out-stream to the pooled buffer.
                                outBuffer = this.bufferManager.Allocate(outLength);
                                await outStream.ReadAsync(outBuffer.Array, 0, outLength);

                                DateTimeOffset cachedDate = await this.cache.SetAsync(key, outBuffer);
                                await this.SendResponse(imageContext, key, cachedDate, outBuffer);
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
                            inBuffer?.Dispose();
                            outBuffer?.Dispose();
                        }
                    }
                }
            }

            if (!processRequest)
            {
                // Call the next delegate/middleware in the pipeline
                await this.next(context);
            }
        }

        private async Task SendResponse(ImageContext imageContext, string key, DateTimeOffset lastModified, IByteBuffer buffer)
        {
            imageContext.ComprehendRequestHeaders(lastModified, buffer.Length);

            string contentType = FormatHelpers.GetContentType(this.options.Configuration, key);

            switch (imageContext.GetPreconditionState())
            {
                case ImageContext.PreconditionState.Unspecified:
                case ImageContext.PreconditionState.ShouldProcess:
                    if (imageContext.IsHeadRequest())
                    {
                        await imageContext.SendStatusAsync(ResponseConstants.Status200Ok, contentType);
                    }

                    this.logger.LogImageServed(imageContext.GetDisplayUrl(), key);
                    await imageContext.SendAsync(contentType, buffer, buffer.Length);

                    break;

                case ImageContext.PreconditionState.NotModified:
                    this.logger.LogImageNotModified(imageContext.GetDisplayUrl());
                    await imageContext.SendStatusAsync(ResponseConstants.Status304NotModified, contentType);
                    break;
                case ImageContext.PreconditionState.PreconditionFailed:
                    this.logger.LogImagePreconditionFailed(imageContext.GetDisplayUrl());
                    await imageContext.SendStatusAsync(ResponseConstants.Status412PreconditionFailed, contentType);
                    break;
                default:
                    var exception = new NotImplementedException(imageContext.GetPreconditionState().ToString());
                    Debug.Fail(exception.ToString());
                    throw exception;
            }
        }
    }
}