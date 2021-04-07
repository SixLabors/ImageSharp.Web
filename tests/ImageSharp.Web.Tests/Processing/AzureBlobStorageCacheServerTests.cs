// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.Tests.TestUtilities;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Processing
{
    public class AzureBlobStorageCacheServerTests : ServerTestBase<AzureBlobStorageCacheTestServerFixture>
    {
        private const int Width = 20;
        private static readonly string Command = "?width=" + Width + "&v=" + Guid.NewGuid().ToString();
        private static readonly string Command2 = "?width=" + (Width + 1) + "&v=" + Guid.NewGuid().ToString();

        public AzureBlobStorageCacheServerTests(AzureBlobStorageCacheTestServerFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData(TestConstants.PhysicalTestImage)]
        [InlineData(TestConstants.AzureTestImage)]
        public Task CanProcessAndResolveImageAsync(string url) => RunClient(this.HttpClient, url, Command, Width);

        [Theory]
        [InlineData(TestConstants.PhysicalTestImage)]
        [InlineData(TestConstants.AzureTestImage)]
        public async Task CanProcessMultipleIdenticalQueriesAsync(string url)
        {
            Task[] tasks = Enumerable.Range(0, 100).Select(i => Task.Run(async () =>
            {
                var command = i % 2 == 0 ? Command : Command2;
                using HttpResponseMessage response = await this.HttpClient.GetAsync(url + command);
                Assert.NotNull(response);
                Assert.True(response.IsSuccessStatusCode);
            })).ToArray();

            var all = Task.WhenAll(tasks);
            await all;
            Assert.True(all.IsCompletedSuccessfully);
        }
    }
}
