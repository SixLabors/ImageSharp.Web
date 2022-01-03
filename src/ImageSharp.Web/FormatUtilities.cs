// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Contains various helper methods based on the given configuration.
    /// </summary>
    public sealed class FormatUtilities
    {
        private readonly List<string> extensions = new();
        private readonly Dictionary<string, string> extensionsByMimeType = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatUtilities" /> class.
        /// </summary>
        /// <param name="options">The middleware options.</param>
        public FormatUtilities(IOptions<ImageSharpMiddlewareOptions> options)
        {
            Guard.NotNull(options, nameof(options));

            foreach (IImageFormat imageFormat in options.Value.Configuration.ImageFormats)
            {
                string[] extensions = imageFormat.FileExtensions.ToArray();

                foreach (string extension in extensions)
                {
                    this.extensions.Add(extension);
                }

                this.extensionsByMimeType[imageFormat.DefaultMimeType] = extensions[0];
            }
        }

        /// <summary>
        /// Gets the file extension for the given image uri.
        /// </summary>
        /// <param name="uri">The full request uri.</param>
        /// <returns>The <see cref="string" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetExtensionFromUri(string uri)
        {
            // TODO: This method should follow TryGet pattern. Fix for V2.
            int query = uri.IndexOf('?');
            ReadOnlySpan<char> path;

            if (query > -1)
            {
                if (uri.Contains(FormatWebProcessor.Format, StringComparison.OrdinalIgnoreCase)
                    && QueryHelpers.ParseQuery(uri.Substring(query)).TryGetValue(FormatWebProcessor.Format, out StringValues ext))
                {
                    // We have a query but is it a valid one?
                    ReadOnlySpan<char> extSpan = ext[0].AsSpan();
                    foreach (string extension in this.extensions)
                    {
                        if (extSpan.Equals(extension, StringComparison.OrdinalIgnoreCase))
                        {
                            return extension;
                        }
                    }

                    return null;
                }

                path = uri.AsSpan(0, query);
            }
            else
            {
                path = uri;
            }

            int extensionIndex;
            if ((extensionIndex = path.LastIndexOf('.')) != -1)
            {
                ReadOnlySpan<char> pathExtension = path.Slice(extensionIndex + 1);

                foreach (string extension in this.extensions)
                {
                    if (pathExtension.Equals(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        return extension;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the correct extension for the given content type (mime-type).
        /// </summary>
        /// <param name="contentType">The content type (mime-type).</param>
        /// <returns>The <see cref="string" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetExtensionFromContentType(string contentType) => this.extensionsByMimeType[contentType];
    }
}
