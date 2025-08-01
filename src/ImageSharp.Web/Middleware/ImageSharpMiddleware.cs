// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Synchronization;

namespace SixLabors.ImageSharp.Web.Middleware;

/// <summary>
/// Middleware for handling the processing of images via image requests.
/// </summary>
public class ImageSharpMiddleware
{
    /// <summary>
    /// Used to temporarily store source metadata reads to reduce the overhead of cache lookups.
    /// </summary>
    private static readonly ConcurrentTLruCache<string, ImageMetadata> SourceMetadataLru
        = new(1024, TimeSpan.FromSeconds(30));

    /// <summary>
    /// Used to temporarily store cache resolver reads to reduce the overhead of cache lookups.
    /// </summary>
    private static readonly ConcurrentTLruCache<string, (IImageCacheResolver?, ImageCacheMetadata)> CacheResolverLru
        = new(1024, TimeSpan.FromSeconds(30));

    /// <summary>
    /// Used to temporarily store cached HMAC-s to reduce the overhead of HMAC token generation.
    /// </summary>
    private static readonly ConcurrentTLruCache<string, string?> HMACTokenLru
        = new(1024, TimeSpan.FromSeconds(30));

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
    /// The cache key.
    /// </summary>
    private readonly ICacheKey cacheKey;

    /// <summary>
    /// The image cache.
    /// </summary>
    private readonly IImageCache cache;

    /// <summary>
    /// The hashing implementation to use when generating cached file names.
    /// </summary>
    private readonly ICacheHash cacheHash;

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
    /// The lock used to prevent concurrent processing of the same exact image request.
    /// </summary>
    private readonly AsyncKeyReaderWriterLock<string> asyncKeyLock;

    /// <summary>
    /// Contains helpers that allow authorization of image requests.
    /// </summary>
    private readonly RequestAuthorizationUtilities authorizationUtilities;

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
    /// <param name="cacheKey">An <see cref="ICacheKey"/> instance used for creating cache keys.</param>
    /// <param name="cacheHash">An <see cref="ICacheHash"/>instance used for calculating cached file names.</param>
    /// <param name="commandParser">The command parser.</param>
    /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
    /// <param name="asyncKeyLock">The async key lock.</param>
    /// <param name="requestAuthorizationUtilities">Contains helpers that allow authorization of image requests.</param>
    public ImageSharpMiddleware(
        RequestDelegate next,
        IOptions<ImageSharpMiddlewareOptions> options,
        ILoggerFactory loggerFactory,
        IRequestParser requestParser,
        IEnumerable<IImageProvider> resolvers,
        IEnumerable<IImageWebProcessor> processors,
        IImageCache cache,
        ICacheKey cacheKey,
        ICacheHash cacheHash,
        CommandParser commandParser,
        FormatUtilities formatUtilities,
        AsyncKeyReaderWriterLock<string> asyncKeyLock,
        RequestAuthorizationUtilities requestAuthorizationUtilities)
    {
        Guard.NotNull(next, nameof(next));
        Guard.NotNull(options, nameof(options));
        Guard.NotNull(loggerFactory, nameof(loggerFactory));
        Guard.NotNull(requestParser, nameof(requestParser));
        Guard.NotNull(resolvers, nameof(resolvers));
        Guard.NotNull(processors, nameof(processors));
        Guard.NotNull(cache, nameof(cache));
        Guard.NotNull(cacheKey, nameof(cacheKey));
        Guard.NotNull(cacheHash, nameof(cacheHash));
        Guard.NotNull(commandParser, nameof(commandParser));
        Guard.NotNull(formatUtilities, nameof(formatUtilities));
        Guard.NotNull(asyncKeyLock, nameof(asyncKeyLock));
        Guard.NotNull(requestAuthorizationUtilities, nameof(requestAuthorizationUtilities));

        this.next = next;
        this.options = options.Value;
        this.requestParser = requestParser;
        this.providers = resolvers as IImageProvider[] ?? resolvers.ToArray();
        this.processors = processors as IImageWebProcessor[] ?? processors.ToArray();
        this.cache = cache;
        this.cacheKey = cacheKey;
        this.cacheHash = cacheHash;
        this.commandParser = commandParser;
        this.parserCulture = this.options.UseInvariantParsingCulture
            ? CultureInfo.InvariantCulture
            : CultureInfo.CurrentCulture;

        this.logger = loggerFactory.CreateLogger<ImageSharpMiddleware>();
        this.formatUtilities = formatUtilities;
        this.asyncKeyLock = asyncKeyLock;
        this.authorizationUtilities = requestAuthorizationUtilities;
    }

