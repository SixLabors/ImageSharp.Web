// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests
{
    public class ArrayPoolStreamTests
    {
        [Fact]
        public static void ArrayPoolStream_Ctor_NullPool()
        {
            Assert.Throws<ArgumentNullException>(() => new ArrayPoolStream(null));
        }

        [Fact]
        public static void ArrayPoolStream_Write_BeyondCapacity()
        {
            using var memoryStream = new ArrayPoolStream();

            long origLength = memoryStream.Length;
            byte[] bytes = new byte[10];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)i;
            }

            const int SpanPastEnd = 5;
            memoryStream.Seek(SpanPastEnd, SeekOrigin.End);
            Assert.Equal(memoryStream.Length + SpanPastEnd, memoryStream.Position);

            // Test Write
            memoryStream.Write(bytes, 0, bytes.Length);
            long pos = memoryStream.Position;
            Assert.Equal(pos, origLength + SpanPastEnd + bytes.Length);
            Assert.Equal(memoryStream.Length, origLength + SpanPastEnd + bytes.Length);

            // Verify bytes were correct.
            memoryStream.Position = origLength;
            byte[] newData = new byte[bytes.Length + SpanPastEnd];
            int n = memoryStream.Read(newData, 0, newData.Length);
            Assert.Equal(n, newData.Length);
            for (int i = 0; i < SpanPastEnd; i++)
            {
                Assert.Equal(0, newData[i]);
            }

            for (int i = 0; i < bytes.Length; i++)
            {
                Assert.Equal(bytes[i], newData[i + SpanPastEnd]);
            }
        }

        [Fact]
        public static void ArrayPoolStream_WriteByte_BeyondCapacity()
        {
            using var memoryStream = new ArrayPoolStream();

            long origLength = memoryStream.Length;
            byte[] bytes = new byte[10];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)i;
            }

            const int SpanPastEnd = 5;
            memoryStream.Seek(SpanPastEnd, SeekOrigin.End);
            Assert.Equal(memoryStream.Length + SpanPastEnd, memoryStream.Position);

            // Test WriteByte
            origLength = memoryStream.Length;
            memoryStream.Position = memoryStream.Length + SpanPastEnd;
            memoryStream.WriteByte(0x42);
            long expected = origLength + SpanPastEnd + 1;
            Assert.Equal(expected, memoryStream.Position);
            Assert.Equal(expected, memoryStream.Length);
        }

        [Fact]
        public static void ArrayPoolStream_GetPositionTest_Negative()
        {
            using var ms = new ArrayPoolStream();
            long iCurrentPos = ms.Position;

            for (int i = -1; i > -6; i--)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => ms.Position = i);
                Assert.Equal(ms.Position, iCurrentPos);
            }
        }

        [Fact]
        public static void ArrayPoolStream_LengthTest()
        {
            using var ms2 = new ArrayPoolStream();

            // [] Get the Length when position is at length
            ms2.SetLength(50);
            ms2.Position = 50;
            var sw2 = new StreamWriter(ms2);
            for (char c = 'a'; c < 'f'; c++)
            {
                sw2.Write(c);
            }

            sw2.Flush();
            Assert.Equal(55, ms2.Length);

            // Somewhere in the middle (set the length to be shorter.)
            ms2.SetLength(30);
            Assert.Equal(30, ms2.Length);
            Assert.Equal(30, ms2.Position);

            // Increase the length
            ms2.SetLength(100);
            Assert.Equal(100, ms2.Length);
            Assert.Equal(30, ms2.Position);
        }

        [Fact]
        public static void ArrayPoolStream_LengthTest_Negative()
        {
            using var ms2 = new ArrayPoolStream();
            Assert.Throws<ArgumentOutOfRangeException>(() => ms2.SetLength(long.MaxValue));
            Assert.Throws<ArgumentOutOfRangeException>(() => ms2.SetLength(-2));
        }

        [Fact]
        public static void ArrayPoolStream_ReadTest_Negative()
        {
            var ms2 = new ArrayPoolStream();

            Assert.Throws<ArgumentNullException>(() => ms2.Read(null, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ms2.Read(new byte[] { 1 }, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ms2.Read(new byte[] { 1 }, 0, -1));
            Assert.Throws<ArgumentException>(() => ms2.Read(new byte[] { 1 }, 2, 0));
            Assert.Throws<ArgumentException>(() => ms2.Read(new byte[] { 1 }, 0, 2));

            ms2.Dispose();

            Assert.Throws<ObjectDisposedException>(() => ms2.Read(new byte[] { 1 }, 0, 1));
        }

        [Fact]
        public static async Task ArrayPoolStream_ReadTest_NegativeAsync()
        {
            var ms2 = new ArrayPoolStream();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await ms2.ReadAsync(null, 0, 0));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await ms2.ReadAsync(new byte[] { 1 }, -1, 0));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await ms2.ReadAsync(new byte[] { 1 }, 0, -1));
            await Assert.ThrowsAsync<ArgumentException>(async () => await ms2.ReadAsync(new byte[] { 1 }, 2, 0));
            await Assert.ThrowsAsync<ArgumentException>(async () => await ms2.ReadAsync(new byte[] { 1 }, 0, 2));

            await StreamDisposeAsync(ms2);

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await ms2.ReadAsync(new byte[] { 1 }, 0, 1));
        }

        [Fact]
        public static void ArrayPoolStream_ReadByte()
        {
            // Short length so we hit optimized loop code.
            const int Length = 8;
            byte[] data = new byte[Length];
            new Random().NextBytes(data);

            using var ms1 = new ArrayPoolStream();
            ms1.Write(data);
            ms1.Position = 0;

            using var ms2 = new ArrayPoolStream();
            ms1.CopyTo(ms2);
            ms1.Position = 0;
            ms2.Position = 0;

            byte[] buffer = new byte[Length];
            ms1.Read(buffer);

            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(data[i], buffer[i]);
                Assert.Equal(data[i], ms2.ReadByte());
            }

            // Test read past stream length.
            Assert.Equal(0, ms1.Read(buffer));
            Assert.Equal(-1, ms2.ReadByte());
        }

        [Fact]
        public static async Task ArrayPoolStream_WriteToTests_ReadAsync()
        {
            using (var ms2 = new ArrayPoolStream())
            {
                byte[] bytArrRet;
                byte[] bytArr = new byte[] { byte.MinValue, byte.MaxValue, 1, 2, 3, 4, 5, 6, 128, 250 };

                // [] Write to memoryStream, check the memorystream
                await ms2.WriteAsync(bytArr, 0, bytArr.Length);

                using var readonlyStream = new ArrayPoolStream();

                ms2.WriteTo(readonlyStream);
                await readonlyStream.FlushAsync();
                readonlyStream.Position = 0;
                bytArrRet = new byte[(int)readonlyStream.Length];
                await readonlyStream.ReadAsync(bytArrRet, 0, (int)readonlyStream.Length);
                for (int i = 0; i < bytArr.Length; i++)
                {
                    Assert.Equal(bytArr[i], bytArrRet[i]);
                }
            }

            // [] Write to memoryStream, check the memoryStream
            using (var ms2 = new ArrayPoolStream())
            using (var ms3 = new ArrayPoolStream())
            {
                byte[] bytArrRet;
                byte[] bytArr = new byte[] { byte.MinValue, byte.MaxValue, 1, 2, 3, 4, 5, 6, 128, 250 };

                await ms2.WriteAsync(bytArr, 0, bytArr.Length);
                ms2.WriteTo(ms3);
                ms3.Position = 0;
                bytArrRet = new byte[(int)ms3.Length];
                await ms3.ReadAsync(bytArrRet, 0, (int)ms3.Length);
                for (int i = 0; i < bytArr.Length; i++)
                {
                    Assert.Equal(bytArr[i], bytArrRet[i]);
                }
            }
        }

        [Fact]
        public static void ArrayPoolStream_WriteToTests()
        {
            using (var ms2 = new ArrayPoolStream())
            {
                byte[] bytArrRet;
                byte[] bytArr = new byte[] { byte.MinValue, byte.MaxValue, 1, 2, 3, 4, 5, 6, 128, 250 };

                // [] Write to memoryStream, check the memorystream
                ms2.Write(bytArr, 0, bytArr.Length);

                using var readonlyStream = new ArrayPoolStream();

                ms2.WriteTo(readonlyStream);
                readonlyStream.Flush();
                readonlyStream.Position = 0;
                bytArrRet = new byte[(int)readonlyStream.Length];
                readonlyStream.Read(bytArrRet, 0, (int)readonlyStream.Length);
                for (int i = 0; i < bytArr.Length; i++)
                {
                    Assert.Equal(bytArr[i], bytArrRet[i]);
                }
            }

            // [] Write to memoryStream, check the memoryStream
            using (var ms2 = new ArrayPoolStream())
            using (var ms3 = new ArrayPoolStream())
            {
                byte[] bytArrRet;
                byte[] bytArr = new byte[] { byte.MinValue, byte.MaxValue, 1, 2, 3, 4, 5, 6, 128, 250 };

                ms2.Write(bytArr, 0, bytArr.Length);
                ms2.WriteTo(ms3);
                ms3.Position = 0;
                bytArrRet = new byte[(int)ms3.Length];
                ms3.Read(bytArrRet, 0, (int)ms3.Length);
                for (int i = 0; i < bytArr.Length; i++)
                {
                    Assert.Equal(bytArr[i], bytArrRet[i]);
                }
            }
        }

        [Fact]
        public static void ArrayPoolStream_WriteToTests_Negative()
        {
            using var ms2 = new ArrayPoolStream();

            Assert.Throws<ArgumentNullException>(() => ms2.WriteTo(null));

            ms2.Write(new byte[] { 1 }, 0, 1);
            var disposedStream = new ArrayPoolStream();
            disposedStream.Dispose();

            // [] Pass in a closed stream
            Assert.Throws<ObjectDisposedException>(() => ms2.WriteTo(disposedStream));
        }

        [Fact]
        public static void ArrayPoolStream_CopyTo_Invalid()
        {
            ArrayPoolStream memoryStream;
            using (memoryStream = new ArrayPoolStream())
            {
                Assert.Throws<ArgumentNullException>("destination", () => memoryStream.CopyTo(destination: null));

                // Validate the destination parameter first.
                Assert.Throws<ArgumentNullException>("destination", () => memoryStream.CopyTo(destination: null, bufferSize: 0));
                Assert.Throws<ArgumentNullException>("destination", () => memoryStream.CopyTo(destination: null, bufferSize: -1));

                // Then bufferSize.
                Assert.Throws<ArgumentOutOfRangeException>("bufferSize", () => memoryStream.CopyTo(Stream.Null, bufferSize: 0)); // 0-length buffer doesn't make sense.
                Assert.Throws<ArgumentOutOfRangeException>("bufferSize", () => memoryStream.CopyTo(Stream.Null, bufferSize: -1));
            }

            // After the Stream is disposed, we should fail on all CopyTos.
            Assert.Throws<ArgumentOutOfRangeException>("bufferSize", () => memoryStream.CopyTo(Stream.Null, bufferSize: 0)); // Not before bufferSize is validated.
            Assert.Throws<ArgumentOutOfRangeException>("bufferSize", () => memoryStream.CopyTo(Stream.Null, bufferSize: -1));

            ArrayPoolStream disposedStream = memoryStream;

            // We should throw first for the source being disposed...
            Assert.Throws<ObjectDisposedException>(() => memoryStream.CopyTo(disposedStream, 1));

            // Then for the destination being disposed.
            memoryStream = new ArrayPoolStream();
            Assert.Throws<ObjectDisposedException>(() => memoryStream.CopyTo(disposedStream, 1));

            // Then we should check whether the destination can read but can't write.
            var readOnlyStream = new DelegateStream(
                canReadFunc: () => true,
                canWriteFunc: () => false);

            Assert.Throws<NotSupportedException>(() => memoryStream.CopyTo(readOnlyStream, 1));

            memoryStream.Dispose();
        }

        [Fact]
        public static async Task ArrayPoolStream_CopyTo_InvalidAsync()
        {
            ArrayPoolStream memoryStream;
            using (memoryStream = new ArrayPoolStream())
            {
                await Assert.ThrowsAsync<ArgumentNullException>("destination", async () => await memoryStream.CopyToAsync(destination: null));

                // Validate the destination parameter first.
                await Assert.ThrowsAsync<ArgumentNullException>("destination", async () => await memoryStream.CopyToAsync(destination: null, bufferSize: 0));
                await Assert.ThrowsAsync<ArgumentNullException>("destination", async () => await memoryStream.CopyToAsync(destination: null, bufferSize: -1));

                // Then bufferSize.
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>("bufferSize", async () => await memoryStream.CopyToAsync(Stream.Null, bufferSize: 0)); // 0-length buffer doesn't make sense.
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>("bufferSize", async () => await memoryStream.CopyToAsync(Stream.Null, bufferSize: -1));
            }

            // After the Stream is disposed, we should fail on all CopyTos.
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>("bufferSize", async () => await memoryStream.CopyToAsync(Stream.Null, bufferSize: 0)); // Not before bufferSize is validated.
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>("bufferSize", async () => await memoryStream.CopyToAsync(Stream.Null, bufferSize: -1));

            ArrayPoolStream disposedStream = memoryStream;

            // We should throw first for the source being disposed...
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await memoryStream.CopyToAsync(disposedStream, 1));

            // Then for the destination being disposed.
            memoryStream = new ArrayPoolStream();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await memoryStream.CopyToAsync(disposedStream, 1));

            // Then we should check whether the destination can read but can't write.
            var readOnlyStream = new DelegateStream(
                canReadFunc: () => true,
                canWriteFunc: () => false);

            await Assert.ThrowsAsync<NotSupportedException>(async () => await memoryStream.CopyToAsync(readOnlyStream, 1));

            await StreamDisposeAsync(memoryStream);
        }

        [Theory]
        [MemberData(nameof(CopyToData))]
        public void ArrayPoolStream_CopyTo(Stream source, byte[] expected)
        {
            using var destination = new ArrayPoolStream();
            source.CopyTo(destination);

            // Copying the data should have read to the end of the stream or stayed past the end.
            Assert.InRange(source.Position, source.Length, int.MaxValue);
            Assert.Equal(expected, destination.ToArray());

            source.Dispose();
        }

        [Theory]
        [MemberData(nameof(CopyToData))]
        public async Task ArrayPoolStream_CopyToAsync(Stream source, byte[] expected)
        {
            using var destination = new ArrayPoolStream();
            await source.CopyToAsync(destination);

            // Copying the data should have read to the end of the stream or stayed past the end.
            Assert.InRange(source.Position, source.Length, int.MaxValue);
            Assert.Equal(expected, destination.ToArray());

#if NETCOREAPP2_1
            try
            {
                source.Dispose();
            }
            catch (Exception ex)
            {
                await new ValueTask(Task.FromException(ex));
            }
#else
            await source.DisposeAsync();
#endif
        }

        [Fact]
        public void ArrayPoolStream_WriteSpan_DataWrittenAndPositionUpdated_Success()
        {
            const int Iters = 100;
            byte[] data = new byte[(Iters * (Iters + 1)) / 2];
            new Random().NextBytes(data);

            using var s = new ArrayPoolStream();

            int expectedPos = 0;
            for (int i = 0; i <= Iters; i++)
            {
                s.Write(new ReadOnlySpan<byte>(data, expectedPos, i));
                expectedPos += i;
                Assert.Equal(expectedPos, s.Position);
            }

            Assert.Equal(data, s.ToArray());
        }

        [Fact]
        public void ArrayPoolStream_ReadSpan_DataReadAndPositionUpdated_Success()
        {
            const int Iters = 100;
            byte[] data = new byte[(Iters * (Iters + 1)) / 2];
            new Random().NextBytes(data);

            using var ms = new MemoryStream(data);
            using var s = new ArrayPoolStream();
            ms.CopyTo(s);
            s.Position = 0;

            int expectedPos = 0;
            for (int i = 0; i <= Iters; i++)
            {
                // Enough room to read the data and have some offset and have slack at the end
                var toRead = new Span<byte>(new byte[i * 3]);

                // Do the read and validate we read the expected number of bytes
                Assert.Equal(i, s.Read(toRead.Slice(i, i)));

                // The contents prior to and after the read should be empty.
                Assert.Equal<byte>(new byte[i], toRead.Slice(0, i).ToArray());
                Assert.Equal<byte>(new byte[i], toRead.Slice(i * 2, i).ToArray());

                // And the data read should match what was expected.
                Assert.Equal(new Span<byte>(data, expectedPos, i).ToArray(), toRead.Slice(i, i).ToArray());

                // Updated position should match
                expectedPos += i;
                Assert.Equal(expectedPos, s.Position);
            }

            // A final read should be empty
            Assert.Equal(0, s.Read(new Span<byte>(new byte[1])));
        }

        [Fact]
        public async Task ArrayPoolStream_WriteAsyncReadOnlyMemory_DataWrittenAndPositionUpdated_SuccessAsync()
        {
            const int Iters = 100;
            byte[] data = new byte[(Iters * (Iters + 1)) / 2];
            new Random().NextBytes(data);

            using var s = new ArrayPoolStream();
            int expectedPos = 0;
            for (int i = 0; i <= Iters; i++)
            {
                await s.WriteAsync(new ReadOnlyMemory<byte>(data, expectedPos, i));
                expectedPos += i;
                Assert.Equal(expectedPos, s.Position);
            }

            Assert.Equal(data, s.ToArray());
        }

        [Fact]
        public async Task ArrayPoolStream_ReadAsyncMemory_DataReadAndPositionUpdated_SuccessAsync()
        {
            const int Iters = 100;
            byte[] data = new byte[(Iters * (Iters + 1)) / 2];
            new Random().NextBytes(data);

            using var ms = new MemoryStream(data);
            using var s = new ArrayPoolStream();
            await ms.CopyToAsync(s);
            s.Position = 0;

            int expectedPos = 0;
            for (int i = 0; i <= Iters; i++)
            {
                // Enough room to read the data and have some offset and have slack at the end
                var toRead = new Memory<byte>(new byte[i * 3]);

                // Do the read and validate we read the expected number of bytes
                Assert.Equal(i, await s.ReadAsync(toRead.Slice(i, i)));

                // The contents prior to and after the read should be empty.
                Assert.Equal<byte>(new byte[i], toRead.Slice(0, i).ToArray());
                Assert.Equal<byte>(new byte[i], toRead.Slice(i * 2, i).ToArray());

                // And the data read should match what was expected.
                Assert.Equal(new Span<byte>(data, expectedPos, i).ToArray(), toRead.Slice(i, i).ToArray());

                // Updated position should match
                expectedPos += i;
                Assert.Equal(expectedPos, s.Position);
            }

            // A final read should be empty
            Assert.Equal(0, await s.ReadAsync(new Memory<byte>(new byte[1])));
        }

