using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Helpers;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Helpers
{
    public class AsyncKeyLockTests
    {
        private readonly AsyncKeyLock asyncKeyLock = new AsyncKeyLock();
        const string AsyncKey = "ASYNC_KEY";
        const string AsyncKey1 = "ASYNC_KEY1";
        const string AsyncKey2 = "ASYNC_KEY2";
        const string Key = "KEY";
        const string Key1 = "KEY1";
        const string Key2 = "KEY2";

        [Fact]
        public void AsyncLockCanLockByKey()
        {
            bool zeroEntered = false;
            bool entered = false;
            int index = 0;
            Task[] tasks = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
            {
                using (await this.asyncKeyLock.LockAsync(AsyncKey))
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
        public void LockCanLockByKey()
        {
            bool zeroEntered = false;
            bool entered = false;
            int index = 0;
            Task[] tasks = Enumerable.Range(0, 5).Select(i => Task.Run(() =>
           {
               using (this.asyncKeyLock.Lock(Key))
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
                using (await this.asyncKeyLock.LockAsync(i > 0 ? AsyncKey2 : AsyncKey1))
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

        [Fact]
        public void LockAllowsDifferentKeysToRun()
        {
            bool zeroEntered = false;
            bool entered = false;
            int index = 0;
            Task[] tasks = Enumerable.Range(0, 5).Select(i => Task.Run(() =>
           {
               using (this.asyncKeyLock.Lock(i > 0 ? Key2 : Key1))
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