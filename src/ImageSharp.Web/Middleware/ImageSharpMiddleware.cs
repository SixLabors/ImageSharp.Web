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
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

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
        private static readonly AsyncKeyLock AsyncLock = new AsyncKeyLock();

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
        /// The collection of image providers.
        /// </summary>
        private readonly IImageProvider[] providers;

        /// <summary>
        /// The collection of image processors.
        /// </summary>
        private readonly IImageWebProcessor[] processors;

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
        private readonly HashSet<string> knownCommands;

        /// <summary>
        /// Contains various helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities formatUtilities;

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
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
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
            FormatUtilities formatUtilities)
        {
            Guard.NotNull(next, nameof(next));
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            Guard.NotNull(memoryAllocator, nameof(memoryAllocator));
            Guard.NotNull(requestParser, nameof(requestParser));
            Guard.NotNull(resolvers, nameof(resolvers));
            Guard.NotNull(processors, nameof(processors));
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(cacheHash, nameof(cacheHash));
            Guard.NotNull(AsyncLock, nameof(AsyncLock));

            this.next = next;
            this.options = options.Value;
            this.memoryAllocator = memoryAllocator;
            this.requestParser = requestParser;
            this.providers = resolvers as IImageProvider[] ?? resolvers.ToArray();
            this.processors = processors as IImageWebProcessor[] ?? processors.ToArray();
            this.cache = cache;
            this.cacheHash = cacheHash;

            var commands = new HashSet<string>();
            foreach (IImageWebProcessor processor in this.processors)
            {
                foreach (string command in processor.Commands)
                {
                    commands.Add(command);
                }
            }

            this.knownCommands = commands;

            this.logger = loggerFactory.CreateLogger<ImageSharpMiddleware>();
            this.formatUtilities = formatUtilities;
        }

#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Performs operations upon the current request.
        /// </summary>
        /// <param name="context">The current HTTP request context.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task Invoke(HttpContext context)
