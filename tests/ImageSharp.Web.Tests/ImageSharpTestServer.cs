// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Tests
{
    public static class ImageSharpTestServer
    {
        public static string TestImage = "http://localhost/SubFolder/imagesharp-logo.png";

        public static Action<IApplicationBuilder> DefaultConfig = app =>
        {
            app.UseImageSharp();
        };

        public static Action<IServiceCollection> DefaultServices = services =>
        {
            services.AddImageSharpCore(
                    options =>
                        {
                            options.Configuration = Configuration.Default;
                            options.MaxBrowserCacheDays = -1;
                            options.MaxCacheDays = -1;
                            options.CachedNameLength = 12;
                            options.OnValidate = _ => { };
                            options.OnBeforeSave = _ => { };
                            options.OnProcessed = _ => { };
                            options.OnPrepareResponse = _ => { };
                        })
                    .SetUriParser<QueryCollectionUriParser>()
                    .SetCache<PhysicalFileSystemCache>()
                    .SetCacheHash<CacheHash>()
                    .SetAsyncKeyLock<AsyncKeyLock>()
                    .AddResolver<PhysicalFileSystemResolver>()
                    .AddProcessor<ResizeWebProcessor>();
        };

        public static TestServer CreateDefault() => Create(DefaultConfig, DefaultServices);


        public static TestServer CreateWithActions(
            Action<ImageValidationContext> onValidate,
            Action<FormattedImage> onBeforeSave = null,
            Action<ImageProcessingContext> onProcessed = null,
            Action<HttpContext> onPrepareResponse = null)
        {
            void ConfigureServices(IServiceCollection services) => services.AddImageSharpCore(options =>
                {
                    options.Configuration = Configuration.Default;
                    options.MaxBrowserCacheDays = -1;
                    options.MaxCacheDays = -1;
                    options.OnValidate = onValidate;
                    options.OnBeforeSave = onBeforeSave;
                    options.OnProcessed = onProcessed;
                    options.OnPrepareResponse = onPrepareResponse;
                })
                .SetUriParser<QueryCollectionUriParser>()
                .SetCache<PhysicalFileSystemCache>()
                .SetCacheHash<CacheHash>()
                .SetAsyncKeyLock<AsyncKeyLock>()
                .AddResolver<PhysicalFileSystemResolver>()
                .AddProcessor<ResizeWebProcessor>();

            return Create(DefaultConfig, ConfigureServices);
        }


        public static TestServer Create(Action<IApplicationBuilder> configureApp, Action<IServiceCollection> configureServices = null)
        {
            void DefaultConfigureServices(IServiceCollection services)
            {
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("webroot", "../../../")
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