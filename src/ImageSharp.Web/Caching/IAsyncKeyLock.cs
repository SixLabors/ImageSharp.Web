// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Defines a contract that allows the prevention of multiple asynchronous threads acting upon the same object with the given
    /// key at the same time
    /// </summary>
    public interface IAsyncKeyLock
    {
        /// <summary>
        /// Locks the current thread asynchronously.
        /// </summary>
        /// <param name="key">The key identifying the specific object to lock against.</param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        Task<IDisposable> LockAsync(string key);
    }
}