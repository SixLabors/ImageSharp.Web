// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Net.Http.Headers;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Providers.Azure;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities;

public static class AzureBlobStorageImageProviderFactory
{
    public static AzureBlobStorageImageProvider Create(IServiceProvider services)
    {
        IOptions<AzureBlobStorageImageProviderOptions> options = services.GetRequiredService<IOptions<AzureBlobStorageImageProviderOptions>>();
        FormatUtilities utilities = services.GetRequiredService<FormatUtilities>();
        InitializeAzureStorage(services, options.Value);

        return new AzureBlobStorageImageProvider(options, utilities, services);
    }

    private static void InitializeAzureStorage(IServiceProvider services, AzureBlobStorageImageProviderOptions options)
    {
        // Upload an image to the Azure Test Storage;
        AzureBlobContainerClientOptions containerOptions = options.BlobContainers.First();
        var container = new BlobContainerClient(containerOptions.ConnectionString, containerOptions.ContainerName);
        container.CreateIfNotExists(PublicAccessType.Blob);

        IWebHostEnvironment environment = services.GetRequiredService<IWebHostEnvironment>();

        BlobClient blob = container.GetBlobClient(TestConstants.ImagePath);

        if (!blob.Exists())
        {
            IFileInfo file = environment.WebRootFileProvider.GetFileInfo(TestConstants.ImagePath);
            using Stream stream = file.CreateReadStream();

            // Set the max-age property so we get coverage for testing is in our Azure provider.
            var cacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(7),
                MustRevalidate = true
            };

            var headers = new BlobHttpHeaders
            {
                CacheControl = cacheControl.ToString(),
            };

            blob.Upload(stream, httpHeaders: headers);
        }
    }
}
