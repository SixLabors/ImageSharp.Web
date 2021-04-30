// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using SixLabors.ImageSharp.Formats;
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
        /// The write worker used for limiting identical requests.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Task<ImageWorkerResult>> WriteWorkers
            = new ConcurrentDictionary<string, Task<ImageWorkerResult>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The read worker used for limiting identical requests.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Task<ImageWorkerResult>> ReadWorkers
            = new ConcurrentDictionary<string, Task<ImageWorkerResult>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Used to temporarily store source metadata reads to reduce the overhead of cache lookups.
        /// </summary>
        private static readonly ConcurrentTLruCache<string, ImageMetadata> SourceMetadataLru
            = new ConcurrentTLruCache<string, ImageMetadata>(1024, TimeSpan.FromMinutes(5));

        /// <summary>
        /// Used to temporarily store cache resolver reads to reduce the overhead of cache lookups.
        /// </summary>
        private static readonly ConcurrentTLruCache<string, ValueTuple<IImageCacheResolver, ImageCacheMetadata>> CacheResolverLru
            = new ConcurrentTLruCache<string, ValueTuple<IImageCacheResolver, ImageCacheMetadata>>(1024, TimeSpan.FromMinutes(5));

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
        /// Used to parse processing commands.
        /// </summary>
        private readonly CommandParser commandParser;

        /// <summary>
        /// The culture to use when parsing processing commands.
        /// </summary>
        private readonly CultureInfo parserCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="options">The middleware configuration options.</param>
        /// <param name="loggerFactory">An <see cref="ILoggerFactory"/> instance used to create loggers.</param>
        /// <param name="requestParser">An <see cref="IRequestParser"/> instance used to parse image requests for commands.</param>
        /// <param name="resolvers">A collection of <see cref="IImageProvider"/> instances used to resolve images.</param>
        /// <param name="processors">A collection of <see cref="IImageWebProcessor"/> instances used to process images.</param>
        /// <param name="cache">An <see cref="IImageCache"/> instance used for caching images.</param>
        /// <param name="cacheHash">An <see cref="ICacheHash"/>instance used for calculating cached file names.</param>
        /// <param name="commandParser">The command parser</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public ImageSharpMiddleware(
            RequestDelegate next,
            IOptions<ImageSharpMiddlewareOptions> options,
            ILoggerFactory loggerFactory,
            IRequestParser requestParser,
            IEnumerable<IImageProvider> resolvers,
            IEnumerable<IImageWebProcessor> processors,
            IImageCache cache,
            ICacheHash cacheHash,
            CommandParser commandParser,
            FormatUtilities formatUtilities)
        {
            Guard.NotNull(next, nameof(next));
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            Guard.NotNull(requestParser, nameof(requestParser));
            Guard.NotNull(resolvers, nameof(resolvers));
            Guard.NotNull(processors, nameof(processors));
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(cacheHash, nameof(cacheHash));
            Guard.NotNull(commandParser, nameof(commandParser));
            Guard.NotNull(formatUtilities, nameof(formatUtilities));

            this.next = next;
            this.options = options.Value;
            this.requestParser = requestParser;
            this.providers = resolvers as IImageProvider[] ?? resolvers.ToArray();
            this.processors = processors as IImageWebProcessor[] ?? processors.ToArray();
            this.cache = cache;
            this.cacheHash = cacheHash;
            this.commandParser = commandParser;
            this.parserCulture = this.options.UseInvariantParsingCulture
                ? CultureInfo.InvariantCulture
                : CultureInfo.CurrentCulture;

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
                // Strip out any unknown commands.
                foreach (string command in new List<string>(commands.Keys))
                {
                    if (!this.knownCommands.Contains(command))
                    {
                        commands.Remove(command);
                    }
                }
            }

            await this.options.OnParseCommandsAsync.Invoke(
                new ImageCommandContext(context, commands, this.commandParser, this.parserCulture));

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

            if ((commands.Count == 0 && provider?.ProcessingBehavior != ProcessingBehavior.All)
                || provider?.IsValidRequest(context) != true)
            {
                // Nothing to do. call the next delegate/middleware in the pipeline
                await this.next(context);
                return;
            }

            IImageResolver sourceImageResolver = await provider.GetAsync(context);

            if (sourceImageResolver is null)
            {
                // Log the error but let the pipeline handle the 404
                // by calling the next delegate/middleware in the pipeline.
                var imageContext = new ImageContext(context, this.options);
                this.logger.LogImageResolveFailed(imageContext.GetDisplayUrl());
                await this.next(context);
                return;
            }

            await this.ProcessRequestAsync(
                context,
                sourceImageResolver,
                new ImageContext(context, this.options),
                commands);
        }

        private async Task ProcessRequestAsync(
            HttpContext context,
            IImageResolver sourceImageResolver,
            ImageContext imageContext,
            IDictionary<string, string> commands)
        {
            // Create a cache key based on all the components of the requested url
            string uri = GetUri(context, commands);
            string key = this.cacheHash.Create(uri, this.options.CachedNameLength);

            // Check the cache, if present, not out of date and not requiring and update
            // we'll simply serve the file from there.
            ImageWorkerResult readResult = default;
            try
            {
                readResult = await this.IsNewOrUpdatedAsync(sourceImageResolver, imageContext, key);
            }
            finally
            {
                ReadWorkers.TryRemove(key, out Task<ImageWorkerResult> _);
            }

            if (!readResult.IsNewOrUpdated)
            {
                await this.SendResponseAsync(imageContext, key, readResult.CacheImageMetadata, readResult.Resolver);
                return;
            }

            // Not cached, or is updated? Let's get it from the image resolver.
            var sourceImageMetadata = readResult.SourceImageMetadata;

            // Enter an asynchronous write worker which prevents multiple writes and delays any reads for the same request.
            // This reduces the overheads of unnecessary processing.
            try
            {
                ImageWorkerResult writeResult = await WriteWorkers.GetOrAddAsync(
                    key,
                    async (key) =>
                    {
                        RecyclableMemoryStream outStream = null;
                        try
                        {
                            // Prevent a second request from starting a read during write execution.
                            if (ReadWorkers.TryGetValue(key, out Task<ImageWorkerResult> readWork))
                            {
                                await readWork;
                            }

                            ImageCacheMetadata cachedImageMetadata = default;
                            outStream = new RecyclableMemoryStream(this.options.MemoryStreamManager);
                            IImageFormat format;

                            // 14.9.3 CacheControl Max-Age
                            // Check to see if the source metadata has a CacheControl Max-Age value
                            // and use it to override the default max age from our options.
                            TimeSpan maxAge = this.options.BrowserMaxAge;
                            if (!sourceImageMetadata.CacheControlMaxAge.Equals(TimeSpan.MinValue))
                            {
                                maxAge = sourceImageMetadata.CacheControlMaxAge;
                            }

                            using (Stream inStream = await sourceImageResolver.OpenReadAsync())
                            {
                                // No commands? We simply copy the stream across.
                                if (commands.Count == 0)
                                {
                                    await inStream.CopyToAsync(outStream);
                                    outStream.Position = 0;
                                    format = await Image.DetectFormatAsync(this.options.Configuration, outStream);
                                }
                                else
                                {
                                    using var image = FormattedImage.Load(this.options.Configuration, inStream);

                                    image.Process(
                                        this.logger,
                                        this.processors,
                                        commands,
                                        this.commandParser,
                                        this.parserCulture);

                                    await this.options.OnBeforeSaveAsync.Invoke(image);

                                    image.Save(outStream);
                                    format = image.Format;
                                }
                            }

                            // Allow for any further optimization of the image.
                            outStream.Position = 0;
                            string contentType = format.DefaultMimeType;
                            string extension = this.formatUtilities.GetExtensionFromContentType(contentType);
                            await this.options.OnProcessedAsync.Invoke(new ImageProcessingContext(context, outStream, commands, contentType, extension));
                            outStream.Position = 0;

                            cachedImageMetadata = new ImageCacheMetadata(
                                sourceImageMetadata.LastWriteTimeUtc,
                                DateTime.UtcNow,
                                contentType,
                                maxAge,
                                outStream.Length);

                            // Save the image to the cache and send the response to the caller.
                            await this.cache.SetAsync(key, outStream, cachedImageMetadata);

                            // Remove any resolver from the cache so we always resolve next request
                            // for the same key.
                            CacheResolverLru.TryRemove(key);

                            // Place the resolver in the lru cache.
                            (IImageCacheResolver ImageCacheResolver, ImageCacheMetadata ImageCacheMetadata) cachedImage = await
                                CacheResolverLru.GetOrAddAsync(
                                    key,
                                    async k =>
                                    {
                                        IImageCacheResolver resolver = await this.cache.GetAsync(k);
                                        ImageCacheMetadata metadata = default;
                                        if (resolver != null)
                                        {
                                            metadata = await resolver.GetMetaDataAsync();
                                        }

                                        return (resolver, metadata);
                                    });

                            return new ImageWorkerResult(cachedImage.ImageCacheMetadata, cachedImage.ImageCacheResolver);
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
                            await this.StreamDisposeAsync(outStream);
                        }
                    });

                await this.SendResponseAsync(imageContext, key, writeResult.CacheImageMetadata, writeResult.Resolver);
            }
            finally
            {
                // As soon as we have sent a response from a writer the result is available from a reader so we remove this task.
                // Any existing awaiters will continue to await.
                WriteWorkers.TryRemove(key, out Task<ImageWorkerResult> _);
            }
        }

        private ValueTask StreamDisposeAsync(Stream stream)
        {
            if (stream is null)
            {
                return default;
            }

#if NETCOREAPP2_1
            try
            {
                stream.Dispose();
                return default;
            }
            catch (Exception ex)
            {
                return new ValueTask(Task.FromException(ex));
            }
#else
            return stream.DisposeAsync();
#endif
        }

        private async Task<ImageWorkerResult> IsNewOrUpdatedAsync(
            IImageResolver sourceImageResolver,
            ImageContext imageContext,
            string key)
        {
            // Pause until the write has been completed.
            if (WriteWorkers.TryGetValue(key, out Task<ImageWorkerResult> writeWorkResult))
            {
                await writeWorkResult;
            }

            if (ReadWorkers.TryGetValue(key, out Task<ImageWorkerResult> readWorkResult))
            {
                return await readWorkResult;
            }

            return await ReadWorkers.GetOrAdd(
                key,
                async (key) =>
                    {
                        // Get the source metadata for processing, storing the result for future checks.
                        ImageMetadata sourceImageMetadata = await
                            SourceMetadataLru.GetOrAddAsync(
                                key,
                                _ => sourceImageResolver.GetMetaDataAsync());

                        // Check to see if the cache contains this image.
                        // If not, we return early. No further checks necessary.
                        (IImageCacheResolver ImageCacheResolver, ImageCacheMetadata ImageCacheMetadata) cachedImage = await
                            CacheResolverLru.GetOrAddAsync(
                                key,
                                async k =>
                                {
                                    IImageCacheResolver resolver = await this.cache.GetAsync(k);
                                    ImageCacheMetadata metadata = default;
                                    if (resolver != null)
                                    {
                                        metadata = await resolver.GetMetaDataAsync();
                                    }

                                    return (resolver, metadata);
                                });

                        if (cachedImage.ImageCacheResolver is null)
                        {
                            // Remove the null resolver from the store.
                            CacheResolverLru.TryRemove(key);

                            return new ImageWorkerResult(sourceImageMetadata);
                        }

                        // Has the cached image expired?
                        // Or has the source image changed since the image was last cached?
                        if (cachedImage.ImageCacheMetadata.ContentLength == 0 // Fix for old cache without length property
                            || cachedImage.ImageCacheMetadata.CacheLastWriteTimeUtc <= (DateTimeOffset.UtcNow - this.options.CacheMaxAge)
                            || cachedImage.ImageCacheMetadata.SourceLastWriteTimeUtc != sourceImageMetadata.LastWriteTimeUtc)
                        {
                            // We want to remove the resolver from the store so that the next check gets the updated file.
                            CacheResolverLru.TryRemove(key);
                            return new ImageWorkerResult(sourceImageMetadata);
                        }

                        // The image is cached. Return the cached image so multiple callers can write a response.
                        return new ImageWorkerResult(sourceImageMetadata, cachedImage.ImageCacheMetadata, cachedImage.ImageCacheResolver);
                    });
        }

        private async Task SendResponseAsync(
            ImageContext imageContext,
            string key,
            ImageCacheMetadata metadata,
            IImageCacheResolver cacheResolver)
        {
            imageContext.ComprehendRequestHeaders(metadata.CacheLastWriteTimeUtc, metadata.ContentLength);

            switch (imageContext.GetPreconditionState())
            {
                case ImageContext.PreconditionState.Unspecified:
                case ImageContext.PreconditionState.ShouldProcess:
                    if (imageContext.IsHeadRequest())
                    {
                        await imageContext.SendStatusAsync(ResponseConstants.Status200Ok, metadata);
                        return;
                    }

                    this.logger.LogImageServed(imageContext.GetDisplayUrl(), key);

                    // When stream is null we're sending from the cache.
                    using (var stream = await cacheResolver.OpenReadAsync())
                    {
                        await imageContext.SendAsync(stream, metadata);
                    }
                    return;

                case ImageContext.PreconditionState.NotModified:
                    this.logger.LogImageNotModified(imageContext.GetDisplayUrl());
                    await imageContext.SendStatusAsync(ResponseConstants.Status304NotModified, metadata);
                    return;

                case ImageContext.PreconditionState.PreconditionFailed:
                    this.logger.LogImagePreconditionFailed(imageContext.GetDisplayUrl());
                    await imageContext.SendStatusAsync(ResponseConstants.Status412PreconditionFailed, metadata);
                    return;
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
