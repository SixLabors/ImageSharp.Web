// Copyright (c) Six Labors and contributors.
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
        [Fact]
        public async Task ShouldRunOnValidateAction()
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

            TestServer server = ImageSharpTestServer.CreateWithActions(OnParseCommands);

            await server.CreateClient().GetAsync(ImageSharpTestServer.TestImage + "?width=20").ConfigureAwait(false);
            Assert.True(complete);
        }

        [Fact]
        public async Task ShouldRunOnBeforeSaveAction()
        {
            bool complete = false;
            void OnBeforeSave(FormattedImage image)
            {
                Assert.NotNull(image);
                Assert.NotNull(image.Format);
                Assert.NotNull(image.Image);
                complete = true;
            }

            TestServer server = ImageSharpTestServer.CreateWithActions(null, OnBeforeSave);

            await server.CreateClient().GetAsync(ImageSharpTestServer.TestImage + "?width=20").ConfigureAwait(false);
            Assert.True(complete);
        }

        [Fact]
        public async Task ShouldRunOnProcessedAction()
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

            TestServer server = ImageSharpTestServer.CreateWithActions(null, null, OnProcessed);

            await server.CreateClient().GetAsync(ImageSharpTestServer.TestImage + "?width=20").ConfigureAwait(false);
            Assert.True(complete);
        }

        [Fact]
        public async Task ShouldRunOnPrepareResponseAction()
        {
            bool complete = false;
            void OnPrepareResponse(HttpContext context)
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Response);
                complete = true;
            }

            TestServer server = ImageSharpTestServer.CreateWithActions(null, null, null, OnPrepareResponse);

            await server.CreateClient().GetAsync(ImageSharpTestServer.TestImage + "?width=20").ConfigureAwait(false);
            Assert.True(complete);
        }
    }
}