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

            using (TestServer server = ImageSharpTestServer.CreateWithActions(OnParseCommands))
            {
                await server.CreateClient().GetAsync(url + Command);
            }

            Assert.True(complete);
        }

        [Theory]
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

            using (TestServer server = ImageSharpTestServer.CreateWithActions(OnParseCommands))
            {
                await server.CreateClient().GetAsync(url);
            }

            Assert.True(complete);
        }

        [Fact]
        public async Task ShouldRunOnBeforeSaveActionAsync()
        {
            bool complete = false;
            void OnBeforeSave(FormattedImage image)
            {
                Assert.NotNull(image);
                Assert.NotNull(image.Format);
                Assert.NotNull(image.Image);
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActions(null, OnBeforeSave))
            {
                await server.CreateClient().GetAsync(ImageSharpTestServer.PhysicalTestImage + Command);
            }

            Assert.True(complete);
        }

        [Fact]
        public async Task ShouldRunOnProcessedActionAsync()
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

            using (TestServer server = ImageSharpTestServer.CreateWithActions(null, null, OnProcessed))
            {
                await server.CreateClient().GetAsync(ImageSharpTestServer.PhysicalTestImage + Command);
            }

            Assert.True(complete);
        }

        [Fact]
        public async Task ShouldRunOnPrepareResponseActionAsync()
        {
            bool complete = false;
            void OnPrepareResponse(HttpContext context)
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Response);
                complete = true;
            }

            using (TestServer server = ImageSharpTestServer.CreateWithActions(null, null, null, OnPrepareResponse))
            {
                await server.CreateClient().GetAsync(ImageSharpTestServer.PhysicalTestImage + Command);
            }

            Assert.True(complete);
        }
    }
}
