// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
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
                        options.ContainerName = AzureContainerName;
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
                    options.ContainerName = AzureContainerName;
                })
                .AddProvider<AzureBlobStorageImageProvider>()
                .AddProcessor<ResizeWebProcessor>();
            }

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
                }).Build();

            IWebHostBuilder builder = new WebHostBuilder()
                .UseConfiguration(configuration)
                .Configure(configureApp)
                .ConfigureServices(configureServices ?? DefaultConfigureServices);

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
            catch (StorageException)
            {
                // On Appveyor "Exists" appears to fail and the following exception is thrown.
                // I cannot replicate this locally via Visual Studio Test Explorer or "dotnet test".
                //
                // Failed CanAddRemoveImageProcessors
                // Error Message:
                // Microsoft.Azure.Storage.StorageException : The specified container already exists.
                // Stack Trace:
                //   at Microsoft.Azure.Storage.Core.Executor.Executor.ExecuteAsync[T](RESTCommand`1 cmd, IRetryPolicy policy, OperationContext operationContext, CancellationToken token)
                //   at Microsoft.Azure.Storage.Core.Executor.Executor.<> c__DisplayClass0_0`1.< ExecuteSync > b__0()
                //   at Microsoft.Azure.Storage.Core.Util.CommonUtility.RunWithoutSynchronizationContext[T](Func`1 actionToRun)
                //   at Microsoft.Azure.Storage.Core.Executor.Executor.ExecuteSync[T](RESTCommand`1 cmd, IRetryPolicy policy, OperationContext operationContext)
                //   at Microsoft.Azure.Storage.Blob.CloudBlobContainer.Create(BlobContainerPublicAccessType accessType, BlobRequestOptions requestOptions, OperationContext operationContext)
                //   at Microsoft.Azure.Storage.Blob.CloudBlobContainer.Create(BlobRequestOptions requestOptions, OperationContext operationContext)
                //   at SixLabors.ImageSharp.Web.Tests.ImageSharpTestServer.InitializeAzureStorage(TestServer server) in C:\projects\imagesharp - web\tests\ImageSharp.Web.Tests\ImageSharpTestServer.cs:line 143
                //   at SixLabors.ImageSharp.Web.Tests.ImageSharpTestServer.Create(Action`1 configureApp, Action`1 configureServices)
                //   in C:\projects\imagesharp - web\tests\ImageSharp.Web.Tests\ImageSharpTestServer.cs:line 123
                //   at SixLabors.ImageSharp.Web.Tests.DependencyInjection.ServiceRegistrationExtensionsTests.CanAddRemoveImageProcessors()
                //   in C:\projects\imagesharp - web\tests\ImageSharp.Web.Tests\DependencyInjection\ServiceRegistrationExtensionsTests.cs:line 46
            }
        }

        private static PhysicalFileSystemProvider PhysicalProviderFactory(IServiceProvider provider)
        {
            return new PhysicalFileSystemProvider(
                provider.GetRequiredService<IHostingEnvironment>(),
                provider.GetRequiredService<FormatUtilities>())
            {
                Match = context =>
                {
                    return !context.Request.Path.StartsWithSegments("/" + AzureContainerName);
                }
            };
        }
    }
}