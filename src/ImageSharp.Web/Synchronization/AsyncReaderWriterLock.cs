// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Synchronization;

/// <summary>
/// An asynchronous locker that provides read and write locking policies.
/// <para>
/// This is based on the following blog post:
/// https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-7-asyncreaderwriterlock/
/// </para>
/// </summary>
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public class AsyncReaderWriterLock
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private readonly object stateLock;
    private readonly IDisposable writerReleaser;
    private readonly IDisposable readerReleaser;
    private readonly Task<IDisposable> writerReleaserTask;
    private readonly Task<IDisposable> readerReleaserTask;
    private readonly Queue<TaskCompletionSource<IDisposable>> waitingWriters;
    private TaskCompletionSource<IDisposable>? waitingReaders;
    private int readersWaiting;

    /// <summary>
    /// Tracks the current status of the lock:
    /// 0 == lock is unheld
    /// -1 == lock is held by a single writer
    /// >0 == lock is held by this number of concurrent readers
    /// </summary>
    private int status;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncReaderWriterLock"/> class.
    /// </summary>
    public AsyncReaderWriterLock()
    {
        this.stateLock = new object();
        this.writerReleaser = new Releaser(this, true);
        this.readerReleaser = new Releaser(this, false);
        this.writerReleaserTask = Task.FromResult(this.writerReleaser);
        this.readerReleaserTask = Task.FromResult(this.readerReleaser);
        this.waitingWriters = new Queue<TaskCompletionSource<IDisposable>>();
        this.waitingReaders = null;
        this.readersWaiting = 0;
        this.status = 0;
    }

    /// <summary>
    /// Gets or sets the callback that should be invoked whenever this lock is released.
    /// </summary>
    public Action? OnRelease { get; set; }

    /// <summary>
    /// Asynchronously obtains the lock in shared reader mode. Dispose the returned <see cref="IDisposable"/>
    /// to release the lock.
    /// </summary>
    /// <returns>
    /// The <see cref="IDisposable"/> that will release the lock.
    /// </returns>
    public Task<IDisposable> ReaderLockAsync()
    {
        lock (this.stateLock)
        {
            if (this.status >= 0 && this.waitingWriters.Count == 0)
            {
                // Lock is not held by a writer and no writers are waiting, so allow this reader to obtain
                // the lock immediately and return the pre-allocated reader releaser.
                ++this.status;
                return this.readerReleaserTask;
            }

            // This reader has to wait to obtain the lock.  Lazy instantiate the waitingReader tcs
            // and return the task.
            ++this.readersWaiting;

            this.waitingReaders ??= new TaskCompletionSource<IDisposable>(TaskCreationOptions.RunContinuationsAsynchronously);

            return this.waitingReaders.Task;
        }
    }

    /// <summary>
    /// Asynchronously obtains the lock in exclusive writer mode. Dispose the returned <see cref="IDisposable"/>
    /// to release the lock.
    /// </summary>
    /// <returns>
    /// The <see cref="IDisposable"/> that will release the lock.
    /// </returns>
    public Task<IDisposable> WriterLockAsync()
    {
        lock (this.stateLock)
        {
            if (this.status == 0)
            {
                // Lock is currently unheld, so allow this writer to obtain the lock immediately and return the
                // pre-allocated writer releaser.
                this.status = -1;
                return this.writerReleaserTask;
            }

            // This writer has to wait to obtain the lock. Create a new tcs for this writer, add it to the
            // queue of waiting writers, and return the task.
            TaskCompletionSource<IDisposable> waiter = new(TaskCreationOptions.RunContinuationsAsynchronously);
            this.waitingWriters.Enqueue(waiter);

            return waiter.Task;
        }
    }

    private void ReaderRelease()
    {
        try
        {
            TaskCompletionSource<IDisposable>? nextLockHolder = null;

            lock (this.stateLock)
            {
                --this.status;
                if (this.status == 0 && this.waitingWriters.Count > 0)
                {
                    // The lock is now unheld, and there's a writer waiting, so give the lock to the first writer in the queue.
                    this.status = -1;
                    nextLockHolder = this.waitingWriters.Dequeue();
                }
            }

            nextLockHolder?.SetResult(this.writerReleaser);
        }
        finally
        {
            this.OnRelease?.Invoke();
        }
    }

    private void WriterRelease()
    {
        try
        {
            TaskCompletionSource<IDisposable>? nextLockHolder;
            IDisposable releaser;

            lock (this.stateLock)
            {
                if (this.waitingWriters.Count > 0)
                {
                    // There's another writer waiting, so pass the lock on to the next writer.
                    nextLockHolder = this.waitingWriters.Dequeue();
                    releaser = this.writerReleaser;
                }
                else if (this.readersWaiting > 0)
                {
                    // There are readers waiting. Wake them all up at the same time since they can all concurrently
                    // hold the reader lock.
                    nextLockHolder = this.waitingReaders;
                    releaser = this.readerReleaser;
                    this.status = this.readersWaiting;
                    this.readersWaiting = 0;
                    this.waitingReaders = null;
                }
                else
                {
                    // Nobody is waiting, so the lock is now unheld.
                    this.status = 0;
                    return;
                }
            }

            nextLockHolder?.SetResult(releaser);
        }
        finally
        {
            this.OnRelease?.Invoke();
        }
    }

    /// <summary>
    /// Utility class that releases an <see cref="AsyncReaderWriterLock"/> on disposal.
    /// </summary>
    private sealed class Releaser : IDisposable
    {
        private readonly AsyncReaderWriterLock toRelease;
        private readonly bool writer;

        internal Releaser(AsyncReaderWriterLock toRelease, bool writer)
        {
            this.toRelease = toRelease;
            this.writer = writer;
        }

        /// <summary>
        /// Releases the <see cref="AsyncReaderWriterLock"/> associated with this <see cref="Releaser"/>.
        /// </summary>
        public void Dispose()
        {
            if (this.writer)
            {
                this.toRelease.WriterRelease();
            }
            else
            {
                this.toRelease.ReaderRelease();
            }
        }
    }
}
