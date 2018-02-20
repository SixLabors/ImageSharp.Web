// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Concurrent;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Provides a resource pool that enables reusing instances of <see cref="Doorman"/>
    /// </summary>
    internal static class DoormanPool
    {
        private static readonly ConcurrentBag<Doorman> Pool = new ConcurrentBag<Doorman>();

        /// <summary>
        /// Retrieves a <see cref="Doorman"/> from the pool or a new one if the pool is empty
        /// </summary>
        /// <returns>Tre <see cref="Doorman"/></returns>
        public static Doorman Rent() => Pool.TryTake(out Doorman doorman) ? doorman : new Doorman();

        /// <summary>
        /// Returns an doorman to the pool that was previously obtained using the <see cref="Rent"></see>
        /// method on the same <see cref="DoormanPool"></see> instance.
        /// </summary>
        /// <param name="doorman">The doorman to return</param>
        public static void Return(Doorman doorman)
        {
            Guard.NotNull(doorman, nameof(doorman));
            Pool.Add(doorman);
        }

        /// <summary>
        /// Gets the number of items contained within the pool
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public static int Count() => Pool.Count;
    }
}
