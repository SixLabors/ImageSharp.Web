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
        /// Gets or sets the AWS region endpoint (us-east-1/us-west-1/ap-southeast-2).
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the AWS bucket name.
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// Gets or sets the AWS key - Can be used to override keys provided by the environment.
        /// If deploying inside an EC2 instance AWS keys will already be available via environment
        /// variables and don't need to be specified. Follow AWS best security practices on  <see href="https://docs.aws.amazon.com/general/latest/gr/aws-access-keys-best-practices.html"/>.
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the AWS secret - Can be used to override keys provided by the environment.
        /// If deploying inside an EC2 instance AWS keys will already be available via environment
        /// variables and don't need to be specified. Follow AWS best security practices on  <see href="https://docs.aws.amazon.com/general/latest/gr/aws-access-keys-best-practices.html"/>.
        /// </summary>
        public string AccessSecret { get; set; }

        /// <summary>
        /// Gets or sets the AWS endpoint - used for testing to over region endpoint allowing it
        /// to be set to localhost.
        /// </summary>
        public string Endpoint { get; set; }
    }
}
