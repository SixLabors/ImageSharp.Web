// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Time aware Least Recently Used (TLRU) is a variant of LRU which discards the least
    /// recently used items first, and any item that has expired.
    /// </summary>
    /// <remarks>
    /// This class measures time using stopwatch.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    internal readonly struct TLruLongTicksPolicy<TKey, TValue>
    {
        private readonly long timeToLive;

        public TLruLongTicksPolicy(TimeSpan timeToLive)
        {
            this.timeToLive = timeToLive.Ticks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LongTickCountLruItem<TKey, TValue> CreateItem(TKey key, TValue value)
        {
            return new LongTickCountLruItem<TKey, TValue>(key, value, Stopwatch.GetTimestamp());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Touch(LongTickCountLruItem<TKey, TValue> item)
        {
            item.WasAccessed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShouldDiscard(LongTickCountLruItem<TKey, TValue> item)
        {
            if (Stopwatch.GetTimestamp() - item.TickCount > this.timeToLive)
            {
                return true;
            }

            return false;
        }

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
}
