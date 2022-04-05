// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.AspNetCore.Hosting;

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Returns images from the web root file provider.
    /// </summary>
    public sealed class WebRootImageProvider : FileProviderImageProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebRootImageProvider"/> class.
        /// </summary>
        /// <param name="environment">The web hosting environment.</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public WebRootImageProvider(
#if NETCOREAPP2_1
            IHostingEnvironment environment,
#else
            IWebHostEnvironment environment,
#endif
            FormatUtilities formatUtilities)
            : base(environment.WebRootFileProvider, ProcessingBehavior.CommandOnly, formatUtilities)
        {
        }
    }
}
