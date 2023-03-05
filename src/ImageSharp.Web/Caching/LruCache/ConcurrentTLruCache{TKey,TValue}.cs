// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web.Caching;

/// <summary>
/// Pseudo LRU implementation where LRU list is composed of 3 segments: hot, warm and cold. Cost of maintaining
/// segments is amortized across requests. Items are only cycled when capacity is exceeded. Pure read does
/// not cycle items if all segments are within capacity constraints.
/// There are no global locks. On cache miss, a new item is added. Tail items in each segment are dequeued,
/// examined, and are either enqueued or discarded.
/// This scheme of hot, warm and cold is based on the implementation used in MemCached described online here:
/// https://memcached.org/blog/modern-lru/
/// </summary>
/// <remarks>
/// Each segment has a capacity. When segment capacity is exceeded, items are moved as follows:
/// 1. New items are added to hot, WasAccessed = false
/// 2. When items are accessed, update WasAccessed = true
/// 3. When items are moved WasAccessed is set to false.
/// 4. When hot is full, hot tail is moved to either Warm or Cold depending on WasAccessed.
/// 5. When warm is full, warm tail is moved to warm head or cold depending on WasAccessed.
/// 6. When cold is full, cold tail is moved to warm head or removed from dictionary on depending on WasAccessed.
/// </remarks>
internal class ConcurrentTLruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, LongTickCountLruItem<TKey, TValue>> dictionary;

    private readonly ConcurrentQueue<LongTickCountLruItem<TKey, TValue>> hotQueue;
    private readonly ConcurrentQueue<LongTickCountLruItem<TKey, TValue>> warmQueue;
    private readonly ConcurrentQueue<LongTickCountLruItem<TKey, TValue>> coldQueue;

    // Maintain count outside ConcurrentQueue, since ConcurrentQueue.Count holds a global lock
    private int hotCount;
    private int warmCount;
    private int coldCount;

    private readonly int hotCapacity;
    private readonly int warmCapacity;
    private readonly int coldCapacity;

    private readonly TLruLongTicksPolicy<TKey, TValue> policy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentTLruCache{K, V}"/> class with the specified capacity and time to live that has the default.
    /// concurrency level, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="capacity">The maximum number of elements that the FastConcurrentTLru can contain.</param>
    /// <param name="timeToLive">The time to live for cached values.</param>
    public ConcurrentTLruCache(int capacity, TimeSpan timeToLive)
        : this(Environment.ProcessorCount, capacity, EqualityComparer<TKey>.Default, timeToLive)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentTLruCache{K, V}"/> class that has the specified concurrency level, has the.
    /// specified initial capacity, uses the specified <see cref="IEqualityComparer{T}"/>, and has the specified time to live.
    /// </summary>
    /// <param name="concurrencyLevel">The estimated number of threads that will update the ConcurrentTLru concurrently.</param>
    /// <param name="capacity">The maximum number of elements that the ConcurrentTLru can contain.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys.</param>
    /// <param name="timeToLive">The time to live for cached values.</param>
    public ConcurrentTLruCache(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer, TimeSpan timeToLive)
    {
        Guard.MustBeGreaterThanOrEqualTo(capacity, 3, nameof(capacity));
        Guard.NotNull(comparer, nameof(comparer));

        this.hotCapacity = capacity / 3;
        this.warmCapacity = capacity / 3;
        this.coldCapacity = capacity / 3;

        this.hotQueue = new ConcurrentQueue<LongTickCountLruItem<TKey, TValue>>();
        this.warmQueue = new ConcurrentQueue<LongTickCountLruItem<TKey, TValue>>();
        this.coldQueue = new ConcurrentQueue<LongTickCountLruItem<TKey, TValue>>();

        int dictionaryCapacity = this.hotCapacity + this.warmCapacity + this.coldCapacity + 1;

        this.dictionary = new ConcurrentDictionary<TKey, LongTickCountLruItem<TKey, TValue>>(concurrencyLevel, dictionaryCapacity, comparer);
        this.policy = new TLruLongTicksPolicy<TKey, TValue>(timeToLive);
    }

    // No lock count: https://arbel.net/2013/02/03/best-practices-for-using-concurrentdictionary/
    public int Count => this.dictionary.Skip(0).Count();

    public int HotCount => this.hotCount;

    public int WarmCount => this.warmCount;

    public int ColdCount => this.coldCount;

    /// <summary>
    /// Attempts to get the value associated with the specified key from the cache.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the object from the cache that has the specified key, or the default value of the type if the operation failed.</param>
    /// <returns><see langword="true"/> if the key was found in the cache; otherwise, <see langword="false"/>.</returns>
    public bool TryGet(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        if (this.dictionary.TryGetValue(key, out LongTickCountLruItem<TKey, TValue>? item))
        {
            return this.GetOrDiscard(item, out value);
        }

        value = default;
        return false;
    }

    // AggressiveInlining forces the JIT to inline policy.ShouldDiscard(). For LRU policy
    // the first branch is completely eliminated due to JIT time constant propogation.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetOrDiscard(LongTickCountLruItem<TKey, TValue> item, out TValue? value)
    {
        if (this.policy.ShouldDiscard(item))
        {
            this.Move(item, ItemDestination.Remove);
            value = default;
            return false;
        }

        value = item.Value;
        TLruLongTicksPolicy<TKey, TValue>.Touch(item);
        return true;
    }

    /// <summary>
    /// Adds a key/value pair to the cache if the key does not already exist. Returns the new value, or the
    /// existing value if the key already exists.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="valueFactory">The factory function used to generate a value for the key.</param>
    /// <returns>The value for the key. This will be either the existing value for the key if the key is already
    /// in the cache, or the new value if the key was not in the dictionary.</returns>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (this.TryGet(key, out TValue? value))
        {
            return value;
        }

        // The value factory may be called concurrently for the same key, but the first write to the dictionary wins.
        // This is identical logic in ConcurrentDictionary.GetOrAdd method.
        LongTickCountLruItem<TKey, TValue> newItem = TLruLongTicksPolicy<TKey, TValue>.CreateItem(key, valueFactory(key));

        if (this.dictionary.TryAdd(key, newItem))
        {
            this.hotQueue.Enqueue(newItem);
            Interlocked.Increment(ref this.hotCount);
            this.Cycle();
            return newItem.Value;
        }

        return this.GetOrAdd(key, valueFactory);
    }

    /// <summary>
    /// Adds a key/value pair to the cache if the key does not already exist. Returns the new value, or the
    /// existing value if the key already exists.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="valueFactory">The factory function used to asynchronously generate a value for the key.</param>
    /// <returns>A task that represents the asynchronous <see cref="GetOrAdd(TKey, Func{TKey, TValue})"/> operation.</returns>
    public async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> valueFactory)
    {
        if (this.TryGet(key, out TValue? value))
        {
            return value;
        }

        // The value factory may be called concurrently for the same key, but the first write to the dictionary wins.
        // This is identical logic in ConcurrentDictionary.GetOrAdd method.
        LongTickCountLruItem<TKey, TValue> newItem = TLruLongTicksPolicy<TKey, TValue>.CreateItem(key, await valueFactory(key).ConfigureAwait(false));

        if (this.dictionary.TryAdd(key, newItem))
        {
            this.hotQueue.Enqueue(newItem);
            Interlocked.Increment(ref this.hotCount);
            this.Cycle();
            return newItem.Value;
        }

        return await this.GetOrAddAsync(key, valueFactory).ConfigureAwait(false);
    }

    /// <summary>
    /// Attempts to remove and return the value that has the specified key from the cache.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns><see langword="true"/> if the object was removed successfully; otherwise, <see langword="false"/>.</returns>
    public bool TryRemove(TKey key)
    {
        // Possible race condition:
        // Thread A TryRemove(1), removes LruItem1, has reference to removed item but not yet marked as removed
        // Thread B GetOrAdd(1) => Adds LruItem1*
        // Thread C GetOrAdd(2), Cycle, Move(LruItem1, Removed)
        //
        // Thread C can run and remove LruItem1* from this.dictionary before Thread A has marked LruItem1 as removed.
        //
        // In this situation, a subsequent attempt to fetch 1 will be a miss. The queues will still contain LruItem1*,
        // and it will not be marked as removed. If key 1 is fetched while LruItem1* is still in the queue, there will
        // be two queue entries for key 1, and neither is marked as removed. Thus when LruItem1 * ages out, it will
        // incorrectly remove 1 from the dictionary, and this cycle can repeat.
        if (this.dictionary.TryGetValue(key, out LongTickCountLruItem<TKey, TValue>? existing))
        {
            if (existing.WasRemoved)
            {
                return false;
            }

            lock (existing)
            {
                if (existing.WasRemoved)
                {
                    return false;
                }

                existing.WasRemoved = true;
            }

            if (this.dictionary.TryRemove(key, out LongTickCountLruItem<TKey, TValue>? removedItem))
            {
                // Mark as not accessed, it will later be cycled out of the queues because it can never be fetched
                // from the dictionary. Note: Hot/Warm/Cold count will reflect the removed item until it is cycled
                // from the queue.
                removedItem.WasAccessed = false;

                if (removedItem.Value is IDisposable d)
                {
                    d.Dispose();
                }

                return true;
            }
        }

        return false;
    }

    private void Cycle()
    {
        // There will be races when queue count == queue capacity. Two threads may each dequeue items.
        // This will prematurely free slots for the next caller. Each thread will still only cycle at most 5 items.
        // Since TryDequeue is thread safe, only 1 thread can dequeue each item. Thus counts and queue state will always
        // converge on correct over time.
        this.CycleHot();

        // Multi-threaded stress tests show that due to races, the warm and cold count can increase beyond capacity when
        // hit rate is very high. Double cycle results in stable count under all conditions. When contention is low,
        // secondary cycles have no effect.
        this.CycleWarm();
        this.CycleWarm();
        this.CycleCold();
        this.CycleCold();
    }

    private void CycleHot()
    {
        if (this.hotCount > this.hotCapacity)
        {
            Interlocked.Decrement(ref this.hotCount);

            if (this.hotQueue.TryDequeue(out LongTickCountLruItem<TKey, TValue>? item))
            {
                ItemDestination where = this.policy.RouteHot(item);
                this.Move(item, where);
            }
            else
            {
                Interlocked.Increment(ref this.hotCount);
            }
        }
    }

    private void CycleWarm()
    {
        if (this.warmCount > this.warmCapacity)
        {
            Interlocked.Decrement(ref this.warmCount);

            if (this.warmQueue.TryDequeue(out LongTickCountLruItem<TKey, TValue>? item))
            {
                ItemDestination where = this.policy.RouteWarm(item);

                // When the warm queue is full, we allow an overflow of 1 item before redirecting warm items to cold.
                // This only happens when hit rate is high, in which case we can consider all items relatively equal in
                // terms of which was least recently used.
                if (where == ItemDestination.Warm && this.warmCount <= this.warmCapacity)
                {
                    this.Move(item, where);
                }
                else
                {
                    this.Move(item, ItemDestination.Cold);
                }
            }
            else
            {
                Interlocked.Increment(ref this.warmCount);
            }
        }
    }

    private void CycleCold()
    {
        if (this.coldCount > this.coldCapacity)
        {
            Interlocked.Decrement(ref this.coldCount);

            if (this.coldQueue.TryDequeue(out LongTickCountLruItem<TKey, TValue>? item))
            {
                ItemDestination where = this.policy.RouteCold(item);

                if (where == ItemDestination.Warm && this.warmCount <= this.warmCapacity)
                {
                    this.Move(item, where);
                }
                else
                {
                    this.Move(item, ItemDestination.Remove);
                }
            }
            else
            {
                Interlocked.Increment(ref this.coldCount);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Move(LongTickCountLruItem<TKey, TValue> item, ItemDestination where)
    {
        item.WasAccessed = false;

        switch (where)
        {
            case ItemDestination.Warm:
                this.warmQueue.Enqueue(item);
                Interlocked.Increment(ref this.warmCount);
                break;
            case ItemDestination.Cold:
                this.coldQueue.Enqueue(item);
                Interlocked.Increment(ref this.coldCount);
                break;
            case ItemDestination.Remove:
                if (!item.WasRemoved)
                {
                    // Avoid race where 2 threads could remove the same key - see TryRemove for details.
                    lock (item)
                    {
                        if (item.WasRemoved)
                        {
                            break;
                        }

                        if (this.dictionary.TryRemove(item.Key, out LongTickCountLruItem<TKey, TValue>? removedItem))
                        {
                            item.WasRemoved = true;
                            if (removedItem.Value is IDisposable d)
                            {
                                d.Dispose();
                            }
                        }
                    }
                }

                break;
        }
    }
}
