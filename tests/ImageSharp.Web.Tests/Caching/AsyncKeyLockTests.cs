using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Caching;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Caching
{
    [Collection(nameof(NonParallelFixture))]
    public class AsyncKeyLockTests
    {
        private static readonly AsyncKeyLock AsyncLock = new AsyncKeyLock();
        private const string AsyncKey = "ASYNC_KEY";
        private const string AsyncKey1 = "ASYNC_KEY1";
        private const string AsyncKey2 = "ASYNC_KEY2";

        [Fact]
        public void AsyncLockActsAsDoorman()
        {
            // Run the two tests from a single test to see if we can stop tests freezing on Travis.
            this.AsyncLockCanLockByKey();
            this.AsyncLockAllowsDifferentKeysToRun();
        }

        private void AsyncLockCanLockByKey()
        {
            bool zeroEntered = false;
            bool entered = false;
            int index = 0;
            Task[] tasks = Enumerable.Range(0, 3).Select(i => Task.Run(async () =>
            {
                using (await AsyncLock.WriterLockAsync(AsyncKey).ConfigureAwait(false))
                {
                    if (i == 0)
                    {
                        entered = true;
                        zeroEntered = true;
                        await Task.Delay(3000).ConfigureAwait(false);
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
            Assert.Equal(3, index);
        }

        private void AsyncLockAllowsDifferentKeysToRun()
        {
            bool zeroEntered = false;
            bool entered = false;
            int index = 0;
            Task[] tasks = Enumerable.Range(0, 3).Select(i => Task.Run(async () =>
            {
                using (await AsyncLock.WriterLockAsync(i > 0 ? AsyncKey2 : AsyncKey1).ConfigureAwait(false))
                {
                    if (i == 0)
                    {
                        entered = true;
                        zeroEntered = true;
                        await Task.Delay(2000).ConfigureAwait(false);
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
            Assert.Equal(3, index);
        }
    }
}