// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web.Caching;

/// <summary>
/// Time aware Least Recently Used (TLRU) is a variant of LRU which discards the least
/// recently used items first, and any item that has expired.
/// </summary>
/// <typeparam name="TKey">The type of key object.</typeparam>
/// <typeparam name="TValue">The type of value object.</typeparam>
/// <remarks>
/// This class measures time using stopwatch.
/// </remarks>
internal readonly struct TLruLongTicksPolicy<TKey, TValue>
{
    private readonly long timeToLive;

    public TLruLongTicksPolicy(TimeSpan timeToLive)
        => this.timeToLive = timeToLive.Ticks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LongTickCountLruItem<TKey, TValue> CreateItem(TKey key, TValue value)
        => new(key, value, Stopwatch.GetTimestamp());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Touch(LongTickCountLruItem<TKey, TValue> item) => item.WasAccessed = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldDiscard(LongTickCountLruItem<TKey, TValue> item)
        => Stopwatch.GetTimestamp() - item.TickCount > this.timeToLive;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ItemDestination RouteHot(LongTickCountLruItem<TKey, TValue> item)
    {
        if (this.ShouldDiscard(item))
        {
            return ItemDestination.Remove;
        }

        if (item.WasAccessed)
        {
            return ItemDestination.Warm;
        }

        return ItemDestination.Cold;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ItemDestination RouteWarm(LongTickCountLruItem<TKey, TValue> item)
    {
        if (this.ShouldDiscard(item))
        {
            return ItemDestination.Remove;
        }

        if (item.WasAccessed)
        {
            return ItemDestination.Warm;
        }

        return ItemDestination.Cold;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ItemDestination RouteCold(LongTickCountLruItem<TKey, TValue> item)
    {
        if (this.ShouldDiscard(item))
        {
            return ItemDestination.Remove;
        }

        if (item.WasAccessed)
        {
            return ItemDestination.Warm;
        }

        return ItemDestination.Remove;
    }
}
