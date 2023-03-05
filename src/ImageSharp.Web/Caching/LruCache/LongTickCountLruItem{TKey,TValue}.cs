// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Caching;

internal class LongTickCountLruItem<TKey, TValue>
{
    private volatile bool wasAccessed;
    private volatile bool wasRemoved;

#pragma warning disable SA1401 // Fields should be private
    public readonly TKey Key;

    public readonly TValue Value;
#pragma warning restore SA1401 // Fields should be private

    public LongTickCountLruItem(TKey k, TValue v, long tickCount)
    {
        this.Key = k;
        this.Value = v;
        this.TickCount = tickCount;
    }

    public long TickCount { get; set; }

    public bool WasAccessed
    {
        get => this.wasAccessed;
        set => this.wasAccessed = value;
    }

    public bool WasRemoved
    {
        get => this.wasRemoved;
        set => this.wasRemoved = value;
    }
}
