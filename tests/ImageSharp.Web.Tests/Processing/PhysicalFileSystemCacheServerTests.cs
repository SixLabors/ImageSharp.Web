// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Tests.TestUtilities;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.Processing;

public class PhysicalFileSystemCacheServerTests : ServerTestBase<PhysicalFileSystemCacheTestServerFixture>
{
    public PhysicalFileSystemCacheServerTests(PhysicalFileSystemCacheTestServerFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper, TestConstants.PhysicalTestImage)
    {
    }
}
