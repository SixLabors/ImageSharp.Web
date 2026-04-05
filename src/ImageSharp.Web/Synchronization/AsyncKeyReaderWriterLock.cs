// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections.Concurrent;

namespace SixLabors.ImageSharp.Web.Synchronization;

/// <summary>
/// Extension of the <see cref="AsyncReaderWriterLock"/> that enables fine-grained locking on a given key.
/// Concurrent write lock requests using different keys can execute simultaneously, while requests to lock
/// using the same key will be forced to wait. This object is thread-safe and internally uses a pooling
/// mechanism to minimize allocation of new locks.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public class AsyncKeyReaderWriterLock<TKey>
    where TKey : notnull
{
    private readonly RefCountedConcurrentDictionary<TKey, AsyncReaderWriterLock> activeLocks;
    private readonly ConcurrentBag<AsyncReaderWriterLock> pool;
    private readonly int maxPoolSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncKeyReaderWriterLock{TKey}"/> class.
    /// </summary>
    /// <param name="maxPoolSize">The maximum number of locks that should be pooled for reuse.</param>
    public AsyncKeyReaderWriterLock(int maxPoolSize = 64)
    {
        this.pool = [];
        this.activeLocks = new RefCountedConcurrentDictionary<TKey, AsyncReaderWriterLock>(this.CreateLeasedLock, this.ReturnLeasedLock);
        this.maxPoolSize = maxPoolSize;
    }

    /// <summary>
    /// Locks the current thread in read mode asynchronously.
    /// </summary>
    /// <param name="key">The key identifying the specific object to lock against.</param>
    /// <returns>
    /// The <see cref="IDisposable"/> that will release the lock.
    /// </returns>
    public Task<IDisposable> ReaderLockAsync(TKey key)
        => this.activeLocks.Get(key).ReaderLockAsync();

    /// <summary>
    /// Locks the current thread in write mode asynchronously.
    /// </summary>
    /// <param name="key">The key identifying the specific object to lock against.</param>
    /// <returns>
    /// The <see cref="IDisposable"/> that will release the lock.
    /// </returns>
    public Task<IDisposable> WriterLockAsync(TKey key)
        => this.activeLocks.Get(key).WriterLockAsync();

    private AsyncReaderWriterLock CreateLeasedLock(TKey key)
    {
        if (!this.pool.TryTake(out AsyncReaderWriterLock? asyncLock))
        {
            asyncLock = new AsyncReaderWriterLock();
        }

        asyncLock.OnRelease = () => this.activeLocks.Release(key);
        return asyncLock;
    }

    private void ReturnLeasedLock(AsyncReaderWriterLock asyncLock)
    {
        if (this.pool.Count < this.maxPoolSize)
        {
            this.pool.Add(asyncLock);
        }
    }
}
