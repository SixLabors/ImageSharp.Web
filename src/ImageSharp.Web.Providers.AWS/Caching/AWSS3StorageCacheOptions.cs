// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Web.Caching.AWS
{
    /// <summary>
    /// Configuration options for the <see cref="AWSS3StorageCache"/>.
    /// </summary>
    public class AWSS3StorageCacheOptions
    {
        /// <summary>
        /// Gets or sets the AWS region name.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the AWS bucket name.
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// Gets or sets the AWS key - optional.
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the AWS secret - optional.
        /// </summary>
        public string AccessSecret { get; set; }

        /// <summary>
        /// Gets or sets the AWS endpoint - override for testing
        /// </summary>
        public string Endpoint { get; set; }
    }
}
