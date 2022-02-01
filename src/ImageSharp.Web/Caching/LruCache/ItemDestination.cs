// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Diagnostics.CodeAnalysis;

namespace SixLabors.ImageSharp.Web.Caching
{
    [ExcludeFromCodeCoverage]
    internal enum ItemDestination
    {
        Warm,
        Cold,
        Remove
    }
}
