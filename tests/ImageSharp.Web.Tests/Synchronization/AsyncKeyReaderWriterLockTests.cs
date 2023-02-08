// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Synchronization;

namespace SixLabors.ImageSharp.Web.Tests.Synchronization;

public class AsyncKeyReaderWriterLockTests
{
    [Fact]
    public void OneWriterAtATime()
    {
        AsyncKeyReaderWriterLock<string> l = new();

        Task<IDisposable> first = l.WriterLockAsync("key");
        Assert.True(first.IsCompletedSuccessfully);

        Task<IDisposable> second = l.WriterLockAsync("key");
        Assert.False(second.IsCompleted);

        first.Result.Dispose();
        Assert.True(second.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task OneWriterAtATime_ByKey()
    {
        AsyncKeyReaderWriterLock<string> l = new();

        Task<IDisposable> firstFoo = l.WriterLockAsync("Foo");
        Assert.True(firstFoo.IsCompletedSuccessfully);

        Task<IDisposable> secondFoo = l.WriterLockAsync("Foo");
        Assert.False(secondFoo.IsCompleted);

        // Now take a different lock ("bar") and confirm it doesn't conflict with the previous lock
        Task<IDisposable> firstBar = l.WriterLockAsync("Bar");
        Assert.True(firstBar.IsCompletedSuccessfully);

        // Release the "bar" lock, and confirm it doesn't change anything about the original "foo" lock.
        // The second caller that was waiting on the "foo" lock should still be waiting.
        firstBar.Result.Dispose();
        Assert.False(secondFoo.IsCompleted);

        // Release the "foo" lock
        firstFoo.Result.Dispose();

        // Await the second task to make sure it gets the lock.
        await secondFoo;
    }

    [Fact]
    public void WriterBlocksReaders()
    {
        AsyncKeyReaderWriterLock<string> l = new();

        Task<IDisposable> first = l.WriterLockAsync("key");
        Assert.True(first.IsCompletedSuccessfully);

        Task<IDisposable> second = l.ReaderLockAsync("key");
        Assert.False(second.IsCompleted);

        first.Result.Dispose();
        Assert.True(second.IsCompletedSuccessfully);
    }

    [Fact]
    public void WaitingWriterBlocksReaders()
    {
        AsyncKeyReaderWriterLock<string> l = new();

        Task<IDisposable> first = l.ReaderLockAsync("key");
        Assert.True(first.IsCompletedSuccessfully);

        Task<IDisposable> second = l.WriterLockAsync("key");
        Assert.False(second.IsCompleted);

        Task<IDisposable> third = l.ReaderLockAsync("key");
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
        AsyncKeyReaderWriterLock<string> l = new();

        Task<IDisposable> first = l.ReaderLockAsync("key");
        Assert.True(first.IsCompletedSuccessfully);

        Task<IDisposable> second = l.ReaderLockAsync("key");
        Assert.True(second.IsCompletedSuccessfully);

        Task<IDisposable> third = l.ReaderLockAsync("key");
        Assert.True(third.IsCompletedSuccessfully);
    }

    [Fact]
    public void AllWaitingReadersReleasedConcurrently()
    {
        AsyncKeyReaderWriterLock<string> l = new();

        Task<IDisposable> writer = l.WriterLockAsync("key");
        Assert.True(writer.IsCompletedSuccessfully);

        Task<IDisposable> reader1 = l.ReaderLockAsync("key");
        Assert.False(reader1.IsCompleted);

        Task<IDisposable> reader2 = l.ReaderLockAsync("key");
        Assert.False(reader2.IsCompleted);

        Task<IDisposable> reader3 = l.ReaderLockAsync("key");
        Assert.False(reader3.IsCompleted);

        writer.Result.Dispose();
        Assert.True(reader1.IsCompletedSuccessfully);
        Assert.True(reader2.IsCompletedSuccessfully);
        Assert.True(reader3.IsCompletedSuccessfully);
    }
}
