// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Web.Tests.TestUtilities;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.Processing
{
    public class AWSS3StorageCacheServerTests : ServerTestBase<AWSS3StorageCacheTestServerFixture>
    {
        public AWSS3StorageCacheServerTests(AWSS3StorageCacheTestServerFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper, TestConstants.AWSTestImage)
        {
        }
    }
}
