// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public abstract class TestServerFixture : IDisposable
    {
        private TestServer server;
        private bool isDisposed;

        protected TestServerFixture()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("webroot", string.Empty)
            })
            .Build();

            IWebHostBuilder builder = new WebHostBuilder()
                .UseConfiguration(configuration)
                .Configure(this.Configure)
                .ConfigureServices(this.ConfigureServices);

            this.server = new TestServer(builder);
            this.Services = this.server.Host.Services;
            this.HttpClient = this.server.CreateClient();
        }

        public HttpClient HttpClient { get; private set; }

        public IServiceProvider Services { get; private set; }

        protected abstract void ConfigureServices(IServiceCollection services);

        protected abstract void Configure(IApplicationBuilder app);

        protected IImageSharpBuilder AddDefaultImageSharp(IServiceCollection services, Action<ImageSharpMiddlewareOptions> configure = null)
        {
            return services.AddImageSharp(options =>
            {
                configure?.Invoke(options);

                Func<ImageCommandContext, Task> onParseCommandsAsync = options.OnParseCommandsAsync;

                options.OnParseCommandsAsync = context =>
                {
                    Assert.NotNull(context);
                    Assert.NotNull(context.Context);
                    Assert.NotNull(context.Commands);
                    Assert.NotNull(context.Parser);

                    return onParseCommandsAsync.Invoke(context);
                };

                Func<ImageProcessingContext, Task> onProcessedAsync = options.OnProcessedAsync;

                options.OnProcessedAsync = context =>
                {
                    Assert.NotNull(context);
                    Assert.NotNull(context.Commands);
                    Assert.NotNull(context.ContentType);
                    Assert.NotNull(context.Context);
                    Assert.NotNull(context.Extension);
                    Assert.NotNull(context.Stream);

                    return onProcessedAsync.Invoke(context);
                };

                Func<FormattedImage, Task> onBeforeSaveAsync = options.OnBeforeSaveAsync;

                options.OnBeforeSaveAsync = context =>
                {
                    Assert.NotNull(context);
                    Assert.NotNull(context.Format);
                    Assert.NotNull(context.Encoder);
                    Assert.NotNull(context.Image);

                    return onBeforeSaveAsync.Invoke(context);
                };

                Func<HttpContext, Task> onPrepareResponseAsync = options.OnPrepareResponseAsync;

                options.OnPrepareResponseAsync = context =>
                {
                    Assert.NotNull(context);
                    Assert.NotNull(context.Response);

                    return onPrepareResponseAsync.Invoke(context);
                };
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.server.Dispose();
                    this.HttpClient.Dispose();
                }

                this.server = null;
                this.HttpClient = null;
                this.isDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
