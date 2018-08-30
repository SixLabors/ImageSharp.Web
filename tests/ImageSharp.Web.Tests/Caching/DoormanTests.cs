using SixLabors.ImageSharp.Web.Caching;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class DoormanTests
    {
        [Fact]
        public void DoormanInitializesSemaphoreSlim()
        {
            var doorman = new Doorman();
            Assert.NotNull(doorman.Semaphore);
            Assert.Equal(1, doorman.Semaphore.CurrentCount);
        }

        [Fact]
        public void DoormanResetsRefCounter()
        {
            var doorman = new Doorman();
            Assert.Equal(0, doorman.RefCount);

            doorman.TryAcquire();
            Assert.Equal(1, doorman.RefCount);

            doorman.TryAcquire();
            Assert.Equal(2, doorman.RefCount);

            Assert.False(doorman.Release());
            Assert.Equal(1, doorman.RefCount);

            Assert.True(doorman.Release());
            Assert.Equal(-1, doorman.RefCount);
        }
    }
}