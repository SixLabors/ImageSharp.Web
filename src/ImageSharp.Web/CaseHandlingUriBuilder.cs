// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Optimized helper methods for generating encoded Uris from URI components.
/// Much of this code has been adapted from the MIT licensed .NET runtime.
/// </summary>
public static class CaseHandlingUriBuilder
{
    private static readonly Uri FallbackBaseUri = new("http://localhost/");
    private static readonly SpanAction<char, (bool LowerInvariant, string Scheme, string Host, string PathBase, string Path, string Query)> InitializeAbsoluteUriStringSpanAction = new(InitializeAbsoluteUriString);

    /// <summary>
    /// Provides Uri case handling options.
    /// </summary>
    public enum CaseHandling
    {
        /// <summary>
        /// No adjustments to casing are made.
        /// </summary>
        None,

        /// <summary>
        /// All URI components are converted to lower case using the invariant culture before combining.
        /// </summary>
        LowerInvariant
    }

    /// <summary>
    /// Combines the given URI components into a string that is properly encoded for use in HTTP headers.
    /// </summary>
    /// <param name="handling">Determines case handling for the result. <paramref name="query"/> is always converted to invariant lowercase.</param>
    /// <param name="pathBase">The first portion of the request path associated with application root.</param>
    /// <param name="path">The portion of the request path that identifies the requested resource.</param>
    /// <param name="query">The query, if any.</param>
    /// <returns>The combined URI components, properly encoded for use in HTTP headers.</returns>
    public static string BuildRelative(
        CaseHandling handling,
        PathString pathBase = default,
        PathString path = default,
        QueryString query = default)

        // Take any potential performance hit vs concatination for code reading sanity.
        => BuildAbsolute(handling, default, pathBase, path, query);

    /// <summary>
    /// Combines the given URI components into a string that is properly encoded for use in HTTP headers.
    /// Note that unicode in the HostString will be encoded as punycode and the scheme is not included
    /// in the result.
    /// </summary>
    /// <param name="handling">Determines case handling for the result. <paramref name="query"/> is always converted to invariant lowercase.</param>
    /// <param name="host">The host portion of the uri normally included in the Host header. This may include the port.</param>
    /// <param name="pathBase">The first portion of the request path associated with application root.</param>
    /// <param name="path">The portion of the request path that identifies the requested resource.</param>
    /// <param name="query">The query, if any.</param>
    /// <returns>The combined URI components, properly encoded for use in HTTP headers.</returns>
    public static string BuildAbsolute(
        CaseHandling handling,
        HostString host,
        PathString pathBase = default,
        PathString path = default,
        QueryString query = default)
        => BuildAbsolute(handling, string.Empty, host, pathBase, path, query);

    /// <summary>
    /// Combines the given URI components into a string that is properly encoded for use in HTTP headers.
    /// Note that unicode in the HostString will be encoded as punycode.
    /// </summary>
    /// <param name="handling">Determines case handling for the result. <paramref name="query"/> is always converted to invariant lowercase.</param>
    /// <param name="scheme">http, https, etc.</param>
    /// <param name="host">The host portion of the uri normally included in the Host header. This may include the port.</param>
    /// <param name="pathBase">The first portion of the request path associated with application root.</param>
    /// <param name="path">The portion of the request path that identifies the requested resource.</param>
    /// <param name="query">The query, if any.</param>
    /// <returns>The combined URI components, properly encoded for use in HTTP headers.</returns>
    public static string BuildAbsolute(
        CaseHandling handling,
        string scheme,
        HostString host,
        PathString pathBase = default,
        PathString path = default,
        QueryString query = default)
    {
        Guard.NotNull(scheme, nameof(scheme));

        string hostText = host.ToUriComponent();
        string pathBaseText = pathBase.ToUriComponent();
        string pathText = path.ToUriComponent();
        string queryText = query.ToUriComponent();

        // PERF: Calculate string length to allocate correct buffer size for string.Create.
        int length =
            (scheme.Length > 0 ? scheme.Length + Uri.SchemeDelimiter.Length : 0) +
            hostText.Length +
            pathBaseText.Length +
            pathText.Length +
            queryText.Length;

        if (string.IsNullOrEmpty(pathText))
        {
            if (string.IsNullOrEmpty(pathBaseText))
            {
                pathText = "/";
                length++;
            }
        }
        else if (pathBaseText.EndsWith('/'))
        {
            // If the path string has a trailing slash and the other string has a leading slash, we need
            // to trim one of them.
            // Just decrement the total length, for now.
            length--;
        }

        return string.Create(
            length,
            (handling == CaseHandling.LowerInvariant, scheme, hostText, pathBaseText, pathText, queryText),
            InitializeAbsoluteUriStringSpanAction);
    }

