// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching.Azure;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Providers.Azure;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public class AzureBlobStorageCacheTestServerFixture : TestServerFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            AddDefaultImageSharp(services)
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
                .AddProcessor<CacheBusterWebProcessor>()
                .Configure<AzureBlobStorageCacheOptions>(options =>
                {
                    options.ConnectionString = TestConstants.AzureConnectionString;
                    options.ContainerName = TestConstants.AzureCacheContainerName;

                    AzureBlobStorageCache.CreateIfNotExists(options, PublicAccessType.None);
                })
                .SetCache<AzureBlobStorageCache>();
        }

        protected override void Configure(IApplicationBuilder app)
        {
            app.UseImageSharp();
        }
    }
}
