// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace SixLabors.ImageSharp.Web.Commands
{
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value associated with the specified key or the default value.
        /// </summary>
        /// <param name="dictionary">The dictionary instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <returns>The value associated with the specified key or the default value.</returns>
        public static TValue GetValueOrDefault<TValue, TKey>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out TValue result);
            return result;
        }
    }
}