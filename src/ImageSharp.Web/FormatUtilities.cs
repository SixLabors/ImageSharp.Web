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
        private readonly List<string> fileExtensions = new List<string>();
        private readonly Dictionary<string, string> fileExtension = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatUtilities"/> class.
        /// </summary>
        /// <param name="options">The middleware options.</param>
        public FormatUtilities(IOptions<ImageSharpMiddlewareOptions> options)
        {
            Guard.NotNull(options, nameof(options));

            // The formats contained in the configuration are used a lot in hash generation
            // so we need them to be enumerated to remove allocations and allow indexing.
            IImageFormat[] imageFormats = options.Value.Configuration.ImageFormats.ToArray();

            for (int i = 0; i < imageFormats.Length; i++)
            {
                string[] extensions = imageFormats[i].FileExtensions.ToArray();

                foreach (string extension in extensions)
                {
                    this.fileExtensions.Add(extension);
                }

                this.fileExtension[imageFormats[i].DefaultMimeType] = extensions[0];
            }
        }

        /// <summary>
        /// Gets the file extension for the given image uri.
        /// </summary>
        /// <param name="uri">The full request uri.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetExtensionFromUri(string uri)
        {
            int query = uri.IndexOf('?');
            ReadOnlySpan<char> path;

            if (query > -1)
            {
                if (uri.Contains(FormatWebProcessor.Format, StringComparison.OrdinalIgnoreCase) && QueryHelpers.ParseQuery(uri.Substring(query)).TryGetValue(FormatWebProcessor.Format, out StringValues ext))
                {
                    return ext;
                }

#if !NETCOREAPP3_1_OR_GREATER
                path = uri.ToLowerInvariant().AsSpan(0, query);
#else
                path = uri.AsSpan(0, query);
#endif
            }
            else
            {
#if !NETCOREAPP3_1_OR_GREATER
                path = uri.ToLowerInvariant();
#else
                path = uri;
#endif
            }

            int extensionIndex;
            if ((extensionIndex = path.LastIndexOf('.')) != -1)
            {
                ReadOnlySpan<char> extension = path.Slice(extensionIndex + 1);

                foreach (string ext in this.fileExtensions)
                {
                    if (extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        return ext;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the correct extension for the given content type (mime-type).
        /// </summary>
        /// <param name="contentType">The content type (mime-type).</param>
        /// <returns>The <see cref="string"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetExtensionFromContentType(string contentType) => this.fileExtension[contentType];
    }
}
