// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Configuration options for the <see cref="AWSS3StorageImageProvider"/> provider.
    /// </summary>
    public class AWSS3StorageImageProviderOptions
    {
        /// <summary>
        /// Gets or sets the bucket name.
        /// Must conform to AWS S3 bucket naming guidelines.
        /// </summary>
        public string BucketName { get; set; }
    }
}
