// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections.Concurrent;

namespace SixLabors.ImageSharp.Web.Synchronization;

/// <summary>
/// Extension of the <see cref="AsyncLock"/> that enables fine-grained locking on a given key.
/// Concurrent lock requests using different keys can execute simultaneously, while requests to lock
/// using the same key will be forced to wait. This object is thread-safe and internally uses a pooling
/// mechanism to minimize allocation of new locks.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public sealed class AsyncKeyLock<TKey> : IDisposable
    where TKey : notnull
{
    private readonly RefCountedConcurrentDictionary<TKey, AsyncLock> activeLocks;
    private readonly ConcurrentBag<AsyncLock> pool;
    private readonly int maxPoolSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncKeyLock{TKey}"/> class.
    /// </summary>
    /// <param name="maxPoolSize">The maximum number of locks that should be pooled for reuse.</param>
    public AsyncKeyLock(int maxPoolSize = 64)
    {
        this.pool = [];
        this.activeLocks = new RefCountedConcurrentDictionary<TKey, AsyncLock>(this.CreateLeasedLock, this.ReturnLeasedLock);
        this.maxPoolSize = maxPoolSize;
    }

    /// <summary>
    /// Locks the current thread asynchronously.
    /// </summary>
    /// <param name="key">The key identifying the specific object to lock against.</param>
    /// <returns>
    /// The <see cref="IDisposable"/> that will release the lock.
    /// </returns>
    public Task<IDisposable> LockAsync(TKey key)
        => this.activeLocks.Get(key).LockAsync();

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="AsyncKeyLock{TKey}"/> class.
    /// </summary>
    public void Dispose()
    {
        // Dispose of all the locks in the pool
        while (this.pool.TryTake(out AsyncLock? asyncLock))
        {
            asyncLock.Dispose();
        }

        // activeLocks SHOULD be empty at this point so we don't need to try and clear it out.
        // If it's not empty, then this object is being disposed of prematurely!
    }

    private AsyncLock CreateLeasedLock(TKey key)
    {
        if (!this.pool.TryTake(out AsyncLock? asyncLock))
        {
            asyncLock = new AsyncLock();
        }

        asyncLock.OnRelease = () => this.activeLocks.Release(key);
        return asyncLock;
    }

    private void ReturnLeasedLock(AsyncLock asyncLock)
    {
        if (this.pool.Count < this.maxPoolSize)
        {
            this.pool.Add(asyncLock);
        }
        else
        {
            asyncLock.Dispose();
        }
    }
}
