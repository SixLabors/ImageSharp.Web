using SixLabors.ImageSharp.Web.Helpers;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Helpers
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
            Assert.Equal(1, doorman.RefCount);
            doorman.RefCount--;

            Assert.Equal(0, doorman.RefCount);

            doorman.Reset();
            Assert.Equal(1, doorman.RefCount);
        }
    }
}