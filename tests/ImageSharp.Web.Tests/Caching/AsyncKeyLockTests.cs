using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Caching;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    public class AsyncKeyLockTests
    {
        private readonly IAsyncKeyLock asyncKeyLock = new AsyncKeyLock();
        const string AsyncKey = "ASYNC_KEY";
        const string AsyncKey1 = "ASYNC_KEY1";
        const string AsyncKey2 = "ASYNC_KEY2";

        [Fact]
        public void AsyncLockCanLockByKey()
        {
            bool zeroEntered = false;
            bool entered = false;
            int index = 0;
            Task[] tasks = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
            {
                using (await this.asyncKeyLock.ReaderLockAsync(AsyncKey))
                {
                    if (i == 0)
                    {
                        entered = true;
                        zeroEntered = true;
                        Thread.Sleep(3000);
                        entered = false;
                    }
                    else if (zeroEntered)
                    {
                        Assert.False(entered);
                    }

                    index++;
                }

            })).ToArray();

            Task.WaitAll(tasks);
            Assert.Equal(5, index);
        }

        [Fact]
        public void AsyncLockAllowsDifferentKeysToRun()
        {
            bool zeroEntered = false;
            bool entered = false;
            int index = 0;
            Task[] tasks = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
            {
                using (await this.asyncKeyLock.ReaderLockAsync(i > 0 ? AsyncKey2 : AsyncKey1))
                {
                    if (i == 0)
                    {
                        entered = true;
                        zeroEntered = true;
                        Thread.Sleep(2000);
                        entered = false;
                    }
                    else if (zeroEntered)
                    {
                        Assert.True(entered);
                    }

                    index++;
                }

            })).ToArray();

            Task.WaitAll(tasks);
            Assert.Equal(5, index);
        }
    }
}