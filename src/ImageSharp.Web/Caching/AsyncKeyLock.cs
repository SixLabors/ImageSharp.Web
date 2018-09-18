// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// The async key lock prevents multiple asynchronous threads acting upon the same object with the given key at the same time.
    /// It is designed so that it does not block unique requests allowing a high throughput.
    /// </summary>
    internal sealed class AsyncKeyLock
    {
        /// <summary>
        /// A collection of doorman counters used for tracking references to the same key.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Doorman> Keys = new ConcurrentDictionary<string, Doorman>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Locks the current thread in read mode asynchronously.
        /// </summary>
        /// <param name="key">The key identifying the specific object to lock against.</param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public async Task<IDisposable> ReaderLockAsync(string key)
        {
            Doorman doorman = Keys.GetOrAdd(key, GetDoorman);

            return await doorman.ReaderLockAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Locks the current thread in write mode asynchronously.
        /// </summary>
        /// <param name="key">The key identifying the specific object to lock against.</param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public async Task<IDisposable> WriterLockAsync(string key)
        {
            Doorman doorman = Keys.GetOrAdd(key, GetDoorman);

            return await doorman.WriterLockAsync().ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Doorman GetDoorman(string key) => new Doorman(key, () => Keys.TryRemove(key, out Doorman localDoorman));
    }
}