#pragma warning restore IDE1006 // Naming Styles
        {
            IDictionary<string, string> commands = this.requestParser.ParseRequestCommands(context);
            if (commands.Count > 0)
            {
                foreach (string command in new List<string>(commands.Keys))
                {
                    if (!this.knownCommands.Contains(command))
                    {
                        commands.Remove(command);
                    }
                }
            }

            this.options.OnParseCommands?.Invoke(new ImageCommandContext(context, commands, CommandParser.Instance));

            // Get the correct service for the request.
            IImageProvider provider = null;
            foreach (IImageProvider resolver in this.providers)
            {
                if (resolver.Match(context))
                {
                    provider = resolver;
                    break;
                }
            }

            if ((commands.Count == 0 && provider?.ProcessingBehavior != ProcessingBehavior.All) || provider?.IsValidRequest(context) != true)
            {
                // Nothing to do. call the next delegate/middleware in the pipeline
                await this.next(context);
                return;
            }

            bool processRequest = true;
            IImageResolver sourceImageResolver = await provider.GetAsync(context);

            if (sourceImageResolver == null)
            {
                // Log the error but let the pipeline handle the 404
                var imageContext = new ImageContext(context, this.options);
                this.logger.LogImageResolveFailed(imageContext.GetDisplayUrl());
                processRequest = false;
            }

            if (!processRequest)
            {
                // Call the next delegate/middleware in the pipeline
                await this.next(context);
                return;
            }

            await this.ProcessRequestAsync(context, processRequest, sourceImageResolver, new ImageContext(context, this.options), commands);
        }

        private async Task ProcessRequestAsync(HttpContext context, bool processRequest, IImageResolver sourceImageResolver, ImageContext imageContext, IDictionary<string, string> commands)
        {
            // Create a cache key based on all the components of the requested url
            string uri = GetUri(context, commands);
            string key = this.cacheHash.Create(uri, this.options.CachedNameLength);

            ImageMetadata sourceImageMetadata = default;
            if (processRequest)
            {
                // Lock any reads when a write is being done for the same key to prevent potential file locks.
                using (await AsyncLock.ReaderLockAsync(key))
                {
                    // Check to see if the cache contains this image
                    sourceImageMetadata = await sourceImageResolver.GetMetaDataAsync();
                    IImageCacheResolver cachedImageResolver = await this.cache.GetAsync(key);
                    if (cachedImageResolver != null)
                    {
                        ImageCacheMetadata cachedImageMetadata = await cachedImageResolver.GetMetaDataAsync();
                        if (cachedImageMetadata != default)
                        {
                            // Has the cached image expired or has the source image been updated?
                            if (cachedImageMetadata.SourceLastWriteTimeUtc == sourceImageMetadata.LastWriteTimeUtc
                                && cachedImageMetadata.CacheLastWriteTimeUtc > DateTimeOffset.Now.AddDays(-this.options.MaxCacheDays))
                            {
                                // We're pulling the image from the cache.
                                using (Stream cachedBuffer = await cachedImageResolver.OpenReadAsync())
                                {
                                    await this.SendResponseAsync(imageContext, key, cachedBuffer, cachedImageMetadata);
                                }

                                return;
                            }
                        }
                    }
                }

                // Not cached? Let's get it from the image resolver.
                ChunkedMemoryStream outStream = null;
                try
                {
                    if (processRequest)
                    {
                        // Enter a write lock which locks writing and any reads for the same request.
                        // This reduces the overheads of unnecessary processing plus avoids file locks.
                        using (await AsyncLock.WriterLockAsync(key))
                        {
                            // No allocations here for inStream since we are passing the raw input stream.
                            // outStream allocation depends on the memory allocator used.
                            ImageCacheMetadata cachedImageMetadata = default;
                            outStream = new ChunkedMemoryStream();
                            using (Stream inStream = await sourceImageResolver.OpenReadAsync())
                            {
                                IImageFormat format;

                                // No commands? We simply copy the stream across.
                                if (commands.Count == 0)
                                {
                                    format = Image.DetectFormat(this.options.Configuration, inStream);
                                    await inStream.CopyToAsync(outStream);
                                }
                                else
                                {
                                    using (var image = FormattedImage.Load(this.options.Configuration, inStream))
                                    {
                                        image.Process(this.logger, this.processors, commands);
                                        this.options.OnBeforeSave?.Invoke(image);
                                        image.Save(outStream);
                                        format = image.Format;
                                    }
                                }

                                // Check to see if the source metadata has a cachecontrol max-age value and use it to
                                // override the default max age from our options.
                                var maxAge = TimeSpan.FromDays(this.options.MaxBrowserCacheDays);
                                if (!sourceImageMetadata.CacheControlMaxAge.Equals(TimeSpan.MinValue))
                                {
                                    maxAge = sourceImageMetadata.CacheControlMaxAge;
                                }

                                cachedImageMetadata = new ImageCacheMetadata(
                                    sourceImageMetadata.LastWriteTimeUtc,
                                    DateTime.UtcNow,
                                    format.DefaultMimeType,
                                    maxAge);
                            }

                            // Allow for any further optimization of the image. Always reset the position just in case.
                            outStream.Position = 0;
                            string contentType = cachedImageMetadata.ContentType;
                            string extension = this.formatUtilities.GetExtensionFromContentType(contentType);
                            this.options.OnProcessed?.Invoke(new ImageProcessingContext(context, outStream, commands, contentType, extension));
                            outStream.Position = 0;

                            // Save the image to the cache and send the response to the caller.
                            await this.cache.SetAsync(key, outStream, cachedImageMetadata);
                            await this.SendResponseAsync(imageContext, key, outStream, cachedImageMetadata);
                        }
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

        private async Task SendResponseAsync(ImageContext imageContext, string key, Stream stream, ImageCacheMetadata metadata)
        {
            imageContext.ComprehendRequestHeaders(metadata.CacheLastWriteTimeUtc, stream.Length);

            switch (imageContext.GetPreconditionState())
            {
                case ImageContext.PreconditionState.Unspecified:
                case ImageContext.PreconditionState.ShouldProcess:
                    if (imageContext.IsHeadRequest())
                    {
                        await imageContext.SendStatusAsync(ResponseConstants.Status200Ok, metadata);
                    }

                    this.logger.LogImageServed(imageContext.GetDisplayUrl(), key);
                    await imageContext.SendAsync(stream, metadata);

                    break;

                case ImageContext.PreconditionState.NotModified:
                    this.logger.LogImageNotModified(imageContext.GetDisplayUrl());
                    await imageContext.SendStatusAsync(ResponseConstants.Status304NotModified, metadata);
                    break;
                case ImageContext.PreconditionState.PreconditionFailed:
                    this.logger.LogImagePreconditionFailed(imageContext.GetDisplayUrl());
                    await imageContext.SendStatusAsync(ResponseConstants.Status412PreconditionFailed, metadata);
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
