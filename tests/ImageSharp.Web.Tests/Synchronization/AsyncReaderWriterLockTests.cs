// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

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
            Task<AsyncReaderWriterLock.Releaser> first = this.l.WriterLockAsync();
            Assert.True(first.IsCompletedSuccessfully);

            Task<AsyncReaderWriterLock.Releaser> second = this.l.WriterLockAsync();
            Assert.False(second.IsCompleted);

            first.Result.Dispose();
            Assert.True(second.IsCompletedSuccessfully);
        }

        [Fact]
        public void WriterBlocksReaders()
        {
            Task<AsyncReaderWriterLock.Releaser> first = this.l.WriterLockAsync();
            Assert.True(first.IsCompletedSuccessfully);

            Task<AsyncReaderWriterLock.Releaser> second = this.l.ReaderLockAsync();
            Assert.False(second.IsCompleted);

            first.Result.Dispose();
            Assert.True(second.IsCompletedSuccessfully);
        }

        [Fact]
        public void WaitingWriterBlocksReaders()
        {
            Task<AsyncReaderWriterLock.Releaser> first = this.l.ReaderLockAsync();
            Assert.True(first.IsCompletedSuccessfully);

            Task<AsyncReaderWriterLock.Releaser> second = this.l.WriterLockAsync();
            Assert.False(second.IsCompleted);

            Task<AsyncReaderWriterLock.Releaser> third = this.l.ReaderLockAsync();
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
            Task<AsyncReaderWriterLock.Releaser> first = this.l.ReaderLockAsync();
            Assert.True(first.IsCompletedSuccessfully);

            Task<AsyncReaderWriterLock.Releaser> second = this.l.ReaderLockAsync();
            Assert.True(second.IsCompletedSuccessfully);

            Task<AsyncReaderWriterLock.Releaser> third = this.l.ReaderLockAsync();
            Assert.True(third.IsCompletedSuccessfully);
        }

        [Fact]
        public void AllWaitingReadersReleasedConcurrently()
        {
            Task<AsyncReaderWriterLock.Releaser> writer = this.l.WriterLockAsync();
            Assert.True(writer.IsCompletedSuccessfully);

            Task<AsyncReaderWriterLock.Releaser> reader1 = this.l.ReaderLockAsync();
            Assert.False(reader1.IsCompleted);

            Task<AsyncReaderWriterLock.Releaser> reader2 = this.l.ReaderLockAsync();
            Assert.False(reader2.IsCompleted);

            Task<AsyncReaderWriterLock.Releaser> reader3 = this.l.ReaderLockAsync();
            Assert.False(reader3.IsCompleted);

            writer.Result.Dispose();
            Assert.True(reader1.IsCompletedSuccessfully);
            Assert.True(reader2.IsCompletedSuccessfully);
            Assert.True(reader3.IsCompletedSuccessfully);
        }
    }
}
