// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web.Commands
{
    /// <summary>
    /// Represents an ordered collection of processing commands.
    /// </summary>
    public sealed class CommandCollection : KeyedCollection<string, KeyValuePair<string, string>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCollection"/> class.
        /// </summary>
        public CommandCollection()
            : this(StringComparer.OrdinalIgnoreCase)
        {
        }

        private CommandCollection(IEqualityComparer<string> comparer)
            : base(comparer)
        {
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{String}"/> representing the keys of the collection.
        /// </summary>
        public IEnumerable<string> Keys
        {
            get
            {
                foreach (KeyValuePair<string, string> item in this)
                {
                    yield return this.GetKeyForItem(item);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>
        /// The value associated with the specified key. If the specified key is not found,
        /// a get operation throws a <see cref="KeyNotFoundException"/>, and
        /// a set operation creates a new element with the specified key.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">An element with the specified key does not exist in the collection.</exception>
        public new string this[string key]
        {
            get
            {
                if (this.TryGetValue(key, out KeyValuePair<string, string> item))
                {
                    return item.Value;
                }

                ThrowKeyNotFound();
                return default;
            }

            set
            {
                if (this.TryGetValue(key, out KeyValuePair<string, string> item))
                {
                    this.SetItem(this.IndexOf(item), new(key, value));
                }
                else
                {
                    this.Add(new(key, value));
                }
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified
        /// predicate, and returns the zero-based index of the first occurrence within the
        /// entire <see cref="CommandCollection"/>.
        /// </summary>
        /// <param name="match">
        /// The <see cref="Predicate{T}"/> delegate that defines the conditions of the element to
        /// search for.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of an element that matches the conditions
        /// defined by match, if found; otherwise, -1.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int FindIndex(Predicate<string> match)
        {
            Guard.NotNull(match, nameof(match));

            int index = 0;
            foreach (KeyValuePair<string, string> item in this)
            {
                if (match(item.Key))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string GetKeyForItem(KeyValuePair<string, string> item) => item.Key;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowKeyNotFound() => throw new KeyNotFoundException();
    }
}
