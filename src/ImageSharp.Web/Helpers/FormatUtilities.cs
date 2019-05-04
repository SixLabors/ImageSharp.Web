// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web.Helpers
{
    /// <summary>
    /// Contains various helper methods based on the given configuration.
    /// </summary>
    public sealed class FormatUtilities
    {
        private readonly IImageFormat[] imageFormats;
        private readonly Dictionary<IImageFormat, string[]> fileExtensions = new Dictionary<IImageFormat, string[]>();
        private readonly Dictionary<string, string> fileExtension = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatUtilities"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public FormatUtilities(Configuration configuration)
        {
            // The formats contained in the configuration are used a lot in hash generation
            // so we need them to be enumerated to remove allocations and allow indexing.
            this.imageFormats = configuration.ImageFormats.ToArray();
            for (int i = 0; i < this.imageFormats.Length; i++)
            {
                string[] extensions = this.imageFormats[i].FileExtensions.ToArray();
                this.fileExtensions[this.imageFormats[i]] = extensions;
                this.fileExtension[this.imageFormats[i].DefaultMimeType] = extensions[0];
            }
        }

        /// <summary>
        /// Gets the file extension for the given image uri.
        /// </summary>
        /// <param name="uri">The full request uri.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetExtensionFromUri(string uri)
        {
            // TODO: Investigate using span to reduce allocations here.
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
        /// Gets the correct extension for the given content type (mime-type).
        /// </summary>
        /// <param name="contentType">The content type (mime-type).</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetExtensionFromContentType(string contentType) => this.fileExtension[contentType];
    }
}