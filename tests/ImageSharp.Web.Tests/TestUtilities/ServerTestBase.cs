// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public abstract class ServerTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : TestServerFixture
    {
        protected ServerTestBase(TFixture fixture) => this.HttpClient = fixture.HttpClient;

        public HttpClient HttpClient { get; }

        public static async Task RunClient(HttpClient client, string url, string Command, int Width)
        {
            string ext = Path.GetExtension(url);
            IImageFormat format = Configuration.Default.ImageFormatsManager.FindFormatByFileExtension(ext);

            // First response
            HttpResponseMessage response = await client.GetAsync(url + Command);

            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(response.Content.Headers.ContentLength > 0);
            Assert.Equal(format.DefaultMimeType, response.Content.Headers.ContentType.MediaType);

            (Image Image, IImageFormat Format) actual = await Image.LoadWithFormatAsync(await response.Content.ReadAsStreamAsync());
            using Image image = actual.Image;

            Assert.Equal(Width, image.Width);
            Assert.Equal(format, actual.Format);

            response.Dispose();

            // Cached Response
            response = await client.GetAsync(url + Command);

            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(response.Content.Headers.ContentLength > 0);
            Assert.Equal(format.DefaultMimeType, response.Content.Headers.ContentType.MediaType);

            (Image Image, IImageFormat Format) cachedActual = await Image.LoadWithFormatAsync(await response.Content.ReadAsStreamAsync());
            using Image cached = cachedActual.Image;

            Assert.Equal(Width, cached.Width);
            Assert.Equal(format, actual.Format);

            response.Dispose();

            // 304 response
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url + Command),
                Method = HttpMethod.Get,
            };

            request.Headers.IfModifiedSince = DateTimeOffset.UtcNow;

            response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength);
            Assert.Equal(format.DefaultMimeType, response.Content.Headers.ContentType.MediaType);

            request.Dispose();
            response.Dispose();

            // 412 response
            request = new HttpRequestMessage
            {
                RequestUri = new Uri(url + Command),
                Method = HttpMethod.Get,
            };

            request.Headers.IfUnmodifiedSince = DateTimeOffset.MinValue;

            response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength);

            request.Dispose();
            response.Dispose();
        }
    }
}
