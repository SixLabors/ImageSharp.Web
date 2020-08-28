// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        /// The minimum allowable last write time for source files.
        /// Used to determine whether a file has expired its cache duration.
        /// </summary>
        private readonly DateTimeOffset minCacheLastWriteTimeUtc;

        /// <summary>
        /// The maximum time to store the response in a browser cache.
        /// </summary>
        private readonly TimeSpan maxBrowserCacheDuration;

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
            Guard.NotNull(requestParser, nameof(requestParser));
            Guard.NotNull(resolvers, nameof(resolvers));
            Guard.NotNull(processors, nameof(processors));
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(cacheHash, nameof(cacheHash));
            Guard.NotNull(AsyncLock, nameof(AsyncLock));

            this.next = next;
            this.options = options.Value;
            this.requestParser = requestParser;
            this.providers = resolvers as IImageProvider[] ?? resolvers.ToArray();
            this.processors = processors as IImageWebProcessor[] ?? processors.ToArray();
            this.cache = cache;
            this.cacheHash = cacheHash;
            this.minCacheLastWriteTimeUtc = this.GetMaxCacheDateTimeOffset();
            this.maxBrowserCacheDuration = this.GetMaxBrowserCacheDuration();

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
            (bool newOrUpdated, ImageMetadata sourceImageMetadata) =
                await this.IsNewOrUpdatedAsync(sourceImageResolver, imageContext, key);

            if (!newOrUpdated)
            {
                return;
            }

            // Not cached? Let's get it from the image resolver.
            RecyclableMemoryStream outStream = null;
            try
            {
                // Enter a write lock which locks writing and any reads for the same request.
                // This reduces the overheads of unnecessary processing plus avoids file locks.
                using (await AsyncLock.WriterLockAsync(key))
                {
                    ImageCacheMetadata cachedImageMetadata = default;
                    outStream = new RecyclableMemoryStream(this.options.MemoryStreamManager);
                    using (Stream inStream = await sourceImageResolver.OpenReadAsync())
                    {
                        IImageFormat format;

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
                            image.Process(this.logger, this.processors, commands);
                            this.options.OnBeforeSave?.Invoke(image);
                            image.Save(outStream);
                            format = image.Format;
                        }

                        // 14.9.3 CacheControl Max-Age
                        // Check to see if the source metadata has a CacheControl Max-Age value
                        // and use it to override the default max age from our options.
                        TimeSpan maxAge = this.maxBrowserCacheDuration;
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

                    // Allow for any further optimization of the image.
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

        private async Task<ValueTuple<bool, ImageMetadata>> IsNewOrUpdatedAsync(
            IImageResolver sourceImageResolver,
            ImageContext imageContext,
            string key)
        {
            ImageMetadata sourceImageMetadata = default;
            using (await AsyncLock.ReaderLockAsync(key))
            {
                // Check to see if the cache contains this image.
                sourceImageMetadata = await sourceImageResolver.GetMetaDataAsync();
                IImageCacheResolver cachedImageResolver = await this.cache.GetAsync(key);
                if (cachedImageResolver != null)
                {
                    ImageCacheMetadata cachedImageMetadata = await cachedImageResolver.GetMetaDataAsync();
                    if (cachedImageMetadata != default)
                    {
                        // Has the cached image expired or has the source image been updated?
                        if (cachedImageMetadata.SourceLastWriteTimeUtc == sourceImageMetadata.LastWriteTimeUtc
                            && cachedImageMetadata.CacheLastWriteTimeUtc > this.minCacheLastWriteTimeUtc)
                        {
                            // We're pulling the image from the cache.
                            using Stream cachedBuffer = await cachedImageResolver.OpenReadAsync();
                            await this.SendResponseAsync(imageContext, key, cachedBuffer, cachedImageMetadata);

                            return (false, sourceImageMetadata);
                        }
                    }
                }
            }

            return (true, sourceImageMetadata);
        }

        private async Task SendResponseAsync(
            ImageContext imageContext,
            string key,
            Stream stream,
            ImageCacheMetadata metadata)
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

        private DateTimeOffset GetMaxCacheDateTimeOffset()
        {
            return DateTimeOffset.UtcNow
                .AddDays(-this.options.MaxCacheDays)
                .AddMinutes(-this.options.MaxCacheMinutes)
                .AddSeconds(-this.options.MaxCacheSeconds);
        }

        private TimeSpan GetMaxBrowserCacheDuration()
        {
            return TimeSpan.FromDays(this.options.MaxBrowserCacheDays)
                .Add(TimeSpan.FromMinutes(this.options.MaxBrowserCacheMinutes))
                .Add(TimeSpan.FromSeconds(this.options.MaxBrowserCacheSeconds));
        }
    }
}
