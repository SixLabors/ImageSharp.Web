// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Web.Caching.Azure
{
    /// <summary>
    /// Configuration options for the <see cref="AzureBlobStorageCache"/>.
    /// </summary>
    public class AzureBlobStorageCacheOptions : IAzureBlobContainerClientOptions
    {
        /// <inheritdoc/>
        public string ConnectionString { get; set; }

        /// <inheritdoc/>
        public string ContainerName { get; set; }
    }
}
