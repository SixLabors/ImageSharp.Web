// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.WindowsAzure.Storage.Blob;

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Configuration options for the Azure Blob Storage provider middleware.
    /// </summary>
    public class BlobStorageImageProviderOptions
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the value indicating the level of public access that is allowed on the container.
        /// </summary>
        public BlobContainerPublicAccessType AccessType { get; set; } = BlobContainerPublicAccessType.Blob;
    }
}
