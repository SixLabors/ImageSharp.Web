// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using SixLabors.ImageSharp.Web.Middleware;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Actions
{
    public class ActionTests
    {
        [Theory]
        [InlineData(ImageSharpTestServer.PhysicalTestImage)]
        [InlineData(ImageSharpTestServer.AzureTestImage)]
        public async Task ShouldRunOnValidateAction(string url)
        {
            // Running this test with Azurite https://github.com/Azure/Azurite/ on Travis results in the following exception.
            // 
            // Error Message:
            // Microsoft.Azure.Storage.StorageException : Unexpected response code, Expected:PartialContent, Received:OK
            // Stack Trace:
            //   at Microsoft.Azure.Storage.Core.Executor.Executor.ExecuteAsync[T](RESTCommand`1 cmd, IRetryPolicy policy, OperationContext operationContext, CancellationToken token)
            //   at Microsoft.Azure.Storage.Blob.BlobReadStream.DispatchReadAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
            //   at Microsoft.Azure.Storage.Blob.BlobReadStream.Read(Byte[] buffer, Int32 offset, Int32 count)
            //   at SixLabors.ImageSharp.Image.InternalDetectFormat(Stream stream, Configuration config)
            //   at SixLabors.ImageSharp.Image.DiscoverDecoder(Stream stream, Configuration config, IImageFormat& format)
            //   at SixLabors.ImageSharp.Image.Decode[TPixel](Stream stream, Configuration config)
            //   at SixLabors.ImageSharp.Image.Load[TPixel](Configuration config, Stream stream, IImageFormat& format)
            //   at SixLabors.ImageSharp.Web.Middleware.ImageSharpMiddleware.Invoke(HttpContext context) in /home/travis/build/SixLabors/ImageSharp.Web/src/ImageSharp.Web/Middleware/ImageSharpMiddleware.cs:line 228
            //   at Microsoft.AspNetCore.Hosting.Internal.RequestServicesContainerMiddleware.Invoke(HttpContext httpContext)
            //   at Microsoft.AspNetCore.TestHost.ClientHandler.<>c__DisplayClass3_0.<<SendAsync>b__0>d.MoveNext()
            // --- End of stack trace from previous location where exception was thrown ---
            //   at Microsoft.AspNetCore.TestHost.ClientHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            //   at System.Net.Http.HttpClient.FinishSendAsyncBuffered(Task`1 sendTask, HttpRequestMessage request, CancellationTokenSource cts, Boolean disposeCts)
            //   at SixLabors.ImageSharp.Web.Tests.Actions.ActionTests.ShouldRunOnValidateAction(String url) in /home/travis/build/SixLabors/ImageSharp.Web/tests/ImageSharp.Web.Tests/Actions/ActionTests.cs:line 31
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return;
            }

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
                await server.CreateClient().GetAsync(url + "?width=20").ConfigureAwait(false);
            }

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

            using (TestServer server = ImageSharpTestServer.CreateWithActions(null, OnBeforeSave))
            {
                await server.CreateClient().GetAsync(ImageSharpTestServer.PhysicalTestImage + "?width=20").ConfigureAwait(false);
            }
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

            using (TestServer server = ImageSharpTestServer.CreateWithActions(null, null, OnProcessed))
            {
                await server.CreateClient().GetAsync(ImageSharpTestServer.PhysicalTestImage + "?width=20").ConfigureAwait(false);
            }
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

            using (TestServer server = ImageSharpTestServer.CreateWithActions(null, null, null, OnPrepareResponse))
            {
                await server.CreateClient().GetAsync(ImageSharpTestServer.PhysicalTestImage + "?width=20").ConfigureAwait(false);
            }
            Assert.True(complete);
        }
    }
}