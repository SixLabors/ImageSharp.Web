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
        /// Gets or sets the Azure Blob Storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the route prefix for processing azure assets.
        /// Url format would be: /{RoutePrefix}/{ContainerName}/{FileName}
        /// </summary>
        public string RoutePrefix { get; set; }
    }
}
