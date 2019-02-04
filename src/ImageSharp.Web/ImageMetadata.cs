// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Represents the metadata associated with an image file.
    /// </summary>
    public readonly struct ImageMetaData : IEquatable<ImageMetaData>
    {
        private const string ContentTypeKey = "CT";
        private const string LastModifiedKey = "LM";
        private const string CacheControlKey = "MA";

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageMetaData"/> struct.
        /// </summary>
        /// <param name="lastWriteTimeUtc">The date and time in coordinated universal time (UTC) since the source file was last modified.</param>
        public ImageMetaData(DateTime lastWriteTimeUtc)
            : this(lastWriteTimeUtc, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageMetaData"/> struct.
        /// </summary>
        /// <param name="lastWriteTimeUtc">The date and time in coordinated universal time (UTC) since the source file was last modified.</param>
        /// <param name="contentType">The content type for the source file.</param>
        public ImageMetaData(DateTime lastWriteTimeUtc, string contentType)
        : this(lastWriteTimeUtc, contentType, TimeSpan.MinValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageMetaData"/> struct.
        /// </summary>
        /// <param name="lastWriteTimeUtc">The date and time in coordinated universal time (UTC) since the source file was last modified.</param>
        /// <param name="contentType">The content type for the source file.</param>
        /// <param name="cacheControlMaxAge">The maximum amount of time a resource will be considered fresh.</param>
        public ImageMetaData(DateTime lastWriteTimeUtc, string contentType, TimeSpan cacheControlMaxAge)
        {
            this.LastWriteTimeUtc = lastWriteTimeUtc;
            this.ContentType = contentType;
            this.CacheControlMaxAge = cacheControlMaxAge;
        }

        /// <summary>
        /// Gets the date and time in coordinated universal time (UTC) since the source file was last modified.
        /// </summary>
        public DateTime LastWriteTimeUtc { get; }

        /// <summary>
        /// Gets the content type of the source file.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Gets the maximum amount of time a resource will be considered fresh.
        /// </summary>
        public TimeSpan CacheControlMaxAge { get; }

        /// <summary>
        /// Compares two <see cref="ImageMetaData"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="ImageMetaData"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="ImageMetaData"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in ImageMetaData left, in ImageMetaData right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="ImageMetaData"/> objects for inequality.
        /// </summary>
        /// <param name="left">The <see cref="ImageMetaData"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="ImageMetaData"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in ImageMetaData left, in ImageMetaData right) => !left.Equals(right);

        /// <summary>
        /// Asynchronously reads and returns an <see cref="ImageMetaData"/> from the input stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <returns>The <see cref="ImageMetaData"/>.</returns>
        public static async Task<ImageMetaData> ReadAsync(Stream stream)
        {
            var keyValuePairs = new Dictionary<string, string>();
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    int idx = line.IndexOf(':');
                    if (idx > 0)
                    {
                        string key = line.Substring(0, idx);
                        keyValuePairs[key] = line.Substring(idx + 1);
                    }
                }
            }

            // DateTime.TryParse(null) ==  DateTime.MinValue so no need for conditional;
            keyValuePairs.TryGetValue(LastModifiedKey, out string lastWriteTimeUtcString);
            DateTime.TryParse(lastWriteTimeUtcString, null, DateTimeStyles.RoundtripKind, out DateTime lastWriteTimeUtc);

            keyValuePairs.TryGetValue(ContentTypeKey, out string contentType);

            // int.TryParse(null) == 0 and we want to return TimeSpan.MinValue not TimeSpan.Zero
            TimeSpan cacheControlMaxAge = TimeSpan.MinValue;
            if (keyValuePairs.TryGetValue(CacheControlKey, out string cacheControlMaxAgeString))
            {
                int.TryParse(cacheControlMaxAgeString, out int maxAge);
                cacheControlMaxAge = TimeSpan.FromSeconds(maxAge);
            }

            return new ImageMetaData(lastWriteTimeUtc, contentType ?? string.Empty, cacheControlMaxAge);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ImageMetaData other)
        {
            return this.LastWriteTimeUtc == other.LastWriteTimeUtc
                   && this.ContentType == other.ContentType
                   && this.CacheControlMaxAge == other.CacheControlMaxAge;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is ImageMetaData data && this.Equals(data);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // TODO: Replace with HashCode from Core.
            int hashCode = this.LastWriteTimeUtc.GetHashCode();
            hashCode = (hashCode * 397) ^ this.ContentType.GetHashCode();
            return hashCode = (hashCode * 397) ^ this.CacheControlMaxAge.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString() => FormattableString.Invariant($"ImageMetaData({this.LastWriteTimeUtc}, {this.ContentType}, {this.CacheControlMaxAge})");

        /// <summary>
        /// Asynchronously writes the metadata to the target stream.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task WriteAsync(Stream stream)
        {
            var keyValuePairs = new Dictionary<string, string>
            {
                { LastModifiedKey, this.LastWriteTimeUtc.ToString("o") },
                { ContentTypeKey, this.ContentType },
                { CacheControlKey, this.CacheControlMaxAge.TotalSeconds.ToString(NumberFormatInfo.InvariantInfo) }
            };

            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
                {
                    await writer.WriteLineAsync($"{keyValuePair.Key}:{keyValuePair.Value}").ConfigureAwait(false);
                }

                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}