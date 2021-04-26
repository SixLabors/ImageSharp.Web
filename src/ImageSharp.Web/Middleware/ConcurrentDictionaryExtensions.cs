// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SixLabors.ImageSharp.Web.Middleware
{
    /// <summary>
    /// Extensions used to manage asynchronous access to the <see cref="ImageSharpMiddleware"/>
    /// https://gist.github.com/davidfowl/3dac8f7b3d141ae87abf770d5781feed
    /// </summary>
    public static class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// Provides an alternative to <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/> specifically for asynchronous values. The factory method will only run once.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The value for the dictionary.</typeparam>
        /// <param name="dictionary">The <see cref="ConcurrentDictionary{TKey, TValue}"/>.</param>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public static async Task<TValue> GetOrAddAsync<TKey, TValue>(
            this ConcurrentDictionary<TKey, Task<TValue>> dictionary,
            TKey key,
            Func<TKey, Task<TValue>> valueFactory)
        {
            while (true)
            {
                if (dictionary.TryGetValue(key, out var task))
                {
                    return await task;
                }

                // This is the task that we'll return to all waiters. We'll complete it when the factory is complete
                var tcs = new TaskCompletionSource<TValue>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (dictionary.TryAdd(key, tcs.Task))
                {
                    try
                    {
                        var value = await valueFactory(key);
                        tcs.TrySetResult(value);
                        return await tcs.Task;
                    }
                    catch (Exception ex)
                    {
                        // Make sure all waiters see the exception
                        tcs.SetException(ex);

                        // We remove the entry if the factory failed so it's not a permanent failure
                        // and future gets can retry (this could be a pluggable policy)
                        dictionary.TryRemove(key, out _);
                        throw;
                    }
                }
            }
        }
    }
}