// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Tests.TestUtilities;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.Processing;

public class AzureBlobStorageCacheCacheFolderServerTests : ServerTestBase<AzureBlobStorageCacheCacheFolderTestServerFixture>
{
    public AzureBlobStorageCacheCacheFolderServerTests(AzureBlobStorageCacheCacheFolderTestServerFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper, TestConstants.AzureTestImage)
    {
    }
}
