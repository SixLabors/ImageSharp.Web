// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// A wrapper around <see cref="SemaphoreSlim"/> that operates a one-in-one out policy.
    /// </summary>
    internal sealed class Doorman : IDisposable
    {
        private volatile int refCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Doorman"/> class.
        /// </summary>
        public Doorman()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Gets the number of references to this doorman.
        /// </summary>
        public int RefCount => this.refCount;

        /// <summary>
        /// Gets the SemaphoreSlim that performs the limiting.
        /// </summary>
        public SemaphoreSlim Semaphore { get; }

        /// <summary>
        /// Removes a reference to this doorman.
        /// </summary>
        /// <returns><c>true</c> if the doorman is not used anymore.</returns>
        public bool Release()
        {
            // We use -1 as a way to prevent any other thread from acquiring it successfully.
            if (Interlocked.CompareExchange(ref this.refCount, -1, 1) == 1)
            {
                return true;
            }
            else
            {
                Interlocked.Decrement(ref this.refCount);

                return false;
            }
        }

        /// <summary>
        /// Tries to add a reference to this doorman.
        /// </summary>
        /// <returns><c>true</c> if it was acquired.</returns>
        public bool TryAcquire()
        {
            // If the doorman is being released then it
            if (Interlocked.Increment(ref this.refCount) > 0)
            {
                return true;
            }

            return false;
        }

        public void Reset()
        {
            Interlocked.Exchange(ref this.refCount, 1);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Semaphore.Dispose();
        }
    }
}
