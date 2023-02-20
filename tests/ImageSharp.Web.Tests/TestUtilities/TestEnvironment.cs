// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

#if !ENV_CI
#endif

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities;

internal static class TestEnvironment
{
    /// <summary>
    /// Gets a value indicating whether test execution runs on CI.
    /// </summary>
#if ENV_CI
    internal static bool RunsOnCI => true;
#else
    internal static bool RunsOnCI
    {
        get
        {
            // TODO: Something weird going on with our environment constants here.
            // try reading the environment directly.
            // https://docs.github.com/en/actions/learn-github-actions/environment-variables
            string variable = Environment.GetEnvironmentVariable("CI");
            bool.TryParse(variable, out bool result);
            return result;
        }
    }
#endif
}
