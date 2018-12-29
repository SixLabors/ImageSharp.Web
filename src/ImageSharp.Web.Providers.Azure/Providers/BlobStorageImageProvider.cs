// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Returns images stored in Azure Blob Storage.
    /// </summary>
    public class BlobStorageImageProvider : IImageProvider
    {
        /// <summary>
        /// The cloud storage account.
        /// </summary>
        private readonly CloudStorageAccount storageAccount;

        /// <summary>
        /// The container in the blob service.
        /// </summary>
        private readonly CloudBlobContainer container;

        /// <summary>
        /// The middleware configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// The blob storage options.
        /// </summary>
        private readonly BlobStorageImageProviderOptions storageOptions;

        /// <summary>
        /// Contains various helper methods based on the current configuration.
        /// </summary>
        private readonly FormatHelper formatHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageImageProvider"/> class.
        /// </summary>
        /// <param name="options">The middleware configuration options.</param>
        /// <param name="storageOptions">The blob storage options.</param>
        public BlobStorageImageProvider(IOptions<ImageSharpMiddlewareOptions> options, IOptions<BlobStorageImageProviderOptions> storageOptions)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(storageOptions, nameof(storageOptions));

            this.options = options.Value;
            this.storageOptions = storageOptions.Value;
            this.formatHelper = new FormatHelper(this.options.Configuration);
            this.storageAccount = CloudStorageAccount.Parse(this.storageOptions.ConnectionString);

            CloudBlobClient client = this.storageAccount.CreateCloudBlobClient();
            this.container = client.GetContainerReference(this.storageOptions.ContainerName);

            if (!this.container.Exists())
            {
                this.container.Create();
                this.container.SetPermissions(new BlobContainerPermissions { PublicAccess = this.storageOptions.AccessType });
            }
        }

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc/>
        public async Task<IImageResolver> GetAsync(HttpContext context)
        {
            // Strip the leading slash from the HTTP request path and treat the remaining path string as the blob name.
            // Path has already been correctly parsed before here.
            string blobName = context.Request.Path.Value.TrimStart('\\', '/');
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return null;
            }

            CloudBlockBlob blob = this.container.GetBlockBlobReference(blobName);
            if (!await blob.ExistsAsync().ConfigureAwait(false))
            {
                return null;
            }

            return new BlobStorageImageResolver(blob);
        }

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context) => this.formatHelper.GetExtension(context.Request.GetDisplayUrl()) != null;
    }
}