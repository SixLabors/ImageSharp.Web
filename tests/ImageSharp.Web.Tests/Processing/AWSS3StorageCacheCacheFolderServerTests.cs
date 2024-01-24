// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Tests.TestUtilities;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.Processing;

public class AWSS3StorageCacheCacheFolderServerTests : ServerTestBase<AWSS3StorageCacheCacheFolderTestServerFixture>
{
    public AWSS3StorageCacheCacheFolderServerTests(AWSS3StorageCacheCacheFolderTestServerFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper, TestConstants.AWSTestImage)
    {
    }
}
