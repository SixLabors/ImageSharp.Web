// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Providers.Azure;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public class PhysicalFileSystemCacheTestServerFixture : TestServerFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddImageSharp(options =>
            {
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
            })
                .ClearProviders()
                .Configure<AzureBlobStorageImageProviderOptions>(options =>
                {
                    options.BlobContainers.Add(new AzureBlobContainerClientOptions
                    {
                        ConnectionString = TestConstants.AzureConnectionString,
                        ContainerName = TestConstants.AzureContainerName
                    });
                })
                .AddProvider(AzureBlobStorageImageProviderFactory.Create)
                .AddProvider<PhysicalFileSystemProvider>()
                .AddProcessor<CacheBusterWebProcessor>();
        }

        protected override void Configure(IApplicationBuilder app)
        {
            app.UseImageSharp();
        }
    }
}
