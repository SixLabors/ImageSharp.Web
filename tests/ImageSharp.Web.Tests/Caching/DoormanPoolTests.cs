using System;
using SixLabors.ImageSharp.Web.Caching;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class DoormanPoolTests
    {
        //[Fact]
        //public void RentingGivesDifferentInstances()
        //{
        //    Doorman first = DoormanPool.Rent();
        //    Doorman second = DoormanPool.Rent();

        //    Assert.NotSame(first, second);

        //    DoormanPool.Return(first);
        //    DoormanPool.Return(second);
        //}

        //[Fact]
        //public void DoormanPoolReusesItems()
        //{
        //    Doorman first = DoormanPool.Rent();

        //    DoormanPool.Return(first);

        //    Doorman second = DoormanPool.Rent();

        //    Assert.Equal(first, second);

        //    DoormanPool.Return(second);
        //}

        //[Fact]
        //public void CallingReturnWithNullThrows()
        //{
        //    Assert.Throws<ArgumentNullException>("doorman", () =>
        //     {
        //         DoormanPool.Return(null);
        //     });
        //}
    }
}