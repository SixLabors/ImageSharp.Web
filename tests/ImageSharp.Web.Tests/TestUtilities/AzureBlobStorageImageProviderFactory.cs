// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Providers.Azure;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public static class AzureBlobStorageImageProviderFactory
    {
        public static AzureBlobStorageImageProvider Create(IServiceProvider services)
        {
            IOptions<AzureBlobStorageImageProviderOptions> options = services.GetRequiredService<IOptions<AzureBlobStorageImageProviderOptions>>();
            FormatUtilities utilities = services.GetRequiredService<FormatUtilities>();
            InitializeAzureStorage(services, options.Value);

            return new AzureBlobStorageImageProvider(options, utilities);
        }

        private static void InitializeAzureStorage(IServiceProvider services, AzureBlobStorageImageProviderOptions options)
        {
            // Upload an image to the Azure Test Storage;
            AzureBlobContainerClientOptions containerOptions = options.BlobContainers.First();
            var container = new BlobContainerClient(containerOptions.ConnectionString, containerOptions.ContainerName);
            container.CreateIfNotExists(PublicAccessType.Blob);

#if NETCOREAPP2_1
            IHostingEnvironment environment = services.GetRequiredService<IHostingEnvironment>();
#else
            IWebHostEnvironment environment = services.GetRequiredService<IWebHostEnvironment>();
#endif

            BlobClient blob = container.GetBlobClient(TestConstants.ImagePath);

            if (!blob.Exists())
            {
                IFileInfo file = environment.WebRootFileProvider.GetFileInfo(TestConstants.ImagePath);
                using Stream stream = file.CreateReadStream();
                blob.Upload(stream, true);
            }
        }
    }
}
