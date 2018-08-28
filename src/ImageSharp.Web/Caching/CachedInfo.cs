// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Contains information about a cached image instance.
    /// </summary>
    public readonly struct CachedInfo : IEquatable<CachedInfo>
    {
        /// <summary>
        /// Gets a value indicating whether the cached image is expired.
        /// </summary>
        public readonly bool Expired;

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), the cached image was last written to.
        /// </summary>
        public readonly DateTime LastModifiedUtc;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedInfo"/> struct.
        /// </summary>
        /// <param name="expired">Whether the cached image is expired.</param>
        /// <param name="lastModifiedUtc">The date and time, in coordinated universal time (UTC), the cached image was last written to.</param>
        public CachedInfo(bool expired, DateTime lastModifiedUtc)
        {
            this.Expired = expired;
            this.LastModifiedUtc = lastModifiedUtc;
        }

        /// <inheritdoc/>
        public bool Equals(CachedInfo other) => this.Expired == other.Expired && this.LastModifiedUtc.Equals(other.LastModifiedUtc);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is CachedInfo info && this.Equals(info);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Expired.GetHashCode();
                return hashCode = (hashCode * 397) ^ this.LastModifiedUtc.GetHashCode();
            }
        }
    }
}