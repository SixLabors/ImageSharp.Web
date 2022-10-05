// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.
using System;

namespace SixLabors.ImageSharp.Web.Caching.AWS
{
    /// <summary>
    /// Configuration options for the <see cref="AWSS3StorageCache"/> provider.
    /// </summary>
    public class AWSS3StorageCacheOptions : IAWSS3BucketClientOptions
    {
        /// <inheritdoc/>
        public string Region { get; set; }

        /// <inheritdoc/>
        public string BucketName { get; set; }

        /// <inheritdoc/>
        public string AccessKey { get; set; }

        /// <inheritdoc/>
        public string AccessSecret { get; set; }

        /// <inheritdoc/>
        public string Endpoint { get; set; }

        /// <inheritdoc/>
        public bool UseAccelerateEndpoint { get; set; }

        /// <inheritdoc/>
        public TimeSpan? Timeout { get; set; }
    }
}