#if !NETCOREAPP2_1
        [Fact]
        public void ArrayPoolStream_DisposeAsync_ClosesStream()
        {
            var ms = new ArrayPoolStream();
            Assert.True(ms.DisposeAsync().IsCompletedSuccessfully);
            Assert.True(ms.DisposeAsync().IsCompletedSuccessfully);
            Assert.Throws<ObjectDisposedException>(() => ms.Position);
        }
#endif

        [Fact]
        public void ArrayPoolStream_TryGetBuffer()
        {
            var stream = new ArrayPoolStream();

            Assert.True(stream.TryGetBuffer(out ArraySegment<byte> buffer));
            Assert.Empty(buffer);

            // Now create some data.
            const int Length = 10;
            var data = new byte[Length];
            new Random().NextBytes(data);

            stream.Write(data, 0, data.Length);

            Assert.True(stream.TryGetBuffer(out buffer));
            Assert.Equal(Length, buffer.Count);

            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(data[i], buffer[i]);
            }

            stream.Dispose();
            Assert.False(stream.TryGetBuffer(out buffer));
            Assert.Empty(buffer);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(-1, -1)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 4)]
        [InlineData(4, 4)]
        [InlineData(5, 8)]
        [InlineData(6, 8)]
        [InlineData(7, 8)]
        [InlineData(8, 8)]
        [InlineData(9, 16)]
        [InlineData(913, 1024)]
        [InlineData(1023, 1024)]
        [InlineData(1024, 1024)]
        [InlineData(1025, 2048)]
        [InlineData(0b0010_0000_0000_0000_0000_0000_0000_0001, 0b0100_0000_0000_0000_0000_0000_0000_0000)]
        [InlineData(0b0100_0000_0000_0000_0000_0000_0000_0000, 0b0100_0000_0000_0000_0000_0000_0000_0000)]
        [InlineData(0b0100_0000_0000_0000_0000_0000_0000_0001, 0b0111_1111_1111_1111_1111_1111_1111_1111)]
        [InlineData(0b0111_1111_1111_1111_1111_1111_1111_1111, 0b0111_1111_1111_1111_1111_1111_1111_1111)]
        public void ArrayPoolStream_ValidateRoundUpBehavior(int capacity, int expected)
        {
            Assert.Equal(expected, ArrayPoolStream.RoundUp(capacity));
            Assert.Equal(expected, ArrayPoolStream.RoundUpScalar(capacity));
        }

        public static IEnumerable<object[]> CopyToData()
        {
            // Stream is positioned @ beginning of data
            var data1 = new byte[] { 1, 2, 3 };
            var stream1 = new MemoryStream(data1);

            yield return new object[] { stream1, data1 };

            // Stream is positioned in the middle of data
            var data2 = new byte[] { 0xff, 0xf3, 0xf0 };
            var stream2 = new MemoryStream(data2) { Position = 1 };

            yield return new object[] { stream2, new byte[] { 0xf3, 0xf0 } };

            // Stream is positioned after end of data
            var data3 = data2;
            var stream3 = new MemoryStream(data3) { Position = data3.Length + 1 };

            yield return new object[] { stream3, Array.Empty<byte>() };
        }

        private static ValueTask StreamDisposeAsync(Stream stream)
        {
            if (stream is null)
            {
                return default;
            }

#if NETCOREAPP2_1
            try
            {
                stream.Dispose();
                return default;
            }
            catch (Exception ex)
            {
                return new ValueTask(Task.FromException(ex));
            }
#else
            return stream.DisposeAsync();
#endif
        }
    }

    /// <summary>
    /// Provides a stream whose implementation is supplied by delegates.
    /// </summary>
    internal sealed class DelegateStream : Stream
    {
        private readonly Func<bool> canReadFunc;
        private readonly Func<bool> canSeekFunc;
        private readonly Func<bool> canWriteFunc;
        private readonly Action flushFunc = null;
        private readonly Func<CancellationToken, Task> flushAsyncFunc = null;
        private readonly Func<long> lengthFunc;
        private readonly Func<long> positionGetFunc;
        private readonly Action<long> positionSetFunc;
        private readonly Func<byte[], int, int, int> readFunc;
        private readonly Func<byte[], int, int, CancellationToken, Task<int>> readAsyncFunc;
        private readonly Func<long, SeekOrigin, long> seekFunc;
        private readonly Action<long> setLengthFunc;
        private readonly Action<byte[], int, int> writeFunc;
        private readonly Func<byte[], int, int, CancellationToken, Task> writeAsyncFunc;
        private readonly Action<bool> disposeFunc;

        public DelegateStream(
            Func<bool> canReadFunc = null,
            Func<bool> canSeekFunc = null,
            Func<bool> canWriteFunc = null,
            Action flushFunc = null,
            Func<CancellationToken, Task> flushAsyncFunc = null,
            Func<long> lengthFunc = null,
            Func<long> positionGetFunc = null,
            Action<long> positionSetFunc = null,
            Func<byte[], int, int, int> readFunc = null,
            Func<byte[], int, int, CancellationToken, Task<int>> readAsyncFunc = null,
            Func<long, SeekOrigin, long> seekFunc = null,
            Action<long> setLengthFunc = null,
            Action<byte[], int, int> writeFunc = null,
            Func<byte[], int, int, CancellationToken, Task> writeAsyncFunc = null,
            Action<bool> disposeFunc = null)
        {
            this.canReadFunc = canReadFunc ?? (() => false);
            this.canSeekFunc = canSeekFunc ?? (() => false);
            this.canWriteFunc = canWriteFunc ?? (() => false);

            this.flushFunc = flushFunc ?? (() => { });
            this.flushAsyncFunc = flushAsyncFunc ?? (token => base.FlushAsync(token));

            this.lengthFunc = lengthFunc ?? (() => throw new NotSupportedException());
            this.positionSetFunc = positionSetFunc ?? (_ => throw new NotSupportedException());
            this.positionGetFunc = positionGetFunc ?? (() => throw new NotSupportedException());

            this.readAsyncFunc = readAsyncFunc ?? ((buffer, offset, count, token) => base.ReadAsync(buffer, offset, count, token));
            this.readFunc = readFunc ?? ((buffer, offset, count) => readAsyncFunc(buffer, offset, count, default).GetAwaiter().GetResult());

            this.seekFunc = seekFunc ?? ((_, __) => throw new NotSupportedException());
            this.setLengthFunc = setLengthFunc ?? (_ => throw new NotSupportedException());

            this.writeAsyncFunc = writeAsyncFunc ?? ((buffer, offset, count, token) => base.WriteAsync(buffer, offset, count, token));
            this.writeFunc = writeFunc ?? ((buffer, offset, count) => writeAsyncFunc(buffer, offset, count, default).GetAwaiter().GetResult());

            this.disposeFunc = disposeFunc;
        }

        public override bool CanRead => this.canReadFunc();

        public override bool CanSeek => this.canSeekFunc();

        public override bool CanWrite => this.canWriteFunc();

        public override void Flush() => this.flushFunc();

        public override Task FlushAsync(CancellationToken cancellationToken)
            => this.flushAsyncFunc(cancellationToken);

        public override long Length => this.lengthFunc();

        public override long Position
        {
            get { return this.positionGetFunc(); }
            set { this.positionSetFunc(value); }
        }

        public override int Read(byte[] buffer, int offset, int count)
            => this.readFunc(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => this.readAsyncFunc(buffer, offset, count, cancellationToken);

        public override long Seek(long offset, SeekOrigin origin)
            => this.seekFunc(offset, origin);

        public override void SetLength(long value)
            => this.setLengthFunc(value);

        public override void Write(byte[] buffer, int offset, int count)
            => this.writeFunc(buffer, offset, count);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => this.writeAsyncFunc(buffer, offset, count, cancellationToken);

        protected override void Dispose(bool disposing)
            => this.disposeFunc?.Invoke(disposing);
    }
}
