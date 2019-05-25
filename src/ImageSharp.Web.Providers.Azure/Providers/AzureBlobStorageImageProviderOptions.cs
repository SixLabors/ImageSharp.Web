// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Azure.Storage.Blob;

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Configuration options for the <see cref="AzureBlobStorageImageProvider"/> provider.
    /// </summary>
    public class AzureBlobStorageImageProviderOptions
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the container name.
        /// Must conform to Azure Blob Storage containiner naming guidlines.
        /// <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata#container-names"/>
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the value indicating the level of public access that is allowed on the container.
        /// </summary>
        public BlobContainerPublicAccessType AccessType { get; set; } = BlobContainerPublicAccessType.Blob;
    }
}
