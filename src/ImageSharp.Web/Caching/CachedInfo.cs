// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Contains information about a cached image instance
    /// </summary>
    public readonly struct CachedInfo : IEquatable<CachedInfo>
    {
        /// <summary>
        /// Gets a value indicating whether the cached image is expired
        /// </summary>
        public readonly bool Expired;

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), the cached image was last written to
        /// </summary>
        public readonly DateTimeOffset LastModifiedUtc;

        /// <summary>
        /// Gets the length, in bytes, of the cached image
        /// </summary>
        public readonly long Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedInfo"/> struct.
        /// </summary>
        /// <param name="expired">Whether the cached image is expired</param>
        /// <param name="lastModified">The date and time, in coordinated universal time (UTC), the cached image was last written to</param>
        /// <param name="length">The length, in bytes, of the cached image</param>
        public CachedInfo(bool expired, DateTimeOffset lastModified, long length)
        {
            this.Expired = expired;
            this.LastModifiedUtc = lastModified;
            this.Length = length;
        }

        /// <inheritdoc/>
        public bool Equals(CachedInfo other)
        {
            return this.Expired == other.Expired && this.LastModifiedUtc.Equals(other.LastModifiedUtc) && this.Length == other.Length;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is CachedInfo info && this.Equals(info);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Expired.GetHashCode();
                hashCode = (hashCode * 397) ^ this.LastModifiedUtc.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Length.GetHashCode();
                return hashCode;
            }
        }
    }
}