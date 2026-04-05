// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection;

public class FakeCacheHash : ICacheHash
{
    public string Create(string value, uint length) => null;
}
