// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Synchronization;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Synchronization
{
    public class AsyncLockTests
    {
        private readonly AsyncLock l = new();

        [Fact]
        public async Task OneAtATime()
        {
            Task<AsyncLock.Releaser> first = this.l.LockAsync();
            Assert.True(first.IsCompletedSuccessfully);

            Task<AsyncLock.Releaser> second = this.l.LockAsync();
            Assert.False(second.IsCompleted);

            // Release first hold on the lock and then await the second task to confirm it completes.
            first.Result.Dispose();

            // Await the second task to make sure we get the lock. The timeout specified in the [Fact] above will prevent this from running forever.
            await second;
        }
    }
}
