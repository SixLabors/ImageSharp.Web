// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Optimized helper methods for generating cache keys from URI components. Much of this code has been adapted from the MIT licensed .NET runtime.
    /// </summary>
    internal static class CacheKeyHelper
    {
        private static readonly SpanAction<char, (bool LowerInvariant, string Host, string PathBase, string Path, string Query)> InitializeAbsoluteUriStringSpanAction = new(InitializeAbsoluteUriString);

        public enum CaseHandling
        {
            None,
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
        public static string BuildRelativeKey(
            CaseHandling handling,
            PathString pathBase = default,
            PathString path = default,
            QueryString query = default)

            // Take any potential performance hit vs concatination for code reading sanity.
            => BuildAbsoluteKey(handling, default, pathBase, path, query);

        /// <summary>
        /// Combines the given URI components into a string that is properly encoded for use in HTTP headers.
        /// Note that unicode in the HostString will be encoded as punycode.
        /// </summary>
        /// <param name="handling">Determines case handling for the result. <paramref name="query"/> is always converted to invariant lowercase.</param>
        /// <param name="host">The host portion of the uri normally included in the Host header. This may include the port.</param>
        /// <param name="pathBase">The first portion of the request path associated with application root.</param>
        /// <param name="path">The portion of the request path that identifies the requested resource.</param>
        /// <param name="query">The query, if any.</param>
        /// <returns>The combined URI components, properly encoded for use in HTTP headers.</returns>
        public static string BuildAbsoluteKey(
            CaseHandling handling,
            HostString host,
            PathString pathBase = default,
            PathString path = default,
            QueryString query = default)
        {
            string hostText = host.ToString();
            string pathBaseText = pathBase.ToString();
            string pathText = path.ToString();
            string queryText = query.ToString();

            // PERF: Calculate string length to allocate correct buffer size for string.Create.
            int length =
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

            return string.Create(length, (handling == CaseHandling.LowerInvariant, hostText, pathBaseText, pathText, queryText), InitializeAbsoluteUriStringSpanAction);
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
        /// Initializes the URI <see cref="string"/> for <see cref="BuildAbsoluteKey(CaseHandling, HostString, PathString, PathString, QueryString)"/>.
        /// </summary>
        /// <param name="buffer">The URI <see cref="string"/>'s <see cref="char"/> buffer.</param>
        /// <param name="uriParts">The URI parts.</param>
        private static void InitializeAbsoluteUriString(Span<char> buffer, (bool Lower, string Host, string PathBase, string Path, string Query) uriParts)
        {
            int index = 0;
            ReadOnlySpan<char> pathBaseSpan = uriParts.PathBase.AsSpan();

            if (uriParts.Path.Length > 0 && pathBaseSpan.Length > 0 && pathBaseSpan[pathBaseSpan.Length - 1] == '/')
            {
                // If the path string has a trailing slash and the other string has a leading slash, we need
                // to trim one of them.
                // Trim the last slash from pathBase. The total length was decremented before the call to string.Create.
                pathBaseSpan = pathBaseSpan.Slice(0, pathBaseSpan.Length - 1);
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
}
