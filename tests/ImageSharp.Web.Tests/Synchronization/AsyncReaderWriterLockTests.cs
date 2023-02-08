// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Synchronization;

namespace SixLabors.ImageSharp.Web.Tests.Synchronization;

public class AsyncReaderWriterLockTests
{
    [Fact]
    public void OneWriterAtATime()
    {
        AsyncReaderWriterLock l = new();

        Task<IDisposable> first = l.WriterLockAsync();
        Assert.True(first.IsCompletedSuccessfully);

        Task<IDisposable> second = l.WriterLockAsync();
        Assert.False(second.IsCompleted);

        first.Result.Dispose();
        Assert.True(second.IsCompletedSuccessfully);
    }

    [Fact]
    public void WriterBlocksReaders()
    {
        AsyncReaderWriterLock l = new();

        Task<IDisposable> first = l.WriterLockAsync();
        Assert.True(first.IsCompletedSuccessfully);

        Task<IDisposable> second = l.ReaderLockAsync();
        Assert.False(second.IsCompleted);

        first.Result.Dispose();
        Assert.True(second.IsCompletedSuccessfully);
    }

    [Fact]
    public void WaitingWriterBlocksReaders()
    {
        AsyncReaderWriterLock l = new();

        Task<IDisposable> first = l.ReaderLockAsync();
        Assert.True(first.IsCompletedSuccessfully);

        Task<IDisposable> second = l.WriterLockAsync();
        Assert.False(second.IsCompleted);

        Task<IDisposable> third = l.ReaderLockAsync();
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
        AsyncReaderWriterLock l = new();

        Task<IDisposable> first = l.ReaderLockAsync();
        Assert.True(first.IsCompletedSuccessfully);

        Task<IDisposable> second = l.ReaderLockAsync();
        Assert.True(second.IsCompletedSuccessfully);

        Task<IDisposable> third = l.ReaderLockAsync();
        Assert.True(third.IsCompletedSuccessfully);
    }

    [Fact]
    public void AllWaitingReadersReleasedConcurrently()
    {
        AsyncReaderWriterLock l = new();

        Task<IDisposable> writer = l.WriterLockAsync();
        Assert.True(writer.IsCompletedSuccessfully);

        Task<IDisposable> reader1 = l.ReaderLockAsync();
        Assert.False(reader1.IsCompleted);

        Task<IDisposable> reader2 = l.ReaderLockAsync();
        Assert.False(reader2.IsCompleted);

        Task<IDisposable> reader3 = l.ReaderLockAsync();
        Assert.False(reader3.IsCompleted);

        writer.Result.Dispose();
        Assert.True(reader1.IsCompletedSuccessfully);
        Assert.True(reader2.IsCompletedSuccessfully);
        Assert.True(reader3.IsCompletedSuccessfully);
    }
}
