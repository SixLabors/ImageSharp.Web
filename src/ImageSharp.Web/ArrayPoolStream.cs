// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#if SUPPORTS_RUNTIME_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif

// Adapted from Pipelines.Sockets.Unofficial. MIT Licensed
// https://github.com/mgravell/Pipelines.Sockets.Unofficial/blob/6740ea4f79a9ae75fda9de23d06ae4a614a516cf/src/Pipelines.Sockets.Unofficial/ArrayPoolStream.cs
namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// An in-memory stream similar to <see cref="MemoryStream"/>, that rents contiguous
    /// buffers from the array pool.
    /// </summary>
    public sealed class ArrayPoolStream : Stream
    {
        private static readonly Task<int> Zero = Task.FromResult(0);

        // Buffer successful large reads, since they tend to be repetitive and
        // same-sized (buffers, etc)
        private Task<int> lastSuccessfulReadTask;
        private readonly ArrayPool<byte> pool;
        private byte[] array = Array.Empty<byte>();
        private int position;
        private int length;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayPoolStream"/> class
        /// using the default array pool.
        /// </summary>
        public ArrayPoolStream()
            : this(ArrayPool<byte>.Shared)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayPoolStream"/> class
        /// using the specified array pool.
        /// </summary>
        /// <param name="pool">The pool to use.</param>
        public ArrayPoolStream(ArrayPool<byte> pool)
        {
            Guard.NotNull(pool, nameof(pool));
            this.pool = pool;
        }

        /// <inheritdoc/>
        public override bool CanRead => !this.isDisposed;

        /// <inheritdoc/>
        public override bool CanWrite => !this.isDisposed;

        /// <inheritdoc/>
        public override bool CanTimeout => false;

        /// <inheritdoc/>
        public override bool CanSeek => !this.isDisposed;

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                this.EnsureNotDisposed();
                return this.position;
            }

            set
            {
                Guard.MustBeBetweenOrEqualTo(value, 0, int.MaxValue, nameof(this.Position));
                this.EnsureNotDisposed();

                this.position = (int)value;
            }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                this.EnsureNotDisposed();
                return this.length;
            }
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            Guard.MustBeBetweenOrEqualTo(value, 0, int.MaxValue, nameof(value));

            if (value == this.length)
            {
                // Nothing to do
                return;
            }

            if (value < this.length)
            {
                // Shrink
                this.length = (int)value;
                if (this.position > this.length)
                {
                    this.position = this.length; // leave at EOF
                }
            }
            else
            {
                // Grow
                this.ExpandCapacity((int)value);
            }
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            this.EnsureNotDisposed();
            Guard.MustBeLessThanOrEqualTo(offset, int.MaxValue, nameof(offset));

            switch (origin)
            {
                case SeekOrigin.Begin:
                    return this.Position = offset;
                case SeekOrigin.End:
                    return this.Position = this.Length + offset;
                case SeekOrigin.Current:
                    return this.Position += offset;
                default:
                    ThrowArgumentOutOfRange(nameof(origin));
                    return default;
            }
        }

        /// <summary>
        /// Exposes the underlying buffer associated with this stream, for the defined length.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        /// <returns><see langword="true"/> if a filled buffer can be returned; otherwise <see langword="false"/>.</returns>
        public bool TryGetBuffer(out ArraySegment<byte> buffer)
        {
            buffer = new ArraySegment<byte>(this.array, 0, Math.Max(0, this.length));
            return this.length >= 0;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            if (disposing)
            {
                this.position = this.length = -1;
                byte[] arr = this.array;
                this.array = Array.Empty<byte>();

                if (arr.Length != 0)
                {
                    this.pool.Return(arr);
                }
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Guard.NotNull(buffer, nameof(buffer));
            Guard.MustBeGreaterThanOrEqualTo(offset, 0, nameof(offset));
            Guard.MustBeGreaterThanOrEqualTo(count, 0, nameof(count));
            Guard.IsFalse(buffer.Length - offset < count, nameof(buffer), $"{offset} subtracted from the buffer length is less than {count}");

            this.EnsureNotDisposed();

            int n = this.length - this.position;
            if (n > count)
            {
                n = count;
            }

            if (n <= 0)
            {
                return 0;
            }

            if (n <= 8)
            {
                int byteCount = n;
                while (--byteCount >= 0)
                {
                    buffer[offset + byteCount] = this.array[this.position + byteCount];
                }
            }
            else
            {
                Buffer.BlockCopy(this.array, this.position, buffer, offset, n);
            }

            this.position += n;

            return n;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            this.EnsureNotDisposed();

            int n = Math.Min(this.length - this.position, buffer.Length);
            if (n <= 0)
            {
                return 0;
            }

            new Span<byte>(this.array, this.position, n).CopyTo(buffer);

            this.position += n;
            return n;
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            try
            {
                int bytes = this.Read(buffer, offset, count);
                if (bytes == 0)
                {
                    return Zero;
                }

                if (this.lastSuccessfulReadTask != null && this.lastSuccessfulReadTask.Result == bytes)
                {
                    return this.lastSuccessfulReadTask;
                }

                return this.lastSuccessfulReadTask = Task.FromResult(bytes);
            }
            catch (Exception ex)
            {
                return Task.FromException<int>(ex);
            }
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask<int>(Task.FromCanceled<int>(cancellationToken));
            }

            try
            {
                return new ValueTask<int>(this.Read(buffer.Span));
            }
            catch (Exception ex)
            {
                return new ValueTask<int>(Task.FromException<int>(ex));
            }
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            this.EnsureNotDisposed();
            return this.position >= this.length ? -1 : this.array[this.position++];
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Guard.NotNull(buffer, nameof(buffer));
            Guard.MustBeGreaterThanOrEqualTo(offset, 0, nameof(offset));
            Guard.MustBeGreaterThanOrEqualTo(count, 0, nameof(count));
            Guard.IsFalse(buffer.Length - offset < count, nameof(buffer), $"{offset} subtracted from the buffer length is less than {count}");

            this.EnsureNotDisposed();

            int positionAfter = this.position + count;
            if (positionAfter < 0)
            {
                ThrowInvalidOperation();
            }

            if (positionAfter > this.length)
            {
                this.ExpandCapacity(positionAfter);
            }

            Buffer.BlockCopy(buffer, offset, this.array, this.position, count);
            this.position = positionAfter;
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            this.EnsureNotDisposed();

            int positionAfter = this.position + buffer.Length;
            if (positionAfter < 0)
            {
                ThrowInvalidOperation();
            }

            if (positionAfter > this.length)
            {
                this.ExpandCapacity(positionAfter);
            }

            buffer.CopyTo(new Span<byte>(this.array, this.position, buffer.Length));
            this.position = positionAfter;
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            this.EnsureNotDisposed();

            if (this.position >= this.length)
            {
                this.ExpandCapacity(this.position + 1);
            }

            this.array[this.position++] = value;
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            try
            {
                this.Write(buffer, offset, count);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException<int>(ex);
            }
        }

        /// <summary>
        /// Writes this <see cref="ArrayPoolStream"/> to another stream.
        /// </summary>
        /// <param name="stream">The stream to write this stream to.</param>
        public void WriteTo(Stream stream)
        {
            Guard.NotNull(stream, nameof(stream));

            this.EnsureNotDisposed();

            stream.Write(this.array, 0, this.length);
        }

        /// <summary>
        /// Writes the stream contents to a byte array, regardless of the <see cref="Position"/> property.
        /// </summary>
        /// <returns>The new byte array.</returns>
        public byte[] ToArray()
        {
            int count = this.length;
            if (count == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] copy = new byte[count];
            Buffer.BlockCopy(this.array, 0, copy, 0, count);
            return copy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExpandCapacity(int capacity)
        {
            if (capacity > this.array.Length)
            {
                this.TakeNewBuffer(capacity);
            }

            this.length = capacity;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void TakeNewBuffer(int capacity)
        {
            byte[] oldArr = this.array;
            int oldLength = this.length;
            byte[] newArr = this.pool.Rent(RoundUp(capacity));
            if (this.length != 0)
            {
                Buffer.BlockCopy(oldArr, 0, newArr, 0, oldLength);
            }

            // Zero the contents when growing
            new Span<byte>(newArr, oldLength, capacity - oldLength).Clear();

            this.array = newArr;
            if (oldLength != 0)
            {
                this.pool.Return(oldArr);
            }
        }

        /// <inheritdoc/>
        public override void CopyTo(Stream destination, int bufferSize)
        {
            Guard.NotNull(destination, nameof(destination));
            Guard.MustBeGreaterThan(bufferSize, 0, nameof(bufferSize));
            this.EnsureNotDisposed();

            bool destinationCanWrite = destination.CanWrite;
            if (!destinationCanWrite && !destination.CanRead)
            {
                ThrowObjectDisposed(nameof(destination));
            }

            if (!destinationCanWrite)
            {
                ThrowNotSupported("Unwritable stream.");
            }

            // Seek to the end of the memory stream.
            int originalPosition = this.position;
            int remaining = this.InternalEmulateRead(this.length - originalPosition);

            if (remaining > 0)
            {
                // Call Write() on the other Stream, using our internal buffer and avoiding any
                // intermediary allocations.
                destination.Write(this.array, originalPosition, remaining);
            }
        }

        /// <inheritdoc/>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            Guard.NotNull(destination, nameof(destination));
            Guard.MustBeGreaterThan(bufferSize, 0, nameof(bufferSize));
            this.EnsureNotDisposed();

            bool destinationCanWrite = destination.CanWrite;
            if (!destinationCanWrite && !destination.CanRead)
            {
                ThrowObjectDisposed(nameof(destination));
            }

            if (!destinationCanWrite)
            {
                ThrowNotSupported("Unwritable stream.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            // Seek to the end of the memory stream.
            int originalPosition = this.position;
            int remaining = this.InternalEmulateRead(this.length - originalPosition);

            // If we were already at or past the end, there's no copying to do so just quit.
            if (remaining == 0)
            {
                return Task.CompletedTask;
            }

            if (destination is MemoryStream || destination is ArrayPoolStream)
            {
                // Write sync
                try
                {
                    destination.Write(this.array, originalPosition, remaining);
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }
            }
            else
            {
                return destination.WriteAsync(this.array, originalPosition, remaining, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask(Task.FromCanceled(cancellationToken));
            }

            try
            {
                this.Write(buffer.Span);
                return default;
            }
            catch (Exception ex)
            {
                return new ValueTask(Task.FromException<int>(ex));
            }
        }

        /// <inheritdoc/>
        public override string ToString()
            => $"{nameof(ArrayPoolStream)} at position {this.Position} of {this.Length}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int RoundUp(int capacity)
        {
            // We need to do this because array-pools stop buffering beyond
            // a certain point, and just give us what we ask for; if we don't
            // apply upwards rounding *ourselves*, then beyond that limit, we
            // end up *constantly* allocating/copying arrays, on each copy.
            //
            // Note we subtract one because it is easier to round up to the *next* bucket size, and
            // subtracting one guarantees that this will work.
            //
            // If we ask for, say, 913; take 1 for 912; that's 0000 0000 0000 0000 0000 0011 1001 0000
            // so lz is 22; 32-22=10, 1 << 10= 1024
            //
            // or for 2: lz of 2-1 is 31, 32-31=1; 1<<1=2
#if SUPPORTS_RUNTIME_INTRINSICS
            return RoundUpLzcnt(capacity);
#else
            return RoundUpScalar(capacity);
#endif
        }

#if SUPPORTS_RUNTIME_INTRINSICS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int RoundUpLzcnt(int capacity)
        {
            if (Lzcnt.IsSupported)
            {
                if (capacity <= 1)
                {
                    return capacity;
                }

                int limit = 1 << (32 - (int)Lzcnt.LeadingZeroCount((uint)(capacity - 1)));
                return limit < 0 ? int.MaxValue : limit;
            }

            return RoundUpScalar(capacity);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int RoundUpScalar(int capacity)
        {
            if (capacity <= 1)
            {
                return capacity;
            }

            static int LeadingZeros(int x)
            {
                // Compile time constant
                const int NumIntBits = sizeof(int) * 8;

                // Do the smearing
                x |= x >> 1;
                x |= x >> 2;
                x |= x >> 4;
                x |= x >> 8;
                x |= x >> 16;

                // Count the ones
                x -= x >> 1 & 0x55555555;
                x = (x >> 2 & 0x33333333) + (x & 0x33333333);
                x = (x >> 4) + x & 0x0f0f0f0f;
                x += x >> 8;
                x += x >> 16;

                // Subtract # of 1s from 32
                return NumIntBits - (x & 0x0000003f);
            }

            int limit = 1 << (32 - LeadingZeros(capacity - 1));
            return limit < 0 ? int.MaxValue : limit;
        }

        private int InternalEmulateRead(int count)
        {
            this.EnsureNotDisposed();

            int n = this.length - this.position;
            if (n > count)
            {
                n = count;
            }

            if (n < 0)
            {
                n = 0;
            }

            // length is less than 2^31 -1.
            Debug.Assert(this.position + n >= 0, "_position + n >= 0");
            this.position += n;
            return n;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentOutOfRange(string name)
            => throw new ArgumentOutOfRangeException(name);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperation()
            => throw new InvalidOperationException();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowObjectDisposed(string name)
            => throw new ObjectDisposedException(name, "Stream is closed.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowNotSupported(string message)
            => throw new NotSupportedException(message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                ThrowObjectDisposed(null);
            }
        }
    }
}
