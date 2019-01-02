// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SixLabors.Memory;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Represents the metadata associated with an image file.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ImageMetaData : IEquatable<ImageMetaData>
    {
        // Bytes per struct.
        private const int DateSize = 8;
        private const int CharSize = 2;

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
        {
            this.LastWriteTimeUtc = lastWriteTimeUtc;
            this.ContentType = contentType;
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
        /// <param name="memoryAllocator">The memory allocator used for managing buffers.</param>
        /// <returns>The <see cref="ImageMetaData"/>.</returns>
        public static async Task<ImageMetaData> ReadAsync(Stream stream, MemoryAllocator memoryAllocator)
        {
            int count = (int)stream.Length;
            using (IManagedByteBuffer buffer = memoryAllocator.AllocateManagedByteBuffer(count))
            {
                stream.Position = 0;
                await stream.ReadAsync(buffer.Array, 0, count).ConfigureAwait(false);
                return Parse(buffer.Memory.Span);
            }
        }

        /// <summary>
        /// Converts the string representation of the cached meta data to its <see cref="ImageMetaData" /> equivalent.
        /// </summary>
        /// <param name="buffer">The source buffer to parse.</param>
        /// <returns>The <see cref="ImageMetaData"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImageMetaData Parse(ReadOnlySpan<byte> buffer) => Unsafe.As<byte, ImageMetaData>(ref MemoryMarshal.GetReference(buffer));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ImageMetaData other)
        {
            return this.LastWriteTimeUtc == other.LastWriteTimeUtc
                   && this.ContentType == other.ContentType;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is ImageMetaData data && this.Equals(data);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // TODO: Replace with HashCode from Core.
            int hashCode = this.LastWriteTimeUtc.GetHashCode();
            return hashCode = (hashCode * 397) ^ this.ContentType.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString() => FormattableString.Invariant($"ImageMetaData({this.LastWriteTimeUtc}, {this.ContentType})");

        /// <summary>
        /// Calculates the number of bytes this <see cref="ImageMetaData"/> represents.
        /// </summary>
        /// <returns>The <see cref="int"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetByteCount()

            // 8 bytes for the datetime + 2 bytes per char * string length.
            => DateSize + (this.ContentType.Length * CharSize);

        /// <summary>
        /// Writes the metadata to the target buffer.
        /// </summary>
        /// <param name="buffer">The target buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteTo(Span<byte> buffer)
        {
            Guard.MustBeGreaterThanOrEqualTo(buffer.Length, this.GetByteCount(), nameof(buffer));

            ref ImageMetaData meta = ref Unsafe.As<byte, ImageMetaData>(ref MemoryMarshal.GetReference(buffer));
            meta = this;
        }

        /// <summary>
        /// Asynchronously writes the metadata to the target stream.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <param name="memoryAllocator">The memory allocator used for managing buffers.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task WriteAsync(Stream stream, MemoryAllocator memoryAllocator)
        {
            int count = this.GetByteCount();
            using (IManagedByteBuffer buffer = memoryAllocator.AllocateManagedByteBuffer(count))
            {
                this.WriteTo(buffer.Memory.Span);
                stream.Position = 0;
                await stream.WriteAsync(buffer.Array, 0, count).ConfigureAwait(false);
            }
        }
    }
}