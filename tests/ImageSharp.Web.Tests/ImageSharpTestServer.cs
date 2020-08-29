// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IO;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Providers.Azure;

namespace SixLabors.ImageSharp.Web.Tests
{
    public static class ImageSharpTestServer
    {
        private const string AzureConnectionString = "UseDevelopmentStorage=true";
        private const string AzureContainerName = "azure";
        private const string AzureCacheContainerName = "is-cache";
        private const string ImagePath = "SubFolder/imagesharp-logo.png";
        public const string PhysicalTestImage = "http://localhost/" + ImagePath;
        public const string AzureTestImage = "http://localhost/" + AzureContainerName + "/" + ImagePath;

        public static Action<IApplicationBuilder> DefaultConfig = app => app.UseImageSharp();

        public static Action<IServiceCollection> DefaultServices = services =>
        {
            services.AddImageSharpCore(
                    options =>
                        {
                            options.Configuration = Configuration.Default;
                            options.MemoryStreamManager = new RecyclableMemoryStreamManager();
                            options.BrowserMaxAge = TimeSpan.FromDays(-1);
                            options.CacheMaxAge = TimeSpan.FromDays(-1);
                            options.CachedNameLength = 12;
                            options.OnParseCommands = _ => { };
                            options.OnBeforeSave = _ => { };
                            options.OnProcessed = _ => { };
                            options.OnPrepareResponse = _ => { };
                        })
                    .SetRequestParser<QueryCollectionRequestParser>()
                    .Configure<PhysicalFileSystemCacheOptions>(_ => { })
                    .SetCache<PhysicalFileSystemCache>()
                    .SetCacheHash<CacheHash>()
                    .AddProvider(PhysicalProviderFactory)
                    .Configure<AzureBlobStorageImageProviderOptions>(options =>
                    {
                        options.BlobContainers.Add(new AzureBlobContainerClientOptions
                        {
                            ConnectionString = AzureConnectionString,
                            ContainerName = AzureContainerName
                        });
                    })
                    .AddProvider<AzureBlobStorageImageProvider>()
                    .AddProcessor<ResizeWebProcessor>();
        };

        public static Action<IServiceCollection> AzureProviderServices = services =>
        {
            services.AddImageSharpCore(
                    options =>
                    {
                        options.Configuration = Configuration.Default;
                        options.MemoryStreamManager = new RecyclableMemoryStreamManager();
                        options.BrowserMaxAge = TimeSpan.FromDays(-1);
                        options.CacheMaxAge = TimeSpan.FromDays(-1);
                        options.CachedNameLength = 12;
                        options.OnParseCommands = _ => { };
                        options.OnBeforeSave = _ => { };
                        options.OnProcessed = _ => { };
                        options.OnPrepareResponse = _ => { };
                    })
                    .SetRequestParser<QueryCollectionRequestParser>()
                    .Configure<PhysicalFileSystemCacheOptions>(_ => { })
                    .SetCache<PhysicalFileSystemCache>()
                    .SetCacheHash<CacheHash>()
                    .Configure<AzureBlobStorageImageProviderOptions>(options =>
                    {
                        options.BlobContainers.Add(new AzureBlobContainerClientOptions
                        {
                            ConnectionString = AzureConnectionString,
                            ContainerName = AzureContainerName
                        });
                    })
                    .AddProvider<AzureBlobStorageImageProvider>()
                    .AddProcessor<ResizeWebProcessor>();
        };

        public static TestServer CreateDefault() => CreateTestServer(DefaultConfig, DefaultServices);

        public static TestServer CreateAzure() => CreateTestServer(DefaultConfig, DefaultServices);

        public static TestServer CreateWithActionsNoCache(
            Action<ImageCommandContext> onParseCommands,
            Action<FormattedImage> onBeforeSave = null,
            Action<ImageProcessingContext> onProcessed = null,
            Action<HttpContext> onPrepareResponse = null)
        {
            Action<ImageSharpMiddlewareOptions> config = options =>
            {
                options.Configuration = Configuration.Default;
                options.MemoryStreamManager = new RecyclableMemoryStreamManager();
                options.BrowserMaxAge = TimeSpan.FromDays(-1);
                options.CacheMaxAge = TimeSpan.FromDays(-1);
                options.OnParseCommands = onParseCommands;
                options.OnBeforeSave = onBeforeSave;
                options.OnProcessed = onProcessed;
                options.OnPrepareResponse = onPrepareResponse;
            };

            return CreateWithActionsImpl(config);
        }

