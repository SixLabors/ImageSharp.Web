// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Tests.TestUtilities;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.Processing;

public class AWSS3StorageCacheServerTests : ServerTestBase<AWSS3StorageCacheTestServerFixture>
{
    public AWSS3StorageCacheServerTests(AWSS3StorageCacheTestServerFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper, TestConstants.AWSTestImage)
    {
    }
}