    /// <summary>
    /// Performs operations upon the current request.
    /// </summary>
    /// <param name="httpContext">The current HTTP request context.</param>
    /// <returns>The <see cref="Task"/>.</returns>
    public Task Invoke(HttpContext httpContext) => this.Invoke(httpContext, false);

    private async Task Invoke(HttpContext httpContext, bool retry)
    {
        // Get the correct provider for the request
        IImageProvider? provider = null;
        foreach (IImageProvider resolver in this.providers)
        {
            if (resolver.Match(httpContext))
            {
                provider = resolver;
                break;
            }
        }

        if (provider?.IsValidRequest(httpContext) != true)
        {
            // Nothing to do. call the next delegate/middleware in the pipeline
            await this.next(httpContext);
            return;
        }

        CommandCollection commands = this.requestParser.ParseRequestCommands(httpContext);

        // First check for a HMAC token and capture before the command is stripped out.
        byte[] secret = this.options.HMACSecretKey;
        bool checkHMAC = false;
        string? token = null;
        if (secret?.Length > 0)
        {
            checkHMAC = true;
            token = commands.GetValueOrDefault(RequestAuthorizationUtilities.TokenCommand);
        }

        this.authorizationUtilities.StripUnknownCommands(commands);
        ImageCommandContext imageCommandContext = new(httpContext, commands, this.commandParser, this.parserCulture);

        // At this point we know that this is an image request so should attempt to compute a validating HMAC.
        string? hmac = null;
        if (checkHMAC && token != null)
        {
            // Generate and cache a HMAC to validate against based upon the current valid commands from the request.
            //
            // If the command collection differs following the stripping of invalid commands prior to this point then this will mean
            // the token will not match our validating HMAC, however, this would be indicative of an attack and should be treated as such.
            //
            // As a rule all image requests should contain valid commands only.
            // Key generation uses string.Create under the hood with very low allocation so should be good enough as a cache key.
            hmac = HMACTokenLru.GetOrAdd(
                httpContext.Request.GetEncodedUrl(),
                _ => this.authorizationUtilities.ComputeHMAC(imageCommandContext));
        }

        await this.options.OnParseCommandsAsync.Invoke(imageCommandContext);

        if (commands.Count == 0 && provider?.ProcessingBehavior != ProcessingBehavior.All)
        {
            // Nothing to do. call the next delegate/middleware in the pipeline
            await this.next(httpContext);
            return;
        }

        // At this point we know that this is an image request designed for processing via this middleware.
        // Check for a token if required and reject if invalid.
        if (checkHMAC && (hmac != token || (hmac is null && commands.Count > 0)))
        {
            SetBadRequest(httpContext);
            return;
        }

        IImageResolver? sourceImageResolver = await provider.GetAsync(httpContext);

        if (sourceImageResolver is null)
        {
            // Log the error but let the pipeline handle the 404
            // by calling the next delegate/middleware in the pipeline.
            this.logger.LogImageResolveFailed(httpContext.Request.GetDisplayUrl());
            await this.next(httpContext);
            return;
        }

        await this.ProcessRequestAsync(
            imageCommandContext,
            sourceImageResolver,
            new ImageContext(httpContext, this.options),
            retry);
    }

