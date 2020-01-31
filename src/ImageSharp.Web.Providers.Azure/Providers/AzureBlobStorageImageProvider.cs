// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Returns images stored in Azure Blob Storage.
    /// </summary>
    public class AzureBlobStorageImageProvider : IImageProvider
    {
        /// <summary>
        /// Character array to remove from paths.
        /// </summary>
        private static readonly char[] SlashChars = { '\\', '/' };

        /// <summary>
        /// The container in the blob service.
        /// </summary>
        private readonly BlobContainerClient container;

        /// <summary>
        /// The blob storage options.
        /// </summary>
        private readonly AzureBlobStorageImageProviderOptions storageOptions;

        /// <summary>
        /// Contains various helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities formatUtilities;

        /// <summary>
        /// A match function used by the resolver to identify itself as the correct resolver to use.
        /// </summary>
        private Func<HttpContext, bool> match;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageImageProvider"/> class.
        /// </summary>
        /// <param name="storageOptions">The blob storage options.</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public AzureBlobStorageImageProvider(
            IOptions<AzureBlobStorageImageProviderOptions> storageOptions,
            FormatUtilities formatUtilities)
        {
            Guard.NotNull(storageOptions, nameof(storageOptions));

            this.storageOptions = storageOptions.Value;
            this.formatUtilities = formatUtilities;

            this.container = new BlobContainerClient(this.storageOptions.ConnectionString, this.storageOptions.ContainerName);
        }

        /// <inheritdoc/>
        public ProcessingBehavior ProcessingBehavior { get; } = ProcessingBehavior.All;

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match
        {
            get => this.match ?? this.IsMatch;
            set => this.match = value;
        }

        /// <inheritdoc/>
        public async Task<IImageResolver> GetAsync(HttpContext context)
        {
            // Strip the leading slash and container name from the HTTP request path and treat
            // the remaining path string as the blob name.
            // Path has already been correctly parsed before here.
            string blobName = context.Request.Path.Value.TrimStart(SlashChars)
                                     .Substring(this.storageOptions.ContainerName.Length)
                                     .TrimStart(SlashChars);

            if (string.IsNullOrWhiteSpace(blobName))
            {
                return null;
            }

            BlobClient blob = this.container.GetBlobClient(blobName);

            if (!await blob.ExistsAsync())
            {
                return null;
            }

            return new AzureBlobStorageImageResolver(blob);
        }

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context)
            => this.formatUtilities.GetExtensionFromUri(context.Request.GetDisplayUrl()) != null;

        private bool IsMatch(HttpContext context)
        {
            string path = context.Request.Path.Value.TrimStart(SlashChars);
            return path.StartsWith(this.storageOptions.ContainerName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
