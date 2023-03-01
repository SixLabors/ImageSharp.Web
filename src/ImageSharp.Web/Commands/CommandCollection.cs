// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web.Commands;

/// <summary>
/// Represents an ordered collection of processing commands.
/// </summary>
public sealed class CommandCollection : KeyedCollection<string, KeyValuePair<string, string?>>
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
            foreach (KeyValuePair<string, string?> item in this)
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
            if (!this.TryGetValue(key, out string? value))
            {
                ThrowKeyNotFound();
            }

            return value;
        }

        set
        {
            if (this.TryGetValue(key, out KeyValuePair<string, string?> item))
            {
                this.SetItem(this.IndexOf(item), new(key, value));
            }
            else
            {
                this.Add(key, value);
            }
        }
    }

    /// <summary>
    /// Adds an element with the provided key and value to the <see cref="CommandCollection"/>.
    /// </summary>
    /// <param name="key">The <see cref="string"/> to use as the key of the element to add.</param>
    /// <param name="value">The <see cref="string"/> to use as the value of the element to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    public void Add(string key, string value) => this.Add(new(key, value));

    /// <summary>
    /// Inserts an element into the <see cref="CommandCollection"/> at the
    /// specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="key">The <see cref="string"/> to use as the key of the element to insert.</param>
    /// <param name="value">The <see cref="string"/> to use as the value of the element to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException">index is less than zero. -or- index is greater than <see cref="P:CommandCollection.Count"/>.</exception>
    public void Insert(int index, string key, string value) => this.Insert(index, new(key, value));

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">
    /// When this method returns, the value associated with the specified key, if the
    /// key is found; otherwise, the default value for the type of the value parameter.
    /// This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the object that implements <see cref="CommandCollection"/> contains
    /// an element with the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        if (this.TryGetValue(key, out KeyValuePair<string, string?> keyValue))
        {
            value = keyValue.Value;
            return value is not null;
        }

        value = default;
        return false;
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
        foreach (KeyValuePair<string, string?> item in this)
        {
            if (match(item.Key))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    /// <summary>
    /// Searches for the specified key and returns the zero-based index of the first
    /// occurrence within the entire <see cref="CommandCollection"/>.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="CommandCollection"/>.</param>
    /// <returns>
    /// The zero-based index of the first occurrence of key within the entire <see cref="CommandCollection"/>,
    /// if found; otherwise, -1.
    /// </returns>
    public int IndexOf(string key)
    {
        if (this.TryGetValue(key, out KeyValuePair<string, string?> item))
        {
            return this.IndexOf(item);
        }

        return -1;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override string GetKeyForItem(KeyValuePair<string, string?> item) => item.Key;

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowKeyNotFound() => throw new KeyNotFoundException();
}