    private static void SetBadRequest(HttpContext httpContext)
    {
        // We return a 400 rather than a 401 as we do not want to prompt follow up requests.
        // We don't log the error to avoid attempts at log poisoning.
        httpContext.Response.Clear();
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    }

    private async Task ProcessRequestAsync(
        ImageCommandContext imageCommandContext,
        IImageResolver sourceImageResolver,
        ImageContext imageContext,
        bool retry)
    {
        HttpContext httpContext = imageCommandContext.Context;
        CommandCollection commands = imageCommandContext.Commands;

        // Create a hashed cache key
        string key = this.cacheHash.Create(
            this.cacheKey.Create(httpContext, commands),
            this.options.CacheHashLength);

        // Check the cache, if present, not out of date and not requiring an update
        // we'll simply serve the file from there.
        ImageWorkerResult readResult = default;
        using (await this.asyncKeyLock.ReaderLockAsync(key))
        {
            readResult = await this.IsNewOrUpdatedAsync(sourceImageResolver, key);
        }

        if (!readResult.IsNewOrUpdated)
        {
            await this.SendResponseAsync(httpContext, imageContext, key, readResult.CacheImageMetadata, readResult.Resolver, null, retry);
            return;
        }

        // Not cached, or is updated? Let's get it from the image resolver.
        ImageMetadata sourceImageMetadata = readResult.SourceImageMetadata;

        // Enter an asynchronous write worker which prevents multiple writes and delays any reads for the same request.
        // This reduces the overheads of unnecessary processing.
        RecyclableMemoryStream? outStream = null;
        try
        {
            Task<IDisposable> takeLockTask = this.asyncKeyLock.WriterLockAsync(key);
            bool lockWasAlreadyHeld = takeLockTask.Status != TaskStatus.RanToCompletion;
            using (await takeLockTask)
            {
                // If the lock was already held, then that means there's a chance another worker has already
                // processed this same request and the value may now be available in the cache, so check
                // the cache one more time
                if (lockWasAlreadyHeld)
                {
                    readResult = await this.IsNewOrUpdatedAsync(sourceImageResolver, key);
                }

                if (readResult.IsNewOrUpdated)
                {
                    try
                    {
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

                        await using (Stream inStream = await sourceImageResolver.OpenReadAsync())
                        {
                            // TODO: Do we need some way to set options based upon processors?
                            DecoderOptions decoderOptions = await this.options.OnBeforeLoadAsync.Invoke(imageCommandContext, this.options.Configuration)
                                ?? new() { Configuration = this.options.Configuration };

                            FormattedImage? image = null;
                            try
                            {
                                // Now we can finally process the image.
                                // We first sort the processor collection by command order then use that collection to determine whether the decoded image pixel format
                                // explicitly requires an alpha component in order to allow correct processing.
                                //
                                // The non-generic variant will decode to the correct pixel format based upon the encoded image metadata which can yield
                                // massive memory savings.
                                IReadOnlyList<(int Index, IImageWebProcessor Processor)> sortedProcessors = this.processors.OrderBySupportedCommands(commands);
                                bool requiresAlpha = sortedProcessors.RequiresTrueColorPixelFormat(commands, this.commandParser, this.parserCulture);

                                if (requiresAlpha)
                                {
                                    image = await FormattedImage.LoadAsync<Rgba32>(decoderOptions, inStream);
                                }
                                else
                                {
                                    image = await FormattedImage.LoadAsync(decoderOptions, inStream);
                                }

                                image.Process(
                                    this.logger,
                                    sortedProcessors,
                                    commands,
                                    this.commandParser,
                                    this.parserCulture);

                                await this.options.OnBeforeSaveAsync.Invoke(image);

                                image.Save(outStream);
                                format = image.Format;
                            }
                            finally
                            {
                                image?.Dispose();
                            }
                        }

                        // Allow for any further optimization of the image.
                        outStream.Position = 0;
                        string contentType = format.DefaultMimeType;
                        string extension = this.formatUtilities.GetExtensionFromContentType(contentType);
                        await this.options.OnProcessedAsync.Invoke(new ImageProcessingContext(httpContext, outStream, commands, contentType, extension));
                        outStream.Position = 0;

                        cachedImageMetadata = new ImageCacheMetadata(
                            sourceImageMetadata.LastWriteTimeUtc,
                            DateTime.UtcNow,
                            contentType,
                            maxAge,
                            outStream.Length);

                        // Save the image to the cache and send the response to the caller.
                        await this.cache.SetAsync(key, outStream, cachedImageMetadata);
                        outStream.Position = 0;

                        // Remove any resolver from the cache so we always resolve next request
                        // for the same key.
                        CacheResolverLru.TryRemove(key);

                        readResult = new ImageWorkerResult(cachedImageMetadata, null);
                    }
                    catch (Exception ex)
                    {
                        // Log the error internally then rethrow.
                        // We don't call next here, the pipeline will automatically handle it
                        this.logger.LogImageProcessingFailed(imageContext.GetDisplayUrl(), ex);
                        throw;
                    }
                }
            }

            await this.SendResponseAsync(httpContext, imageContext, key, readResult.CacheImageMetadata, readResult.Resolver, outStream, retry);
        }
        finally
        {
            await StreamDisposeAsync(outStream);
        }
    }