    /// <summary>
    /// Generates a string from the given absolute or relative Uri that is appropriately encoded for use in
    /// HTTP headers. Note that a unicode host name will be encoded as punycode.
    /// </summary>
    /// <param name="handling">Determines case handling for the result.</param>
    /// <param name="uri">The Uri to encode.</param>
    /// <returns>The encoded string version of <paramref name="uri"/>.</returns>
    public static string Encode(CaseHandling handling, string uri)
    {
        Guard.NotNull(uri, nameof(uri));
        return Encode(handling, new Uri(uri, UriKind.RelativeOrAbsolute));
    }

    /// <summary>
    /// Generates a string from the given absolute or relative Uri that is appropriately encoded for use in
    /// HTTP headers. Note that a unicode host name will be encoded as punycode.
    /// </summary>
    /// <param name="handling">Determines case handling for the result.</param>
    /// <param name="uri">The Uri to encode.</param>
    /// <returns>The encoded string version of <paramref name="uri"/>.</returns>
    public static string Encode(CaseHandling handling, Uri uri)
    {
        Guard.NotNull(uri, nameof(uri));
        if (uri.IsAbsoluteUri)
        {
            return BuildAbsolute(
                handling,
                scheme: uri.Scheme,
                host: HostString.FromUriComponent(uri),
                pathBase: PathString.FromUriComponent(uri),
                query: QueryString.FromUriComponent(uri));
        }

        Uri faux = new(FallbackBaseUri, uri);
        return BuildRelative(
            handling,
            path: PathString.FromUriComponent(faux),
            query: QueryString.FromUriComponent(faux));
    }

    /// <summary>
    /// Copies the specified <paramref name="text"/> to the specified <paramref name="buffer"/> starting at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="buffer">The buffer to copy text to.</param>
    /// <param name="index">The buffer start index.</param>
    /// <param name="text">The text to copy.</param>
    /// <returns>The <see cref="int"/> representing the combined text length.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CopyTextToBuffer(Span<char> buffer, int index, ReadOnlySpan<char> text)
    {
        text.CopyTo(buffer.Slice(index, text.Length));
        return index + text.Length;
    }

    /// <summary>
    /// Copies the specified <paramref name="text"/> to the specified <paramref name="buffer"/> starting at the specified <paramref name="index"/>
    /// converting each character to lowercase, using the casing rules of the invariant culture.
    /// </summary>
    /// <param name="buffer">The buffer to copy text to.</param>
    /// <param name="index">The buffer start index.</param>
    /// <param name="text">The text to copy.</param>
    /// <returns>The <see cref="int"/> representing the combined text length.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CopyTextToBufferLowerInvariant(Span<char> buffer, int index, ReadOnlySpan<char> text)
        => index + text.ToLowerInvariant(buffer.Slice(index, text.Length));

    /// <summary>
    /// Initializes the URI <see cref="string"/> for <see cref="BuildAbsolute(CaseHandling, HostString, PathString, PathString, QueryString)"/>.
    /// </summary>
    /// <param name="buffer">The URI <see cref="string"/>'s <see cref="char"/> buffer.</param>
    /// <param name="uriParts">The URI parts.</param>
    private static void InitializeAbsoluteUriString(Span<char> buffer, (bool Lower, string Scheme, string Host, string PathBase, string Path, string Query) uriParts)
    {
        int index = 0;
        ReadOnlySpan<char> pathBaseSpan = uriParts.PathBase.AsSpan();

        if (uriParts.Path.Length > 0 && pathBaseSpan.Length > 0 && pathBaseSpan[^1] == '/')
        {
            // If the path string has a trailing slash and the other string has a leading slash, we need
            // to trim one of them.
            // Trim the last slash from pathBase. The total length was decremented before the call to string.Create.
            pathBaseSpan = pathBaseSpan[..^1];
        }

        if (uriParts.Scheme.Length > 0)
        {
            index = CopyTextToBufferLowerInvariant(buffer, index, uriParts.Scheme.AsSpan());
            index = CopyTextToBuffer(buffer, index, Uri.SchemeDelimiter.AsSpan());
        }

        if (uriParts.Lower)
        {
            index = CopyTextToBufferLowerInvariant(buffer, index, uriParts.Host.AsSpan());
            index = CopyTextToBufferLowerInvariant(buffer, index, pathBaseSpan);
            index = CopyTextToBufferLowerInvariant(buffer, index, uriParts.Path.AsSpan());
        }
        else
        {
            index = CopyTextToBuffer(buffer, index, uriParts.Host.AsSpan());
            index = CopyTextToBuffer(buffer, index, pathBaseSpan);
            index = CopyTextToBuffer(buffer, index, uriParts.Path.AsSpan());
        }

        // Querystring is always copied as lower invariant.
        _ = CopyTextToBufferLowerInvariant(buffer, index, uriParts.Query.AsSpan());
    }
}
