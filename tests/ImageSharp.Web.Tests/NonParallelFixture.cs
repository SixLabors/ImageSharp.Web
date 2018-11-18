using Xunit;

namespace SixLabors.ImageSharp.Web.Tests
{
    [CollectionDefinition(nameof(NonParallelFixture), DisableParallelization = true)]
    public class NonParallelFixture : ICollectionFixture<object>
    {
        // This class has no code. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
        // Apply this collection fixture to classes:
        // 1. That rely on single threaded processing.
    }
}
