// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Synchronization;

namespace SixLabors.ImageSharp.Web.Tests.Synchronization;

public class AsyncKeyLockTests
{
    [Fact]
    public async Task OneAtATime()
    {
        AsyncKeyLock<string> l = new();

        Task<IDisposable> first = l.LockAsync("key");
        Assert.True(first.IsCompletedSuccessfully);

        Task<IDisposable> second = l.LockAsync("key");
        Assert.False(second.IsCompleted);

        // Release first hold on the lock
        first.Result.Dispose();

        // Await the second task to make sure we get the lock.
        await second;

        l.Dispose();
    }

    [Fact]
    public async Task OneAtATime_ByKey()
    {
        AsyncKeyLock<string> l = new();

        Task<IDisposable> firstFoo = l.LockAsync("Foo");
        Assert.True(firstFoo.IsCompletedSuccessfully);

        Task<IDisposable> secondFoo = l.LockAsync("Foo");
        Assert.False(secondFoo.IsCompleted);

        // Now take a different lock ("bar") and confirm it doesn't conflict with the previous lock
        Task<IDisposable> firstBar = l.LockAsync("Bar");
        Assert.True(firstBar.IsCompletedSuccessfully);

        // Release the "bar" lock, and confirm it doesn't change anything about the original "foo" lock.
        // The second caller that was waiting on the "foo" lock should still be waiting.
        firstBar.Result.Dispose();
        Assert.False(secondFoo.IsCompleted);

        // Release the "foo" lock
        firstFoo.Result.Dispose();

        // Await the second task to make sure it gets the lock.
        await secondFoo;

        l.Dispose();
    }
}
