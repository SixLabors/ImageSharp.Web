// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Tests.TestUtilities;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.Processing;

public class AzureBlobStorageCacheServerTests : ServerTestBase<AzureBlobStorageCacheTestServerFixture>
{
    public AzureBlobStorageCacheServerTests(AzureBlobStorageCacheTestServerFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper, TestConstants.AzureTestImage)
    {
    }
}
