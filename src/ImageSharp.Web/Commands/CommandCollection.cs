// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SixLabors.ImageSharp.Web.Commands
{
    /// <summary>
    /// Represents an ordered collection of processing commands.
    /// </summary>
    public sealed class CommandCollection : KeyedCollection<string, KeyValuePair<string, string>>
    {
        private readonly List<string> keys = new();

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
        /// Gets an <see cref="ICollection{T}"/> representing the keys of the collection.
        /// </summary>
        public ICollection<string> Keys => this.keys;

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

                throw new KeyNotFoundException();
            }

            set
            {
                if (this.TryGetValue(key, out KeyValuePair<string, string> item))
                {
                    this.SetItem(this.IndexOf(item), new(key, value));
                    return;
                }

                throw new KeyNotFoundException();
            }
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, KeyValuePair<string, string> item)
        {
            base.InsertItem(index, item);
            this.keys.Insert(index, item.Key);
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            this.keys.RemoveAt(index);
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, KeyValuePair<string, string> item)
        {
            base.SetItem(index, item);
            this.keys[index] = item.Key;
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            base.ClearItems();
            this.keys.Clear();
        }

        /// <inheritdoc/>
        protected override string GetKeyForItem(KeyValuePair<string, string> item) => item.Key;
    }
}
