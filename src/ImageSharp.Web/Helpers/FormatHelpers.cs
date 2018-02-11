// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace SixLabors.ImageSharp.Web.Helpers
{
    /// <summary>
    /// Helper utilities for image formats
    /// </summary>
    public class FormatHelpers
    {
        /// <summary>
        /// Returns the correct content type (mimetype) for the given cached image key.
        /// </summary>
        /// <param name="configuration">The library configuration</param>
        /// <param name="key">The cache key</param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetContentType(Configuration configuration, string key)
        {
            string extension = Path.GetExtension(key).Replace(".", string.Empty);
            return configuration.ImageFormats.First(f => f.FileExtensions.Contains(extension)).DefaultMimeType;
        }

        /// <summary>
        /// Gets the file extension for the given image uri
        /// </summary>
        /// <param name="configuration">The library configuration</param>
        /// <param name="uri">The full request uri</param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetExtension(Configuration configuration, string uri)
        {
            string[] parts = uri.Split('?');
            if (parts.Length > 1)
            {
                StringValues ext;
                if (QueryHelpers.ParseQuery(parts[1]).TryGetValue("format", out ext))
                {
                    return ext;
                }
            }

            string path = parts[0];
            string extension = null;
            int index = 0;
            foreach (IImageFormat format in configuration.ImageFormats)
            {
                foreach (string ext in format.FileExtensions)
                {
                    int i = path.LastIndexOf("." + ext, StringComparison.OrdinalIgnoreCase);
                    if (i < index)
                    {
                        continue;
                    }

                    index = i;
                    extension = ext;
                }
            }

            if (extension != null && !path.EndsWith(extension))
            {
                return null;
            }

            return extension;
        }

        /// <summary>
        /// Gets the file extension for the given image uri or a default extension from the first available format.
        /// </summary>
        /// <param name="configuration">The library configuration</param>
        /// <param name="uri">The full request uri</param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetExtensionOrDefault(Configuration configuration, string uri)
        {
            return GetExtension(configuration, uri) ?? configuration.ImageFormats.First().FileExtensions.First();
        }
    }
}