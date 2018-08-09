// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Web.Memory;
using SixLabors.Memory;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Memory
{
    public class PooledBufferManagerTests
    {
        private readonly IBufferManager manager = new PooledBufferManager();

        [Theory]
        [InlineData(32)]
        [InlineData(512)]
        [InlineData(PooledBufferManager.DefaultMaxLength - 1)]
        public void BuffersArePooled(int size)
        {
            Assert.True(this.CheckIsRentingPooledBuffer<byte>(size));
        }

        [Theory]
        [InlineData(32, 0)]
        [InlineData(0, 25)]
        public void PooledBufferManagerThrowsWhenParamsAreInvalid(int maxLength, int maxArraysPerBucket)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var m = new PooledBufferManager(maxLength, maxArraysPerBucket);
            });
        }

        [Theory]
        [InlineData(32)]
        [InlineData(512)]
        [InlineData(123)]
        [InlineData(PooledBufferManager.DefaultMaxLength - 1)]
        public void BufferLengthIsCorrect(int size)
        {
            using (IManagedByteBuffer buffer = this.manager.Allocate(size))
            {
                Assert.Equal(size, buffer.Memory.Length);
            }
        }

        /// <summary>
        /// Rent a buffer -> return it -> re-rent -> verify if it's array points to the previous location
        /// </summary>
        private bool CheckIsRentingPooledBuffer<T>(int length)
            where T : struct
        {
            IManagedByteBuffer buffer = this.manager.Allocate(length);
            ref byte ptrToPreviousPosition0 = ref MemoryMarshal.GetReference<byte>(buffer.Array);
            buffer.Dispose();

            buffer = this.manager.Allocate(length);
            bool sameBuffers = Unsafe.AreSame(ref ptrToPreviousPosition0, ref MemoryMarshal.GetReference<byte>(buffer.Array));
            buffer.Dispose();

            return sameBuffers;
        }
    }
}
