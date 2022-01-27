// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace SixLabors.ImageSharp.Web.Providers.AWS
{
    /// <summary>
    /// Configuration options for the <see cref="AWSS3StorageImageProviderOptions"/> provider.
    /// </summary>
    public class AWSS3StorageImageProviderOptions
    {
        /// <summary>
        /// Gets or sets the collection of blob container client options.
        /// </summary>
        public ICollection<AWSS3BucketClientOptions> S3Buckets { get; set; } = new HashSet<AWSS3BucketClientOptions>();
    }

    /// <summary>
    /// Configuration options for the <see cref="AWSS3StorageImageProvider"/> provider.
    /// </summary>
    public class AWSS3BucketClientOptions
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
