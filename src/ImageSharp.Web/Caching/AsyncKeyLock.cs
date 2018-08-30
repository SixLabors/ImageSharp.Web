// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// The async key lock prevents multiple asynchronous threads acting upon the same object with the given key at the same time.
    /// It is designed so that it does not block unique requests allowing a high throughput.
    /// </summary>
    public sealed class AsyncKeyLock : IAsyncKeyLock
    {
        /// <summary>
        /// A collection of doorman counters used for tracking references to the same key.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Doorman> Keys = new ConcurrentDictionary<string, Doorman>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Locks the current thread asynchronously.
        /// </summary>
        /// <param name="key">The key identifying the specific object to lock against.</param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public async Task<IDisposable> LockAsync(string key)
        {
            Doorman doorman = null;

            do
            {
                doorman = Keys.GetOrAdd(key, GetDoorman);
            }
            while (!doorman.TryAcquire());

            await doorman.Semaphore.WaitAsync().ConfigureAwait(false);

            return new Releaser(doorman, key);
        }

        private static Doorman GetDoorman(string key)
        {
            return DoormanPool.Rent();
        }

        /// <summary>
        /// The disposable releaser tasked with releasing the semaphore.
        /// </summary>
        private sealed class Releaser : IDisposable
        {
            /// <summary>
            /// The key identifying the <see cref="Doorman"/> that limits the number of threads.
            /// </summary>
            private readonly string key;

            /// <summary>
            /// The <see cref="Doorman"/> that limits the number of threads.
            /// </summary>
            private readonly Doorman doorman;

            /// <summary>
            /// Initializes a new instance of the <see cref="Releaser"/> class.
            /// </summary>
            /// <param name="doorman">The doorman that limits the number of threads.</param>
            /// <param name="key">The key identifying the doorman that limits the number of threads.</param>
            public Releaser(Doorman doorman, string key)
            {
                this.doorman = doorman;
                this.key = key;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                // Release the semaphore as soon as we can
                this.doorman.Semaphore.Release();

                // If there is no more reference to it we can return it to the pool
                if (this.doorman.Release())
                {
                    Keys.TryRemove(this.key, out Doorman localDoorman);
                    this.doorman.Reset();
                    DoormanPool.Return(this.doorman);
                }
            }
        }
    }
}