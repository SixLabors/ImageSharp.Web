// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using SixLabors.ImageSharp.Web.Middleware;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Actions
{
    public class ActionTests
    {
        private const string Command = "?width=20";

        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldRunOnValidateActionAsync(string url)
        {
            bool complete = false;
            void OnParseCommands(ImageCommandContext context)
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Context);
                Assert.NotNull(context.Commands);
                Assert.NotNull(context.Parser);
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActionsNoCache(OnParseCommands))
            {
                await server.CreateClient().GetAsync(url + Command);
            }

            Assert.True(complete);
        }

        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldRunOnValidateActionWithCacheAsync(string url)
        {
            bool complete = false;
            void OnParseCommands(ImageCommandContext context)
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Context);
                Assert.NotNull(context.Commands);
                Assert.NotNull(context.Parser);
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActionsCache(OnParseCommands))
            {
                await server.CreateClient().GetAsync(url + Command);
                Assert.True(complete);

                complete = false;
                await server.CreateClient().GetAsync(url + Command);
            }

            Assert.True(complete);
        }

        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldRunOnValidateActionNoCommandsAsync(string url)
        {
            bool complete = false;
            void OnParseCommands(ImageCommandContext context)
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Context);
                Assert.NotNull(context.Commands);
                Assert.NotNull(context.Parser);
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActionsNoCache(OnParseCommands))
            {
                await server.CreateClient().GetAsync(url);
            }

            Assert.True(complete);
        }

        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldRunOnValidateActionNoCommandsWithCacheAsync(string url)
        {
            bool complete = false;
            void OnParseCommands(ImageCommandContext context)
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Context);
                Assert.NotNull(context.Commands);
                Assert.NotNull(context.Parser);
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActionsCache(OnParseCommands))
            {
                await server.CreateClient().GetAsync(url + Command);
                Assert.True(complete);

                complete = false;
                await server.CreateClient().GetAsync(url + Command);
            }

            Assert.True(complete);
        }

        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldRunOnBeforeSaveActionAsync(string url)
        {
            bool complete = false;
            void OnBeforeSave(FormattedImage image)
            {
                Assert.NotNull(image);
                Assert.NotNull(image.Format);
                Assert.NotNull(image.Image);
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActionsNoCache(null, OnBeforeSave))
            {
                await server.CreateClient().GetAsync(url + Command);
            }

            Assert.True(complete);
        }

        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldNotRunOnBeforeSaveActionWithCacheAsync(string url)
        {
            bool complete = false;
            void OnBeforeSave(FormattedImage image)
            {
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActionsCache(null, OnBeforeSave))
            {
                await server.CreateClient().GetAsync(url + Command);

                Assert.False(complete);

                await server.CreateClient().GetAsync(url + Command);
            }

            Assert.False(complete);
        }

        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldRunOnProcessedActionAsync(string url)
        {
            bool complete = false;
            void OnProcessed(ImageProcessingContext context)
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Context);
                Assert.NotNull(context.Extension);
                Assert.NotNull(context.Stream);
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActionsNoCache(null, null, OnProcessed))
            {
                await server.CreateClient().GetAsync(url + Command);
            }

            Assert.True(complete);
        }

        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldNotRunOnProcessedActionWithCacheAsync(string url)
        {
            bool complete = false;
            void OnProcessed(ImageProcessingContext context)
            {
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActionsCache(null, null, OnProcessed))
            {
                await server.CreateClient().GetAsync(url + Command);
                Assert.False(complete);

                await server.CreateClient().GetAsync(url + Command);
            }

            Assert.False(complete);
        }

        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldRunOnPrepareResponseActionAsync(string url)
        {
            bool complete = false;
            void OnPrepareResponse(HttpContext context)
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Response);
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActionsNoCache(null, null, null, OnPrepareResponse))
            {
                await server.CreateClient().GetAsync(url + Command);
            }

            Assert.True(complete);
        }

        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldRunOnPrepareResponseActionWithCacheAsync(string url)
        {
            bool complete = false;
            void OnPrepareResponse(HttpContext context)
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Response);
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActionsCache(null, null, null, OnPrepareResponse))
            {
                await server.CreateClient().GetAsync(url + Command);
                Assert.True(complete);

                complete = false;
                await server.CreateClient().GetAsync(url + Command);
            }

            Assert.True(complete);
        }
    }
}
