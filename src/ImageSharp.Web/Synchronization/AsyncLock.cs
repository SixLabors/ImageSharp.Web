// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Synchronization;

/// <summary>
/// An asynchronous locker that uses an IDisposable pattern for releasing the lock.
/// </summary>
public sealed class AsyncLock : IDisposable
{
    private readonly SemaphoreSlim semaphore;
    private readonly IDisposable releaser;
    private readonly Task<IDisposable> releaserTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLock"/> class.
    /// </summary>
    public AsyncLock()
    {
        this.semaphore = new SemaphoreSlim(1, 1);
        this.releaser = new Releaser(this);
        this.releaserTask = Task.FromResult(this.releaser);
    }

    /// <summary>
    /// Gets or sets the callback that should be invoked whenever this lock is released.
    /// </summary>
    public Action? OnRelease { get; set; }

    /// <summary>
    /// Asynchronously obtains the lock. Dispose the returned <see cref="IDisposable"/> to release the lock.
    /// </summary>
    /// <returns>
    /// The <see cref="IDisposable"/> that will release the lock.
    /// </returns>
    public Task<IDisposable> LockAsync()
    {
        Task wait = this.semaphore.WaitAsync();

        // No-allocation fast path when the semaphore wait completed synchronously
        return (wait.Status == TaskStatus.RanToCompletion)
            ? this.releaserTask
            : AwaitThenReturn(wait, this.releaser);

        static async Task<IDisposable> AwaitThenReturn(Task t, IDisposable r)
        {
            await t;
            return r;
        }
    }

    private void Release()
    {
        try
        {
            this.semaphore.Release();
        }
        finally
        {
            this.OnRelease?.Invoke();
        }
    }

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="AsyncLock"/> class.
    /// </summary>
    public void Dispose() => this.semaphore.Dispose();

    /// <summary>
    /// Utility class that releases an <see cref="AsyncLock"/> on disposal.
    /// </summary>
    private sealed class Releaser : IDisposable
    {
        private readonly AsyncLock toRelease;

        internal Releaser(AsyncLock toRelease) => this.toRelease = toRelease;

        /// <summary>
        /// Releases the <see cref="AsyncLock"/> associated with this <see cref="Releaser"/>.
        /// </summary>
        public void Dispose() => this.toRelease?.Release();
    }
}
