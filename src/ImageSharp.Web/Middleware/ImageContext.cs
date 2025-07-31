// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Net.Http.Headers;

namespace SixLabors.ImageSharp.Web.Middleware;

/// <summary>
/// Provides information and methods regarding the current image request.
/// </summary>
internal struct ImageContext
{
    private readonly ImageSharpMiddlewareOptions options;
    private readonly HttpContext context;
    private readonly HttpRequest request;
    private readonly HttpResponse response;
    private readonly RequestHeaders requestHeaders;
    private readonly ResponseHeaders responseHeaders;

    private DateTimeOffset fileLastModified;
    private long fileLength;
    private EntityTagHeaderValue? fileEtag;

    private PreconditionState ifMatchState;
    private PreconditionState ifNoneMatchState;
    private PreconditionState ifModifiedSinceState;
    private PreconditionState ifUnmodifiedSinceState;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageContext"/> struct.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <param name="options">The middleware options.</param>
    public ImageContext(HttpContext context, ImageSharpMiddlewareOptions options)
    {
        this.context = context;
        this.request = context.Request;
        this.response = context.Response;

        this.requestHeaders = context.Request.GetTypedHeaders();
        this.responseHeaders = context.Response.GetTypedHeaders();

        this.options = options;

        this.fileLastModified = DateTimeOffset.MinValue;
        this.fileLength = 0;
        this.fileEtag = null;

        this.ifMatchState = PreconditionState.Unspecified;
        this.ifNoneMatchState = PreconditionState.Unspecified;
        this.ifModifiedSinceState = PreconditionState.Unspecified;
        this.ifUnmodifiedSinceState = PreconditionState.Unspecified;
    }

    /// <summary>
    /// Enumerates the possible precondition states.
    /// </summary>
    internal enum PreconditionState
    {
        /// <summary>
        /// Unspecified
        /// </summary>
        Unspecified,

        /// <summary>
        /// Not modified
        /// </summary>
        NotModified,

        /// <summary>
        /// Should process
        /// </summary>
        ShouldProcess,

        /// <summary>
        /// Precondition Failed
        /// </summary>
        PreconditionFailed,
    }

    /// <summary>
    /// Returns the current HTTP image request display url.
    /// </summary>
    /// <returns>
    /// The combined components of the image request URL in a fully un-escaped form (except
    /// for the QueryString) suitable only for display.
    /// </returns>
    public readonly string GetDisplayUrl() => this.request.GetDisplayUrl();

    /// <summary>
    /// Analyzes the headers for the current request.
    /// </summary>
    /// <param name="lastModified">The point in time when the cached file was last modified.</param>
    /// <param name="length">The length of the cached file in bytes.</param>
    public void ComprehendRequestHeaders(DateTimeOffset lastModified, long length)
    {
        this.fileLastModified = lastModified;
        this.fileLength = length;
        this.ComputeLastModified();

        this.ComputeIfMatch();

        this.ComputeIfModifiedSince();
    }

    /// <summary>
    /// Gets the preconditioned state of the request.
    /// </summary>
    /// <returns>The <see cref="PreconditionState"/>.</returns>
    public readonly PreconditionState GetPreconditionState()
        => GetMaxPreconditionState(
            this.ifMatchState,
            this.ifNoneMatchState,
            this.ifModifiedSinceState,
            this.ifUnmodifiedSinceState);

