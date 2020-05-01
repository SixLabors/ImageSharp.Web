// Copyright (c) Six Labors and contributors.
// Licensed under the GNU Affero General Public License, Version 3.

using System;
using System.Buffers;
using System.IO;

// Adapted from<see href="https://referencesource.microsoft.com/#System.Runtime.Remoting/channels/core/chunkedmemorystream.cs"/>.
namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// A Memory stream that uses buffer pooling and doesn't need to be resized.
    /// Disposing of the stream returns the buffers back to the pool.
    /// </summary>
    public sealed class ChunkedMemoryStream : Stream
    {
        /// <summary>
        /// The default length in bytes of each buffer chunk.
        /// </summary>
        public const int DefaultBufferLength = 4096;

        // Data
        private MemoryChunk memoryChunk;

        // The length of each buffer chunk
        private readonly int chunkLength;

        // Has the stream been disposed.
        private bool isDisposed;

        // Current chunk to write to
        private MemoryChunk writeChunk;

        // Offset into chunk to write to
        private int writeOffset;

        // Current chunk to read from
        private MemoryChunk readChunk;

        // Offset into chunk to read from
        private int readOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkedMemoryStream"/> class.
        /// </summary>
        public ChunkedMemoryStream()
            : this(DefaultBufferLength)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkedMemoryStream"/> class.
        /// </summary>
        /// <param name="bufferLength">The length, in bytes of each buffer chunk.</param>
        public ChunkedMemoryStream(int bufferLength)
        {
            Guard.MustBeGreaterThan(bufferLength, 0, nameof(bufferLength));

            this.chunkLength = bufferLength;
        }

        /// <inheritdoc/>
        public override bool CanRead => !this.isDisposed;

        /// <inheritdoc/>
        public override bool CanSeek => !this.isDisposed;

        /// <inheritdoc/>
        public override bool CanWrite => !this.isDisposed;

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                this.EnsureNotDisposed();

                int length = 0;
                MemoryChunk chunk = this.memoryChunk;
                while (chunk != null)
                {
                    MemoryChunk next = chunk.Next;
                    if (next != null)
                    {
                        length += chunk.Length;
                    }
                    else
                    {
                        length += this.writeOffset;
                    }

                    chunk = next;
                }

                return length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                this.EnsureNotDisposed();

                if (this.readChunk == null)
                {
                    return 0;
                }

                int pos = 0;
                MemoryChunk chunk = this.memoryChunk;
                while (chunk != this.readChunk)
                {
                    pos += chunk.Length;
                    chunk = chunk.Next;
                }

                pos += this.readOffset;

                return pos;
            }

            set
            {
                this.EnsureNotDisposed();

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                // Back up current position in case new position is out of range
                MemoryChunk backupReadChunk = this.readChunk;
                int backupReadOffset = this.readOffset;

                this.readChunk = null;
                this.readOffset = 0;

                int leftUntilAtPos = (int)value;
                MemoryChunk chunk = this.memoryChunk;
                while (chunk != null)
                {
                    if ((leftUntilAtPos < chunk.Length)
                            || ((leftUntilAtPos == chunk.Length)
                            && (chunk.Next == null)))
                    {
                        // The desired position is in this chunk
                        this.readChunk = chunk;
                        this.readOffset = leftUntilAtPos;
                        break;
                    }

                    leftUntilAtPos -= chunk.Length;
                    chunk = chunk.Next;
                }

                if (this.readChunk == null)
                {
                    // Position is out of range
                    this.readChunk = backupReadChunk;
                    this.readOffset = backupReadOffset;
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            this.EnsureNotDisposed();

            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.Position = offset;
                    break;

                case SeekOrigin.Current:
                    this.Position += offset;
                    break;

                case SeekOrigin.End:
                    this.Position = this.Length + offset;
                    break;
            }

            return this.Position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            try
            {
                this.isDisposed = true;
                if (disposing)
                {
                    this.ReleaseMemoryChunks(this.memoryChunk);
                }

                this.memoryChunk = null;
                this.writeChunk = null;
                this.readChunk = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Guard.NotNull(buffer, nameof(buffer));
            Guard.MustBeGreaterThanOrEqualTo(offset, 0, nameof(offset));
            Guard.MustBeGreaterThanOrEqualTo(count, 0, nameof(count));
            if (buffer.Length - offset < count)
            {
                throw new ArgumentException($"{offset} subtracted from the buffer length is less than {count}");
            }

            this.EnsureNotDisposed();

            if (this.readChunk == null)
            {
                if (this.memoryChunk == null)
                {
                    return 0;
                }

                this.readChunk = this.memoryChunk;
                this.readOffset = 0;
            }

            byte[] chunkBuffer = this.readChunk.Buffer;
            int chunkSize = this.readChunk.Length;
            if (this.readChunk.Next == null)
            {
                chunkSize = this.writeOffset;
            }

            int bytesRead = 0;

            while (count > 0)
            {
                if (this.readOffset == chunkSize)
                {
                    // Exit if no more chunks are currently available
                    if (this.readChunk.Next == null)
                    {
                        break;
                    }

                    this.readChunk = this.readChunk.Next;
                    this.readOffset = 0;
                    chunkBuffer = this.readChunk.Buffer;
                    chunkSize = this.readChunk.Length;
                    if (this.readChunk.Next == null)
                    {
                        chunkSize = this.writeOffset;
                    }
                }

                int readCount = Math.Min(count, chunkSize - this.readOffset);
                Buffer.BlockCopy(chunkBuffer, this.readOffset, buffer, offset, readCount);
                offset += readCount;
                count -= readCount;
                this.readOffset += readCount;
                bytesRead += readCount;
            }

            return bytesRead;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            this.EnsureNotDisposed();

            if (this.readChunk == null)
            {
                if (this.memoryChunk == null)
                {
                    return 0;
                }

                this.readChunk = this.memoryChunk;
                this.readOffset = 0;
            }

            byte[] chunkBuffer = this.readChunk.Buffer;
            int chunkSize = this.readChunk.Length;
            if (this.readChunk.Next == null)
            {
                chunkSize = this.writeOffset;
            }

            if (this.readOffset == chunkSize)
            {
                // Exit if no more chunks are currently available
                if (this.readChunk.Next == null)
                {
                    return -1;
                }

                this.readChunk = this.readChunk.Next;
                this.readOffset = 0;
                chunkBuffer = this.readChunk.Buffer;
            }

            return chunkBuffer[this.readOffset++];
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.EnsureNotDisposed();

            if (this.memoryChunk == null)
            {
                this.memoryChunk = this.AllocateMemoryChunk();
                this.writeChunk = this.memoryChunk;
                this.writeOffset = 0;
            }

            byte[] chunkBuffer = this.writeChunk.Buffer;
            int chunkSize = this.writeChunk.Length;

            while (count > 0)
            {
                if (this.writeOffset == chunkSize)
                {
                    // Allocate a new chunk if the current one is full
                    this.writeChunk.Next = this.AllocateMemoryChunk();
                    this.writeChunk = this.writeChunk.Next;
                    this.writeOffset = 0;
                    chunkBuffer = this.writeChunk.Buffer;
                    chunkSize = this.writeChunk.Length;
                }

                int copyCount = Math.Min(count, chunkSize - this.writeOffset);
                Buffer.BlockCopy(buffer, offset, chunkBuffer, this.writeOffset, copyCount);
                offset += copyCount;
                count -= copyCount;
                this.writeOffset += copyCount;
            }
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            this.EnsureNotDisposed();

            if (this.memoryChunk == null)
            {
                this.memoryChunk = this.AllocateMemoryChunk();
                this.writeChunk = this.memoryChunk;
                this.writeOffset = 0;
            }

            byte[] chunkBuffer = this.writeChunk.Buffer;
            int chunkSize = this.writeChunk.Length;

            if (this.writeOffset == chunkSize)
            {
                // Allocate a new chunk if the current one is full
                this.writeChunk.Next = this.AllocateMemoryChunk();
                this.writeChunk = this.writeChunk.Next;
                this.writeOffset = 0;
                chunkBuffer = this.writeChunk.Buffer;
            }

            chunkBuffer[this.writeOffset++] = value;
        }

        /// <summary>
        /// Copy entire buffer into an array.
        /// </summary>
        /// <returns>The <see cref="T:byte[]"/>.</returns>
        public byte[] ToArray()
        {
            int length = (int)this.Length; // This will throw if stream is closed
            byte[] copy = new byte[this.Length];

            MemoryChunk backupReadChunk = this.readChunk;
            int backupReadOffset = this.readOffset;

            this.readChunk = this.memoryChunk;
            this.readOffset = 0;
            this.Read(copy, 0, length);

            this.readChunk = backupReadChunk;
            this.readOffset = backupReadOffset;

            return copy;
        }

        /// <summary>
        /// Write remainder of this stream to another stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public void WriteTo(Stream stream)
        {
            this.EnsureNotDisposed();

            Guard.NotNull(stream, nameof(stream));

            if (this.readChunk == null)
            {
                if (this.memoryChunk == null)
                {
                    return;
                }

                this.readChunk = this.memoryChunk;
                this.readOffset = 0;
            }

            byte[] chunkBuffer = this.readChunk.Buffer;
            int chunkSize = this.readChunk.Length;
            if (this.readChunk.Next == null)
            {
                chunkSize = this.writeOffset;
            }

            // Following code mirrors Read() logic (readChunk/readOffset should
            // point just past last byte of last chunk when done)
            // loop until end of chunks is found
            while (true)
            {
                if (this.readOffset == chunkSize)
                {
                    // Exit if no more chunks are currently available
                    if (this.readChunk.Next == null)
                    {
                        break;
                    }

                    this.readChunk = this.readChunk.Next;
                    this.readOffset = 0;
                    chunkBuffer = this.readChunk.Buffer;
                    chunkSize = this.readChunk.Length;
                    if (this.readChunk.Next == null)
                    {
                        chunkSize = this.writeOffset;
                    }
                }

                int writeCount = chunkSize - this.readOffset;
                stream.Write(chunkBuffer, this.readOffset, writeCount);
                this.readOffset = chunkSize;
            }
        }

        private void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(null, "The stream is closed.");
            }
        }

        private MemoryChunk AllocateMemoryChunk()
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(this.chunkLength);
            return new MemoryChunk
            {
                Buffer = buffer,
                Next = null,
                Length = buffer.Length
            };
        }

        private void ReleaseMemoryChunks(MemoryChunk chunk)
        {
            while (chunk != null)
            {
                ArrayPool<byte>.Shared.Return(chunk.Buffer);
                chunk = chunk.Next;
            }
        }

        private class MemoryChunk
        {
            public byte[] Buffer { get; set; }

            public MemoryChunk Next { get; set; }

            public int Length { get; set; }
        }
    }
}
