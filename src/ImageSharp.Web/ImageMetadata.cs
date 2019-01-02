// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// A collection of metadata properties associated with an image file.
    /// </summary>
    public struct ImageMetadata
    {
        private const string ContentTypeKey = "Content-Type";
        private const string LastModifiedKey = "Last-Modified";

        /// <summary>
        /// The date and time in coordinated universal time (UTC) since this image was last modified.
        /// </summary>
        public DateTimeOffset LastModified;

        /// <summary>
        /// The content type of this image.
        /// </summary>
        public string ContentType;

        /// <summary>
        /// Asynchronously deserializes an <see cref="ImageMetadata"/> from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/>.</param>
        /// <returns>The <see cref="ImageMetadata"/>.</returns>
        public static async Task<ImageMetadata> LoadAsync(Stream stream)
        {
            // Parse the key/value pairs from the stream
            var keyValuePairs = new Dictionary<string, string>();
            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    int idx = line.IndexOf(':');
                    if (idx > 0)
                    {
                        string key = line.Substring(0, idx).Trim();
                        string value = line.Substring(idx + 1).Trim();
                        keyValuePairs[key] = value;
                    }
                }
            }

            // Extract the fields we are interested in
            keyValuePairs.TryGetValue(ContentTypeKey, out string contentType);
            keyValuePairs.TryGetValue(LastModifiedKey, out string lastModifiedString);
            DateTimeOffset.TryParse(lastModifiedString, out DateTimeOffset lastModified);

            return new ImageMetadata()
            {
                ContentType = contentType,
                LastModified = lastModified
            };
        }

        /// <summary>
        /// Asynchronously serializes this ImageMetadata to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/>.</param>
        /// <returns>A task.</returns>
        public async Task WriteAsync(Stream stream)
        {
            // Create a collection of key/value pairs from the metadata
            var keyValuePairs = new Dictionary<string, string>
            {
                { ContentTypeKey, this.ContentType },
                { LastModifiedKey, this.LastModified.ToString("o") }
            };

            // Write the key/value pairs to the stream
            using (var writer = new StreamWriter(stream, System.Text.Encoding.UTF8))
            {
                foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
                {
                    await writer.WriteLineAsync($"{keyValuePair.Key}: {keyValuePair.Value}");
                }

                await writer.FlushAsync();
            }
        }
    }
}
