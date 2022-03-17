// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    internal static class TestEnvironment
    {
        /// <summary>
        /// Gets a value indicating whether test execution runs on CI.
        /// </summary>
#if ENV_CI
        internal static bool RunsOnCI => true;
#else
        internal static bool RunsOnCI => false;
#endif
    }
}
