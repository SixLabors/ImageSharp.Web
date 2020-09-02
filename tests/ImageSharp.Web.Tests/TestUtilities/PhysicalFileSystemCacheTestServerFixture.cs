// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;
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
                options.OnParseCommands = context =>
                {
                    Assert.NotNull(context);
                    Assert.NotNull(context.Context);
                    Assert.NotNull(context.Commands);
                    Assert.NotNull(context.Parser);
                };

                options.OnProcessed = context =>
                {
                    Assert.NotNull(context);
                    Assert.NotNull(context.Commands);
                    Assert.NotNull(context.ContentType);
                    Assert.NotNull(context.Context);
                    Assert.NotNull(context.Extension);
                    Assert.NotNull(context.Stream);
                };

                options.OnBeforeSave = context =>
                {
                    Assert.NotNull(context);
                    Assert.NotNull(context.Format);
                    Assert.NotNull(context.Image);
                };

                options.OnPrepareResponse = context =>
                {
                    Assert.NotNull(context);
                    Assert.NotNull(context.Response);
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
