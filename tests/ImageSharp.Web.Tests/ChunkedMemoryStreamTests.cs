// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests
{
    /// <summary>
    /// Tests for the <see cref="ChunkedMemoryStream"/> class.
    /// </summary>
    public class ChunkedMemoryStreamTests
    {
        [Fact]
        public static void MemoryStream_Ctor_InvalidCapacities()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChunkedMemoryStream(int.MinValue));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChunkedMemoryStream(0));
        }

        [Fact]
        public static void ChunkedMemoryStream_GetPositionTest_Negative()
        {
            using (var ms = new ChunkedMemoryStream())
            {
                long iCurrentPos = ms.Position;
                for (int i = -1; i > -6; i--)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => ms.Position = i);
                    Assert.Equal(ms.Position, iCurrentPos);
                }
            }
        }

        [Fact]
        public static void MemoryStream_ReadTest_Negative()
        {
            var ms2 = new ChunkedMemoryStream();

            Assert.Throws<ArgumentNullException>(() => ms2.Read(null, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ms2.Read(new byte[] { 1 }, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ms2.Read(new byte[] { 1 }, 0, -1));
            Assert.Throws<ArgumentException>(null, () => ms2.Read(new byte[] { 1 }, 2, 0));
            Assert.Throws<ArgumentException>(null, () => ms2.Read(new byte[] { 1 }, 0, 2));

            ms2.Dispose();

            Assert.Throws<ObjectDisposedException>(() => ms2.Read(new byte[] { 1 }, 0, 1));
        }

        [Fact]
        public static void MemoryStream_WriteToTests()
        {
            using (var ms2 = new ChunkedMemoryStream())
            {
                byte[] bytArrRet;
                byte[] bytArr = new byte[] { byte.MinValue, byte.MaxValue, 1, 2, 3, 4, 5, 6, 128, 250 };

                // [] Write to FileStream, check the filestream
                ms2.Write(bytArr, 0, bytArr.Length);

                using (var readonlyStream = new ChunkedMemoryStream())
                {
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
            }

            // [] Write to memoryStream, check the memoryStream
            using (var ms2 = new ChunkedMemoryStream())
            using (var ms3 = new ChunkedMemoryStream())
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
        public static void MemoryStream_WriteToTests_Negative()
        {
            using (var ms2 = new ChunkedMemoryStream())
            {
                Assert.Throws<ArgumentNullException>(() => ms2.WriteTo(null));

                ms2.Write(new byte[] { 1 }, 0, 1);
                var readonlyStream = new MemoryStream(new byte[1028], false);
                Assert.Throws<NotSupportedException>(() => ms2.WriteTo(readonlyStream));

                readonlyStream.Dispose();

                // [] Pass in a closed stream
                Assert.Throws<ObjectDisposedException>(() => ms2.WriteTo(readonlyStream));
            }
        }

        [Fact]
        public static void MemoryStream_CopyTo_Invalid()
        {
            ChunkedMemoryStream memoryStream;
            const string bufferSize = "bufferSize";
            using (memoryStream = new ChunkedMemoryStream())
            {
                const string destination = "destination";
                Assert.Throws<ArgumentNullException>(destination, () => memoryStream.CopyTo(destination: null));

                // Validate the destination parameter first.
                Assert.Throws<ArgumentNullException>(destination, () => memoryStream.CopyTo(destination: null, bufferSize: 0));
                Assert.Throws<ArgumentNullException>(destination, () => memoryStream.CopyTo(destination: null, bufferSize: -1));

                // Then bufferSize.
                Assert.Throws<ArgumentOutOfRangeException>(bufferSize, () => memoryStream.CopyTo(Stream.Null, bufferSize: 0)); // 0-length buffer doesn't make sense.
                Assert.Throws<ArgumentOutOfRangeException>(bufferSize, () => memoryStream.CopyTo(Stream.Null, bufferSize: -1));
            }

            // After the Stream is disposed, we should fail on all CopyTos.
            Assert.Throws<ArgumentOutOfRangeException>(bufferSize, () => memoryStream.CopyTo(Stream.Null, bufferSize: 0)); // Not before bufferSize is validated.
            Assert.Throws<ArgumentOutOfRangeException>(bufferSize, () => memoryStream.CopyTo(Stream.Null, bufferSize: -1));

            ChunkedMemoryStream disposedStream = memoryStream;

            // We should throw first for the source being disposed...
            Assert.Throws<ObjectDisposedException>(() => memoryStream.CopyTo(disposedStream, 1));

            // Then for the destination being disposed.
            memoryStream = new ChunkedMemoryStream();
            Assert.Throws<ObjectDisposedException>(() => memoryStream.CopyTo(disposedStream, 1));
        }

        [Theory]
        [MemberData(nameof(CopyToData))]
        public void CopyTo(Stream source, byte[] expected)
        {
            using (var destination = new ChunkedMemoryStream())
            {
                source.CopyTo(destination);
                Assert.InRange(source.Position, source.Length, int.MaxValue); // Copying the data should have read to the end of the stream or stayed past the end.
                Assert.Equal(expected, destination.ToArray());
            }
        }

        public static IEnumerable<object[]> CopyToData()
        {
            // Stream is positioned @ beginning of data
            byte[] data1 = new byte[] { 1, 2, 3 };
            var stream1 = new MemoryStream(data1);

            yield return new object[] { stream1, data1 };

            // Stream is positioned in the middle of data
            byte[] data2 = new byte[] { 0xff, 0xf3, 0xf0 };
            var stream2 = new MemoryStream(data2) { Position = 1 };

            yield return new object[] { stream2, new byte[] { 0xf3, 0xf0 } };

            // Stream is positioned after end of data
            byte[] data3 = data2;
            var stream3 = new MemoryStream(data3) { Position = data3.Length + 1 };

            yield return new object[] { stream3, Array.Empty<byte>() };
        }
    }
}