        public static TestServer CreateWithActionsCache(
            Action<ImageCommandContext> onParseCommands,
            Action<FormattedImage> onBeforeSave = null,
            Action<ImageProcessingContext> onProcessed = null,
            Action<HttpContext> onPrepareResponse = null)
        {
            Action<ImageSharpMiddlewareOptions> config = options =>
            {
                options.Configuration = Configuration.Default;
                options.MemoryStreamManager = new RecyclableMemoryStreamManager();
                options.BrowserMaxAge = TimeSpan.FromDays(7);
                options.CacheMaxAge = TimeSpan.FromDays(365);
                options.OnParseCommands = onParseCommands;
                options.OnBeforeSave = onBeforeSave;
                options.OnProcessed = onProcessed;
                options.OnPrepareResponse = onPrepareResponse;
            };

            return CreateWithActionsImpl(config);
        }

        private static TestServer CreateWithActionsImpl(
            Action<ImageSharpMiddlewareOptions> config)
        {
            void ConfigureServices(IServiceCollection services)
            {
                services.AddImageSharpCore(config)
                .SetRequestParser<QueryCollectionRequestParser>()
                .Configure<PhysicalFileSystemCacheOptions>(_ => { })
                .SetCache<PhysicalFileSystemCache>()
                .SetCacheHash<CacheHash>()
                .AddProvider(PhysicalProviderFactory)
                .Configure<AzureBlobStorageImageProviderOptions>(options =>
                {
                    options.BlobContainers.Add(new AzureBlobContainerClientOptions
                    {
                        ConnectionString = AzureConnectionString,
                        ContainerName = AzureContainerName
                    });
                })
                .AddProvider<AzureBlobStorageImageProvider>()
                .AddProcessor<ResizeWebProcessor>();
            }

            return CreateTestServer(DefaultConfig, ConfigureServices);
        }

        private static TestServer CreateTestServer(
            Action<IApplicationBuilder> configureApp,
            Action<IServiceCollection> configureServices)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("webroot", string.Empty)
                }).Build();

            IWebHostBuilder builder = new WebHostBuilder()
                .UseConfiguration(configuration)
                .Configure(configureApp)
                .ConfigureServices(configureServices);

            var server = new TestServer(builder);

            // TODO: Move me to the factory.
            InitializeAzureStorage(server);

            return server;
        }

        private static void InitializeAzureStorage(TestServer server)
        {
            // Upload an image to the Azure Test Storage;
            var container = new BlobContainerClient(AzureConnectionString, AzureContainerName);
            container.CreateIfNotExists(PublicAccessType.Blob);

#if NETCOREAPP2_1
            IHostingEnvironment environment = server.Host.Services.GetRequiredService<IHostingEnvironment>();
#else
            IWebHostEnvironment environment = server.Host.Services.GetRequiredService<IWebHostEnvironment>();
#endif

            BlobClient blob = container.GetBlobClient(ImagePath);

            if (!blob.Exists())
            {
                IFileInfo file = environment.WebRootFileProvider.GetFileInfo(ImagePath);
                using Stream stream = file.CreateReadStream();
                blob.Upload(stream, true);
            }
        }

        private static PhysicalFileSystemProvider PhysicalProviderFactory(IServiceProvider provider)
        {
            return new PhysicalFileSystemProvider(
#pragma warning disable SA1114 // Parameter list should follow declaration
#if NETCOREAPP2_1
                provider.GetRequiredService<IHostingEnvironment>(),
#else
                provider.GetRequiredService<IWebHostEnvironment>(),
#pragma warning restore SA1114 // Parameter list should follow declaration
#endif
                provider.GetRequiredService<FormatUtilities>())
            {
                Match = context => !context.Request.Path.StartsWithSegments("/" + AzureContainerName)
            };
        }
    }
}
