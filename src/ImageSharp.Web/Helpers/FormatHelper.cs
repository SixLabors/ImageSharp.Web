// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web.Helpers
{
    /// <summary>
    /// Contains various helper methods based on the given configuration.
    /// </summary>
    public sealed class FormatHelper
    {
        private readonly IImageFormat[] imageFormats;
        private readonly Dictionary<IImageFormat, string[]> fileExtensions = new Dictionary<IImageFormat, string[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatHelper"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public FormatHelper(Configuration configuration)
        {
            // The formats contained in the configuraiton are used a lot in hash generation
            // so we need them to be enumerated to remove allocations and allow indexing.
            this.imageFormats = configuration.ImageFormats.ToArray();
            for (int i = 0; i < this.imageFormats.Length; i++)
            {
                this.fileExtensions[this.imageFormats[i]] = this.imageFormats[i].FileExtensions.ToArray();
            }
        }

        /// <summary>
        /// Returns the correct content type (mime-type) for the given cached image key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetContentType(string key)
        {
            string extension = Path.GetExtension(key).Replace(".", string.Empty);
            return this.GetContentTypeImpl(extension);
        }

        /// <summary>
        /// Gets the file extension for the given image uri.
        /// </summary>
        /// <param name="uri">The full request uri.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetExtension(string uri)
        {
            string[] parts = uri.Split('?');
            if (parts.Length > 1 && QueryHelpers.ParseQuery(parts[1]).TryGetValue(FormatWebProcessor.Format, out StringValues ext))
            {
                return ext;
            }

            string path = parts[0];
            string extension = null;
            int index = 0;
            for (int i = 0; i < this.imageFormats.Length; i++)
            {
                for (int j = 0; j < this.fileExtensions[this.imageFormats[i]].Length; j++)
                {
                    int li = path.LastIndexOf($".{this.fileExtensions[this.imageFormats[i]][j]}", StringComparison.OrdinalIgnoreCase);
                    if (li < index)
                    {
                        continue;
                    }

                    index = li;
                    extension = this.fileExtensions[this.imageFormats[i]][j];
                }
            }

            return extension;
        }

        /// <summary>
        /// Gets the file extension for the given image uri or a default extension from the first available format.
        /// </summary>
        /// <param name="uri">The full request uri.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetExtensionOrDefault(string uri) => this.GetExtension(uri) ?? this.fileExtensions[this.imageFormats[0]][0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetContentTypeImpl(string extension)
        {
            for (int i = 0; i < this.imageFormats.Length; i++)
            {
                IImageFormat format = this.imageFormats[i];
                for (int j = 0; j < this.fileExtensions[format].Length; j++)
                {
                    if (this.fileExtensions[format][j].Equals(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        return format.DefaultMimeType;
                    }
                }
            }

            return null;
        }
    }
}