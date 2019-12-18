// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Shared.Protocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace SixLabors.ImageSharp.Web.Tests
{
    public static class ImageSharpTestServer
    {
        private const string AzureConnectionString = "UseDevelopmentStorage=true";
        private const string AzureContainerName = "azure";
        private const string AzureRoutePrefix = "assets";
        private const string ImagePath = "SubFolder/imagesharp-logo.png";
        public const string PhysicalTestImage = "http://localhost/" + ImagePath;
        public const string AzureTestImage = "http://localhost/" + AzureRoutePrefix + "/" + AzureContainerName + "/" + ImagePath;

        public static Action<IApplicationBuilder> DefaultConfig = app => app.UseImageSharp();

        public static Action<IServiceCollection> DefaultServices = services =>
        {
            services.AddImageSharpCore(
                    options =>
                        {
                            options.Configuration = Configuration.Default;
                            options.MaxBrowserCacheDays = -1;
                            options.MaxCacheDays = -1;
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
                    .AddProvider<PhysicalFileSystemProvider>(PhysicalProviderFactory)
                    .Configure<AzureBlobStorageImageProviderOptions>(options =>
                    {
                        options.ConnectionString = AzureConnectionString;
                        options.RoutePrefix = AzureRoutePrefix;
                    })
                    .AddProvider<AzureBlobStorageImageProvider>()
                    .AddProcessor<ResizeWebProcessor>();
        };

        public static TestServer CreateDefault() => Create(DefaultConfig, DefaultServices);

        public static TestServer CreateWithActions(
            Action<ImageCommandContext> onParseCommands,
            Action<FormattedImage> onBeforeSave = null,
            Action<ImageProcessingContext> onProcessed = null,
            Action<HttpContext> onPrepareResponse = null)
        {
            void ConfigureServices(IServiceCollection services)
            {
                services.AddImageSharpCore(options =>
                {
                    options.Configuration = Configuration.Default;
                    options.MaxBrowserCacheDays = -1;
                    options.MaxCacheDays = -1;
                    options.OnParseCommands = onParseCommands;
                    options.OnBeforeSave = onBeforeSave;
                    options.OnProcessed = onProcessed;
                    options.OnPrepareResponse = onPrepareResponse;
                })
                .SetRequestParser<QueryCollectionRequestParser>()
                .Configure<PhysicalFileSystemCacheOptions>(_ => { })
                .SetCache<PhysicalFileSystemCache>()
                .SetCacheHash<CacheHash>()
                .AddProvider<PhysicalFileSystemProvider>(PhysicalProviderFactory)
                .Configure<AzureBlobStorageImageProviderOptions>(options =>
                {
                    options.ConnectionString = AzureConnectionString;
                    options.RoutePrefix = AzureRoutePrefix;
                })
                .AddProvider<AzureBlobStorageImageProvider>()
                .AddProcessor<ResizeWebProcessor>();
            }

            return Create(DefaultConfig, ConfigureServices);
        }

        public static TestServer Create(Action<IApplicationBuilder> configureApp, Action<IServiceCollection> configureServices = null)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("webroot", "../../../")
                }).Build();

            IWebHostBuilder builder = new WebHostBuilder()
                .UseConfiguration(configuration)
                .Configure(configureApp)
                .ConfigureServices(configureServices ?? default);

            var server = new TestServer(builder);

            InitializeAzureStorage(server);

            return server;
        }

        private static void InitializeAzureStorage(TestServer server)
        {
            try
            {
                // Upload an image to the Azure Test Storage;
                var storageAccount = CloudStorageAccount.Parse(AzureConnectionString);
                CloudBlobClient client = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = client.GetContainerReference(AzureContainerName);

                if (!container.Exists())
                {
                    container.Create();
                    container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                }

                IHostingEnvironment environment = server.Host.Services.GetRequiredService<IHostingEnvironment>();
                CloudBlockBlob blob = container.GetBlockBlobReference(ImagePath);
                if (!blob.Exists())
                {
                    IFileInfo file = environment.WebRootFileProvider.GetFileInfo(ImagePath);
                    using (System.IO.Stream stream = file.CreateReadStream())
                    {
                        blob.UploadFromStream(stream);
                    }
                }
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict
                || storageException.RequestInformation.ExtendedErrorInformation.ErrorCode == StorageErrorCodeStrings.ContainerAlreadyExists)
            {
            }
        }

        private static PhysicalFileSystemProvider PhysicalProviderFactory(IServiceProvider provider)
        {
            return new PhysicalFileSystemProvider(
                provider.GetRequiredService<IHostingEnvironment>(),
                provider.GetRequiredService<FormatUtilities>())
            {
                Match = context => !context.Request.Path.StartsWithSegments("/" + AzureContainerName)
            };
        }
    }
}
