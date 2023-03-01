// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Represents the metadata associated with an image file.
/// </summary>
public readonly struct ImageMetadata : IEquatable<ImageMetadata>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageMetadata"/> struct.
    /// </summary>
    /// <param name="lastWriteTimeUtc">
    /// The date and time in coordinated universal time (UTC) since the source file was last modified.
    /// </param>
    /// <param name="contentLength">The length of the image in bytes.</param>
    public ImageMetadata(DateTime lastWriteTimeUtc, long contentLength)
        : this(lastWriteTimeUtc, TimeSpan.MinValue, contentLength)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageMetadata"/> struct.
    /// </summary>
    /// <param name="lastWriteTimeUtc">
    /// The date and time in coordinated universal time (UTC) since the source file was last modified.
    /// </param>
    /// <param name="cacheControlMaxAge">
    /// The maximum amount of time a resource will be considered fresh.
    /// </param>
    /// <param name="contentLength">The length of the image in bytes.</param>
    public ImageMetadata(
        DateTime lastWriteTimeUtc,
        TimeSpan cacheControlMaxAge,
        long contentLength)
    {
        this.LastWriteTimeUtc = lastWriteTimeUtc;
        this.CacheControlMaxAge = cacheControlMaxAge;
        this.ContentLength = contentLength;
    }

    /// <summary>
    /// Gets the date and time in coordinated universal time (UTC) since the source file was last modified.
    /// </summary>
    public DateTime LastWriteTimeUtc { get; }

    /// <summary>
    /// Gets the maximum amount of time a resource will be considered fresh.
    /// </summary>
    public TimeSpan CacheControlMaxAge { get; }

    /// <summary>
    /// Gets the length of the image in bytes.
    /// </summary>
    public long ContentLength { get; }

    /// <summary>
    /// Compares two <see cref="ImageMetadata"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="ImageMetadata"/> on the left side of the operand.</param>
    /// <param name="right">The <see cref="ImageMetadata"/> on the right side of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in ImageMetadata left, in ImageMetadata right)
        => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="ImageMetadata"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="ImageMetadata"/> on the left side of the operand.</param>
    /// <param name="right">The <see cref="ImageMetadata"/> on the right side of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in ImageMetadata left, in ImageMetadata right)
        => !left.Equals(right);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ImageMetadata other)
        => this.LastWriteTimeUtc == other.LastWriteTimeUtc
        && this.CacheControlMaxAge == other.CacheControlMaxAge
        && this.ContentLength == other.ContentLength;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is ImageMetadata data && this.Equals(data);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(this.LastWriteTimeUtc, this.CacheControlMaxAge, this.ContentLength);

    /// <inheritdoc/>
    public override string ToString()
        => FormattableString
        .Invariant($"ImageMetaData({this.LastWriteTimeUtc}, {this.CacheControlMaxAge}, {this.ContentLength})");
}
