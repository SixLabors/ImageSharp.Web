// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections;
using System.Collections.Concurrent;

namespace SixLabors.ImageSharp.Web.Synchronization;

/// <summary>
/// Represents a thread-safe collection of reference-counted key/value pairs that can be accessed by multiple
/// threads concurrently. Values that don't yet exist are automatically created using a caller supplied
/// value factory method, and when their final refcount is released they are removed from the dictionary.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The value for the dictionary.</typeparam>
public class RefCountedConcurrentDictionary<TKey, TValue>
    where TKey : notnull
    where TValue : class
{
    private readonly ConcurrentDictionary<TKey, RefCountedValue> dictionary;
    private readonly Func<TKey, TValue> valueFactory;
    private readonly Action<TValue>? valueReleaser;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefCountedConcurrentDictionary{TKey, TValue}"/> class that is empty,
    /// has the default concurrency level, has the default initial capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="valueFactory">Factory method that generates a new <typeparamref name="TValue"/> for a given <typeparamref name="TKey"/>.</param>
    /// <param name="valueReleaser">Optional callback that is used to cleanup <typeparamref name="TValue"/>s after their final ref count is released.</param>
    public RefCountedConcurrentDictionary(Func<TKey, TValue> valueFactory, Action<TValue>? valueReleaser = null)
        : this(new ConcurrentDictionary<TKey, RefCountedValue>(), valueFactory, valueReleaser)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefCountedConcurrentDictionary{TKey, TValue}"/> class that is empty,
    /// has the default concurrency level and capacity,, and uses the specified  <see cref="IEqualityComparer{TKey}"/>.
    /// </summary>
    /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/> implementation to use when comparing keys.</param>
    /// <param name="valueFactory">Factory method that generates a new <typeparamref name="TValue"/> for a given <typeparamref name="TKey"/>.</param>
    /// <param name="valueReleaser">Optional callback that is used to cleanup <typeparamref name="TValue"/>s after their final ref count is released.</param>
    public RefCountedConcurrentDictionary(IEqualityComparer<TKey> comparer, Func<TKey, TValue> valueFactory, Action<TValue>? valueReleaser)
        : this(new ConcurrentDictionary<TKey, RefCountedValue>(comparer), valueFactory, valueReleaser)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefCountedConcurrentDictionary{TKey, TValue}"/> class that is empty,
    /// has the specified concurrency level and capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="concurrencyLevel">The estimated number of threads that will access the <see cref="RefCountedConcurrentDictionary{TKey, TValue}"/> concurrently</param>
    /// <param name="capacity">The initial number of elements that the <see cref="RefCountedConcurrentDictionary{TKey, TValue}"/> can contain.</param>
    /// <param name="valueFactory">Factory method that generates a new <typeparamref name="TValue"/> for a given <typeparamref name="TKey"/>.</param>
    /// <param name="valueReleaser">Optional callback that is used to cleanup <typeparamref name="TValue"/>s after their final ref count is released.</param>
    public RefCountedConcurrentDictionary(int concurrencyLevel, int capacity, Func<TKey, TValue> valueFactory, Action<TValue>? valueReleaser = null)
        : this(new ConcurrentDictionary<TKey, RefCountedValue>(concurrencyLevel, capacity), valueFactory, valueReleaser)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefCountedConcurrentDictionary{TKey, TValue}"/> class that is empty,
    /// has the specified concurrency level, has the specified initial capacity, and uses the specified
    /// <see cref="IEqualityComparer{TKey}"/>.
    /// </summary>
    /// <param name="concurrencyLevel">The estimated number of threads that will access the <see cref="RefCountedConcurrentDictionary{TKey, TValue}"/> concurrently</param>
    /// <param name="capacity">The initial number of elements that the <see cref="RefCountedConcurrentDictionary{TKey, TValue}"/> can contain.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/> implementation to use when comparing keys.</param>
    /// <param name="valueFactory">Factory method that generates a new <typeparamref name="TValue"/> for a given <typeparamref name="TKey"/>.</param>
    /// <param name="valueReleaser">Optional callback that is used to cleanup <typeparamref name="TValue"/>s after their final ref count is released.</param>
    public RefCountedConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer, Func<TKey, TValue> valueFactory, Action<TValue>? valueReleaser)
        : this(new ConcurrentDictionary<TKey, RefCountedValue>(concurrencyLevel, capacity, comparer), valueFactory, valueReleaser)
    {
    }

    private RefCountedConcurrentDictionary(ConcurrentDictionary<TKey, RefCountedValue> dictionary, Func<TKey, TValue> valueFactory, Action<TValue>? valueReleaser)
    {
        Guard.NotNull(valueFactory, nameof(valueFactory));

        this.dictionary = dictionary;
        this.valueFactory = valueFactory;
        this.valueReleaser = valueReleaser;
    }

    /// <summary>
    /// Obtains a reference to the value corresponding to the specified key. If no such value exists in the
    /// dictionary, then a new value is generated using the value factory method supplied in the constructor.
    /// To prevent leaks, this reference MUST be released via <see cref="Release(TKey)"/>.
    /// </summary>
    /// <param name="key">The key of the element to add ref.</param>
    /// <returns>The referenced object.</returns>
    public TValue Get(TKey key)
    {
        while (true)
        {
            if (this.dictionary.TryGetValue(key, out RefCountedValue? refCountedValue))
            {
                // Increment ref count
                if (this.dictionary.TryUpdate(key, new RefCountedValue(refCountedValue.Value, refCountedValue.RefCount + 1), refCountedValue))
                {
                    return refCountedValue.Value;
                }
            }
            else
            {
                // Add new value to dictionary
                TValue value = this.valueFactory(key);

                if (this.dictionary.TryAdd(key, new RefCountedValue(value, 1)))
                {
                    return value;
                }
                else
                {
                    this.valueReleaser?.Invoke(value);
                }
            }
        }
    }

    /// <summary>
    /// Releases a reference to the value corresponding to the specified key. If this reference was the last
    /// remaining reference to the value, then the value is removed from the dictionary, and the optional value
    /// releaser callback is invoked.
    /// </summary>
    /// <param name="key">THe key of the element to release.</param>
    public void Release(TKey key)
    {
        while (true)
        {
            if (!this.dictionary.TryGetValue(key, out RefCountedValue? refCountedValue))
            {
                // This is BAD. It indicates a ref counting problem where someone is either double-releasing,
                // or they're releasing a key that they never obtained in the first place!!
                throw new InvalidOperationException($"Tried to release value that doesn't exist in the dictionary ({key})!");
            }

            // If we're releasing the last reference, then try to remove the value from the dictionary.
            // Otherwise, try to decrement the reference count.
            if (refCountedValue.RefCount == 1)
            {
                // Remove from dictionary.  We use the ICollection<>.Remove() method instead of the ConcurrentDictionary.TryRemove()
                // because this specific API will only succeed if the value hasn't been changed by another thread.
                if (((ICollection<KeyValuePair<TKey, RefCountedValue>>)this.dictionary).Remove(new KeyValuePair<TKey, RefCountedValue>(key, refCountedValue)))
                {
                    this.valueReleaser?.Invoke(refCountedValue.Value);
                    return;
                }
            }
            else
            {
                // Decrement ref count
                if (this.dictionary.TryUpdate(key, new RefCountedValue(refCountedValue.Value, refCountedValue.RefCount - 1), refCountedValue))
                {
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Get an enumeration over the contents of the dictionary for testing/debugging purposes
    /// </summary>
    internal IEnumerable<(TKey Key, TValue Value, int RefCount)> DebugGetContents() => new RefCountedDictionaryEnumerable(this);

    /// <summary>
    /// Internal class used for testing/debugging purposes
    /// </summary>
    private class RefCountedDictionaryEnumerable : IEnumerable<(TKey Key, TValue Value, int RefCount)>
    {
        private readonly RefCountedConcurrentDictionary<TKey, TValue> dictionary;

        internal RefCountedDictionaryEnumerable(RefCountedConcurrentDictionary<TKey, TValue> dictionary)
            => this.dictionary = dictionary;

        public IEnumerator<(TKey Key, TValue Value, int RefCount)> GetEnumerator()
            => new RefCountedDictionaryEnumerator(this.dictionary);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    /// <summary>
    /// Internal class used for testing/debugging purposes
    /// </summary>
    private class RefCountedDictionaryEnumerator : IEnumerator<(TKey Key, TValue Value, int RefCount)>
    {
        private readonly IEnumerator<KeyValuePair<TKey, RefCountedValue>> enumerator;

        public RefCountedDictionaryEnumerator(RefCountedConcurrentDictionary<TKey, TValue> dictionary)
            => this.enumerator = dictionary.dictionary.GetEnumerator();

        public (TKey Key, TValue Value, int RefCount) Current
        {
            get
            {
                KeyValuePair<TKey, RefCountedValue> keyValuePair = this.enumerator.Current;
                return (keyValuePair.Key, keyValuePair.Value.Value, keyValuePair.Value.RefCount);
            }
        }

        object IEnumerator.Current => this.Current;

        public void Dispose() => this.enumerator.Dispose();

        public bool MoveNext() => this.enumerator.MoveNext();

        public void Reset() => this.enumerator.Reset();
    }

    /// <summary>
    /// Simple immutable tuple that combines a <typeparamref name="TValue"/> instance with a ref count integer.
    /// </summary>
    private class RefCountedValue : IEquatable<RefCountedValue>
    {
#pragma warning disable SA1401 // Fields should be private
        public readonly TValue Value;
        public readonly int RefCount;
#pragma warning restore SA1401 // Fields should be private

        public RefCountedValue(TValue value, int refCount)
        {
            this.Value = value;
            this.RefCount = refCount;
        }

        public bool Equals(
#if NET5_0_OR_GREATER
            RefCountedValue? other)
#else
            [System.Diagnostics.CodeAnalysis.AllowNull] RefCountedValue other)
#endif
            => (other != null) && (this.RefCount == other.RefCount) && EqualityComparer<TValue>.Default.Equals(this.Value, other.Value);

        public override bool Equals(object? obj)
            => (obj is RefCountedValue other) && this.Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(this.RefCount, this.Value);
    }
}
