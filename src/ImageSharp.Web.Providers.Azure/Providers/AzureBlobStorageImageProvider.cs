// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Resolvers.Azure;

namespace SixLabors.ImageSharp.Web.Providers.Azure
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
        /// The containers for the blob services.
        /// </summary>
        private readonly Dictionary<string, BlobContainerClient> containers
            = new Dictionary<string, BlobContainerClient>();

        /// <summary>
        /// The blob storage options.
        /// </summary>
        private readonly AzureBlobStorageImageProviderOptions storageOptions;

        /// <summary>
        /// The recyclable memorystream manager used for managing pooled stream
        /// buffers independently from image buffer pooling.
        /// </summary>
        private readonly RecyclableMemoryStreamManager memoryStreamManager;

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
        /// <param name="memoryStreamManager">
        /// The recyclable memorystream manager used for managing pooled stream
        /// buffers independently from image buffer pooling.
        /// </param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public AzureBlobStorageImageProvider(
            IOptions<AzureBlobStorageImageProviderOptions> storageOptions,
            RecyclableMemoryStreamManager memoryStreamManager,
            FormatUtilities formatUtilities)
        {
            Guard.NotNull(storageOptions, nameof(storageOptions));
            Guard.NotNull(memoryStreamManager, nameof(memoryStreamManager));

            this.storageOptions = storageOptions.Value;
            this.memoryStreamManager = memoryStreamManager;
            this.formatUtilities = formatUtilities;

            foreach (AzureBlobContainerClientOptions container in this.storageOptions.BlobContainers)
            {
                this.containers.Add(
                    container.ContainerName,
                    new BlobContainerClient(container.ConnectionString, container.ContainerName));
            }
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
            string containerName = string.Empty;
            BlobContainerClient container = null;

            // We want an exact match here to ensure that container names starting with
            // the same prefix are not mixed up.
            string path = context.Request.Path.Value.TrimStart(SlashChars);
            int index = path.IndexOfAny(SlashChars);
            string nameToMatch = index != -1 ? path.Substring(0, index) : path;

            foreach (string key in this.containers.Keys)
            {
                if (nameToMatch.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    containerName = key;
                    container = this.containers[key];
                    break;
                }
            }

            // Something has gone horribly wrong for this to happen but check anyway.
            if (container is null)
            {
                return null;
            }

            // Blob name should be the remaining path string.
            string blobName = path.Substring(containerName.Length).TrimStart(SlashChars);

            if (string.IsNullOrWhiteSpace(blobName))
            {
                return null;
            }

            BlobClient blob = container.GetBlobClient(blobName);

            if (!await blob.ExistsAsync())
            {
                return null;
            }

            return new AzureBlobStorageImageResolver(blob, this.memoryStreamManager);
        }

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context)
            => this.formatUtilities.GetExtensionFromUri(context.Request.GetDisplayUrl()) != null;

        private bool IsMatch(HttpContext context)
        {
            // Only match loosly here for performance.
            // Path matching conflicts should be dealt with by configuration.
            string path = context.Request.Path.Value.TrimStart(SlashChars);
            foreach (string container in this.containers.Keys)
            {
                if (path.StartsWith(container, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
