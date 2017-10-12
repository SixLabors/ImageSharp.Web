// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSharp.Web.Tests
{
    public static class StaticFilesTestServer
    {
        public static TestServer Create(Action<IApplicationBuilder> configureApp, Action<IServiceCollection> configureServices = null)
        {
            void DefaultConfigureServices(IServiceCollection services)
            {
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("webroot", ".")
                })
                .Build();

            IWebHostBuilder builder = new WebHostBuilder()
                .UseConfiguration(configuration)
                .Configure(configureApp)
                .ConfigureServices(configureServices ?? DefaultConfigureServices);
            return new TestServer(builder);
        }
    }
}