    private static ValueTask StreamDisposeAsync(Stream? stream)
    {
        if (stream is null)
        {
            return default;
        }

        return stream.DisposeAsync();
    }

    private async Task<ImageWorkerResult> IsNewOrUpdatedAsync(
        IImageResolver sourceImageResolver,
        string key)
    {
        // Get the source metadata for processing, storing the result for future checks.
        ImageMetadata sourceImageMetadata = await
            SourceMetadataLru.GetOrAddAsync(
                key,
                _ => sourceImageResolver.GetMetaDataAsync());

        // Check to see if the cache contains this image.
        // If not, we return early. No further checks necessary.
        (IImageCacheResolver? ImageCacheResolver, ImageCacheMetadata ImageCacheMetadata) cachedImage = await
            CacheResolverLru.GetOrAddAsync(
                key,
                async k =>
                {
                    IImageCacheResolver? resolver = await this.cache.GetAsync(k);
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
    }

    private async Task SendResponseAsync(
        HttpContext httpContext,
        ImageContext imageContext,
        string key,
        ImageCacheMetadata metadata,
        IImageCacheResolver? cacheResolver,
        Stream? stream,
        bool retry)
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

                // If stream is not null, then send it directly. Otherwise, use the cacheResolver
                // to load from the cache.
                if (stream != null)
                {
                    await imageContext.SendAsync(stream, metadata);
                }
                else
                {
                    try
                    {
                        Guard.NotNull(cacheResolver);

                        await using Stream cacheStream = await cacheResolver.OpenReadAsync();
                        await imageContext.SendAsync(cacheStream, metadata);
                    }
                    catch (Exception ex)
                    {
                        if (!retry)
                        {
                            // The image has failed to be returned from the cache.
                            // This can happen if the cached image has been physically deleted but the item is still in the LRU cache.
                            // We'll retry running the request again in it's entirety. This ensures any changes to the source are tracked also.
                            CacheResolverLru.TryRemove(key);
                            await this.Invoke(httpContext, true);
                            return;
                        }

                        // We've already tried to run this request before.
                        // Log the error internally then rethrow.
                        // We don't call next here, the pipeline will automatically handle it
                        this.logger.LogImageProcessingFailed(imageContext.GetDisplayUrl(), ex);
                        throw;
                    }
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
                NotImplementedException exception = new(imageContext.GetPreconditionState().ToString());
                Debug.Fail(exception.ToString());
                throw exception;
        }
    }
}
