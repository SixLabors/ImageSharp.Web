// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