    /// <summary>
    /// Gets a value indicating whether this request is a head request.
    /// </summary>
    /// <returns>THe <see cref="bool"/>.</returns>
    public readonly bool IsHeadRequest()
        => string.Equals("HEAD", this.request.Method, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Set the response status headers.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <param name="metaData">The image metadata.</param>
    /// <returns>The <see cref="Task"/>.</returns>
    public readonly Task SendStatusAsync(int statusCode, in ImageCacheMetadata metaData)
        => this.ApplyResponseHeadersAsync(
            statusCode,
            metaData.ContentType,
            metaData.CacheControlMaxAge);

    /// <summary>
    /// Set the response content.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="metaData">The image metadata.</param>
    /// <returns>The <see cref="Task"/>.</returns>
    public readonly async Task SendAsync(Stream stream, ImageCacheMetadata metaData)
    {
        await this.ApplyResponseHeadersAsync(
            ResponseConstants.Status200Ok,
            metaData.ContentType,
            metaData.CacheControlMaxAge);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        // We don't need to directly cancel this, if the client disconnects it will fail silently.
        await stream.CopyToAsync(this.response.Body);
        if (this.response.Body.CanSeek)
        {
            this.response.Body.Position = 0;
        }
    }

    private static PreconditionState GetMaxPreconditionState(params PreconditionState[] states)
    {
        PreconditionState max = PreconditionState.Unspecified;
        foreach (PreconditionState state in states)
        {
            if (state > max)
            {
                max = state;
            }
        }

        return max;
    }

    private readonly async Task ApplyResponseHeadersAsync(
        int statusCode,
        string contentType,
        TimeSpan maxAge)
    {
        this.response.StatusCode = statusCode;

        if (statusCode < 400)
        {
            // These headers are returned for 200 and 304
            // They are not returned for 412
            if (!string.IsNullOrEmpty(contentType))
            {
                this.response.ContentType = contentType;
            }

            this.responseHeaders.LastModified = this.fileLastModified;
            this.responseHeaders.ETag = this.fileEtag;
            this.responseHeaders.Headers[HeaderNames.AcceptRanges] = "bytes";

            this.responseHeaders.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = maxAge,
                MustRevalidate = true
            };

            await this.options.OnPrepareResponseAsync.Invoke(this.context);
        }

        if (statusCode == ResponseConstants.Status200Ok)
        {
            // This header is only returned here for 200. It is not returned for 304, and 412
            this.response.ContentLength = this.fileLength;
        }
    }

    private void ComputeLastModified()
    {
        // Truncate to the second.
        this.fileLastModified = new DateTimeOffset(
            this.fileLastModified.Year,
            this.fileLastModified.Month,
            this.fileLastModified.Day,
            this.fileLastModified.Hour,
            this.fileLastModified.Minute,
            this.fileLastModified.Second,
            this.fileLastModified.Offset)
            .ToUniversalTime();

        long etagHash = this.fileLastModified.ToFileTime() ^ this.fileLength;
        this.fileEtag = new EntityTagHeaderValue($"{'\"'}{Convert.ToString(etagHash, 16)}{'\"'}");
    }

    private void ComputeIfMatch()
    {
        // 14.24 If-Match
        IList<EntityTagHeaderValue> ifMatch = this.requestHeaders.IfMatch;

        if (ifMatch?.Count > 0)
        {
            this.ifMatchState = PreconditionState.PreconditionFailed;
            foreach (EntityTagHeaderValue etag in ifMatch)
            {
                if (etag.Equals(EntityTagHeaderValue.Any) || etag.Compare(this.fileEtag, true))
                {
                    this.ifMatchState = PreconditionState.ShouldProcess;
                    break;
                }
            }
        }

        // 14.26 If-None-Match
        IList<EntityTagHeaderValue> ifNoneMatch = this.requestHeaders.IfNoneMatch;

        if (ifNoneMatch?.Count > 0)
        {
            this.ifNoneMatchState = PreconditionState.ShouldProcess;
            foreach (EntityTagHeaderValue etag in ifNoneMatch)
            {
                if (etag.Equals(EntityTagHeaderValue.Any) || etag.Compare(this.fileEtag, true))
                {
                    this.ifNoneMatchState = PreconditionState.NotModified;
                    break;
                }
            }
        }
    }

    private void ComputeIfModifiedSince()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // 14.25 If-Modified-Since
        DateTimeOffset? ifModifiedSince = this.requestHeaders.IfModifiedSince;
        if (ifModifiedSince.HasValue && ifModifiedSince <= now)
        {
            bool modified = ifModifiedSince < this.fileLastModified;
            this.ifModifiedSinceState = modified ? PreconditionState.ShouldProcess : PreconditionState.NotModified;
        }

        // 14.28 If-Unmodified-Since
        DateTimeOffset? ifUnmodifiedSince = this.requestHeaders.IfUnmodifiedSince;
        if (ifUnmodifiedSince.HasValue && ifUnmodifiedSince <= now)
        {
            bool unmodified = ifUnmodifiedSince >= this.fileLastModified;
            this.ifUnmodifiedSinceState = unmodified ? PreconditionState.ShouldProcess : PreconditionState.PreconditionFailed;
        }
    }
}
