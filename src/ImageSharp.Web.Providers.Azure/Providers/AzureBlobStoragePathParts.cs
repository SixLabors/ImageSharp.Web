// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Text;

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Components of Azure blob URL path
    /// </summary>
    public class AzureBlobStoragePathParts
    {
        /// <summary>
        /// Character array to remove from paths.
        /// </summary>
        private static readonly char[] SlashChars = { '\\', '/' };

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStoragePathParts"/> class.
        /// </summary>
        /// <param name="path">URL path to parse</param>
        /// <param name="routePrefix">Azure route prefix</param>
        public AzureBlobStoragePathParts(string path, string routePrefix)
        {
            this.RoutePrefix = routePrefix;

            this.GetPartsForPath(path);
        }

        /// <summary>
        /// Gets the route prefix
        /// </summary>
        public string RoutePrefix { get; private set; }

        /// <summary>
        /// Gets the Azure Blob Container name.
        /// </summary>
        public string ContainerName { get; private set; }

        /// <summary>
        /// Gets the Azue Blob File Name. Null if one could not be found
        /// </summary>
        public string BlobFilename { get; private set; }

        private void GetPartsForPath(string path)
        {
            // Strip the leading slash and route prefix from the HTTP request path
            // Path has already been correctly parsed before here.
            string pathMinusPrefix = path.TrimStart(SlashChars)
                                     .Substring(this.RoutePrefix.Length)
                                     .TrimStart(SlashChars);

            // Split path into container and blob filename
            foreach (char slash in SlashChars)
            {
                int indexOfSlash = pathMinusPrefix.IndexOf(slash);

                if (indexOfSlash < 0 || pathMinusPrefix.Length <= (indexOfSlash + 1))
                {
                    continue;
                }

                this.ContainerName = pathMinusPrefix.Substring(0, indexOfSlash);
                this.BlobFilename = pathMinusPrefix.Substring(indexOfSlash + 1);

                return;
            }
        }
    }
}
