// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Synchronization;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Synchronization
{
    public class AsyncReaderWriterLockTests
    {
        private readonly AsyncReaderWriterLock l = new();

        [Fact]
        public void OneWriterAtATime()
        {
            Task<IDisposable> first = this.l.WriterLockAsync();
            Assert.True(first.IsCompletedSuccessfully);

            Task<IDisposable> second = this.l.WriterLockAsync();
            Assert.False(second.IsCompleted);

            first.Result.Dispose();
            Assert.True(second.IsCompletedSuccessfully);
        }

        [Fact]
        public void WriterBlocksReaders()
        {
            Task<IDisposable> first = this.l.WriterLockAsync();
            Assert.True(first.IsCompletedSuccessfully);

            Task<IDisposable> second = this.l.ReaderLockAsync();
            Assert.False(second.IsCompleted);

            first.Result.Dispose();
            Assert.True(second.IsCompletedSuccessfully);
        }

        [Fact]
        public void WaitingWriterBlocksReaders()
        {
            Task<IDisposable> first = this.l.ReaderLockAsync();
            Assert.True(first.IsCompletedSuccessfully);

            Task<IDisposable> second = this.l.WriterLockAsync();
            Assert.False(second.IsCompleted);

            Task<IDisposable> third = this.l.ReaderLockAsync();
            Assert.False(third.IsCompleted);

            first.Result.Dispose();
            Assert.True(second.IsCompletedSuccessfully);
            Assert.False(third.IsCompleted);

            second.Result.Dispose();
            Assert.True(third.IsCompletedSuccessfully);
        }

        [Fact]
        public void MultipleReadersAtOnce()
        {
            Task<IDisposable> first = this.l.ReaderLockAsync();
            Assert.True(first.IsCompletedSuccessfully);

            Task<IDisposable> second = this.l.ReaderLockAsync();
            Assert.True(second.IsCompletedSuccessfully);

            Task<IDisposable> third = this.l.ReaderLockAsync();
            Assert.True(third.IsCompletedSuccessfully);
        }

        [Fact]
        public void AllWaitingReadersReleasedConcurrently()
        {
            Task<IDisposable> writer = this.l.WriterLockAsync();
            Assert.True(writer.IsCompletedSuccessfully);

            Task<IDisposable> reader1 = this.l.ReaderLockAsync();
            Assert.False(reader1.IsCompleted);

            Task<IDisposable> reader2 = this.l.ReaderLockAsync();
            Assert.False(reader2.IsCompleted);

            Task<IDisposable> reader3 = this.l.ReaderLockAsync();
            Assert.False(reader3.IsCompleted);

            writer.Result.Dispose();
            Assert.True(reader1.IsCompletedSuccessfully);
            Assert.True(reader2.IsCompletedSuccessfully);
            Assert.True(reader3.IsCompletedSuccessfully);
        }
    }
}
