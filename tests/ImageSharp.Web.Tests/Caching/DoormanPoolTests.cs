using System;
using SixLabors.ImageSharp.Web.Caching;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class DoormanPoolTests
    {
        [Fact]
        public void RentingGivesDifferentInstances()
        {
            Doorman first = DoormanPool.Rent();
            Doorman second = DoormanPool.Rent();

            Assert.NotSame(first, second);

            DoormanPool.Return(first);
            DoormanPool.Return(second);
        }

        [Fact]
        public void DoormanPoolReusesItems()
        {
            int initialCount = DoormanPool.Count();
            Doorman first = DoormanPool.Rent();

            int currentCount = DoormanPool.Count();
            if (currentCount > 0)
            {
                Assert.Equal(initialCount - 1, currentCount);
                DoormanPool.Return(first);
                Assert.Equal(initialCount, DoormanPool.Count());
            }
            else
            {
                Assert.Equal(0, currentCount);
                DoormanPool.Return(first);
                Assert.Equal(initialCount + 1, DoormanPool.Count());
            }
        }

        [Fact]
        public void CallingReturnWithNullThrows()
        {
            Assert.Throws<ArgumentNullException>("doorman", () =>
             {
                 DoormanPool.Return(null);
             });
        }
    }
}