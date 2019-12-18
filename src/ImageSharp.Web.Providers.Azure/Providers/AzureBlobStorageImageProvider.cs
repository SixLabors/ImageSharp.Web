// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Shared.Protocol;
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
        /// The cloud storage account.
        /// </summary>
        private readonly CloudStorageAccount storageAccount;

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
            this.storageAccount = CloudStorageAccount.Parse(this.storageOptions.ConnectionString);
        }

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

            string pathMinusPrefix = context.Request.Path.Value.TrimStart(SlashChars)
                                     .Substring(this.storageOptions.RoutePrefix.Length)
                                     .TrimStart(SlashChars);

            string blobName = null;
            string containerName = null;

            foreach (char slash in SlashChars)
            {
                int indexOfSlash = pathMinusPrefix.IndexOf(slash);

                if (indexOfSlash < 0 || pathMinusPrefix.Length <= (indexOfSlash + 1))
                {
                    continue;
                }

                containerName = pathMinusPrefix.Substring(0, indexOfSlash);
                blobName = pathMinusPrefix.Substring(indexOfSlash + 1);

                break;
            }

            if (string.IsNullOrWhiteSpace(blobName))
            {
                return null;
            }

            CloudBlobClient client = this.storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(containerName);

            if (!container.Exists())
            {
                return null;
            }

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            if (!await blob.ExistsAsync().ConfigureAwait(false))
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
            return path.StartsWith(this.storageOptions.RoutePrefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
