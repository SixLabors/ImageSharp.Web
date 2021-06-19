using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Providers.Azure;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Tests.TestUtilities;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Processing
{
    public class MiddlewareOptionsTests
    {
        private const int Width = 20;
        private static readonly string Command = "?width=" + Width + "&v=" + Guid.NewGuid().ToString();
        private static readonly string Command2 = "?width=" + (Width + 1) + "&v=" + Guid.NewGuid().ToString();

        [Theory]
        [InlineData(TestConstants.PhysicalTestImage, true)]
        [InlineData(TestConstants.PhysicalTestImage, false)]
        public async Task CanProcessAndResolveImageAsync(string url, bool ignoreHost)
        {
            VariableHostTestServerFixture.____RULE_VIOLATION____IgnoreHost____RULE_VIOLATION____ = ignoreHost;
            var fixture = new VariableHostTestServerFixture();
            HttpClient client = fixture.HttpClient;

            await ServerTestBase<VariableHostTestServerFixture>.RunClient(client, url, Command, Width);
            fixture.ToggleHost();
            await ServerTestBase<VariableHostTestServerFixture>.RunClient(client, url, Command, Width);

            if (ignoreHost)
            {
                Assert.Equal(1, fixture.KeysUsed);
            }
            else
            {
                Assert.NotInRange(fixture.KeysUsed, 0, 1);
            }
        }
    }

    public class VariableHostTestServerFixture : TestServerFixture
    {
        public int KeysUsed => this.imageCache.Keys.Count;

        //private readonly bool ignoreHost;

        // I get the feeling that this is a naming violation...
        // I had tried to make this an instance property, but for some reason it wasn't working - it was always false...
        public static bool ____RULE_VIOLATION____IgnoreHost____RULE_VIOLATION____;

        private ImageCacheMock imageCache = new ImageCacheMock();

        private int hostNumber = 1;

        //public VariableHostTestServerFixture(bool ignoreHost) => this.ignoreHost = ignoreHost;

        public void ToggleHost() => ++hostNumber;

        protected override void Configure(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Request.Host = new Microsoft.AspNetCore.Http.HostString($"test{hostNumber}.com");

                await next();
            });

            app.UseImageSharp();
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            this.AddDefaultImageSharp(services, options =>
            {
                //options.IgnoreHost = this.ignoreHost;
                options.IgnoreHost = ____RULE_VIOLATION____IgnoreHost____RULE_VIOLATION____;
            })
                .ClearProviders()
                .Configure<AzureBlobStorageImageProviderOptions>(options =>
                {
                    options.BlobContainers.Add(new AzureBlobContainerClientOptions
                    {
                        ConnectionString = TestConstants.AzureConnectionString,
                        ContainerName = TestConstants.AzureContainerName
                    });
                })
                .AddProvider(AzureBlobStorageImageProviderFactory.Create)
                .AddProvider<PhysicalFileSystemProvider>()
                .AddProcessor<CacheBusterWebProcessor>()
                .SetCache(_ => imageCache);
        }
    }

    public class ImageCacheMock : IImageCache
    {
        /// <summary>
        /// All keys which were used to try to get or set an image.
        /// </summary>
        public HashSet<string> Keys { get; } = new HashSet<string>();

        public Task<IImageCacheResolver> GetAsync(string key) => Task.FromResult<IImageCacheResolver>(null);

        public async Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata) => this.Keys.Add(key);
    }
}
