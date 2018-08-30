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
        public async Task<IDisposable> ReaderLockAsync(string key)
        {
            Doorman doorman = null;

            doorman = Keys.GetOrAdd(key, GetDoorman);

            return await doorman.ReaderLockAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Locks the current thread asynchronously.
        /// </summary>
        /// <param name="key">The key identifying the specific object to lock against.</param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public async Task<IDisposable> WriterLockAsync(string key)
        {
            Doorman doorman = null;

            doorman = Keys.GetOrAdd(key, GetDoorman);

            return await doorman.WriterLockAsync().ConfigureAwait(false);
        }

        private static Doorman GetDoorman(string key)
        {
            return new Doorman(key, () => Keys.TryRemove(key, out Doorman localDoorman));
        }
    }
}