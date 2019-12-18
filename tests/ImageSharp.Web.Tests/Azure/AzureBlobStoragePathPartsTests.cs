using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Web.Providers;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Azure
{
    public class AzureBlobStoragePathPartsTests
    {
        private static readonly string RoutePrefix = "assets";

        [Fact]
        public void PathPartsContainerCorrect()
        {
            const string path = "/assets/products/productImage.jpg";

            var pathParts = new AzureBlobStoragePathParts(path, RoutePrefix);

            Assert.Equal("products", pathParts.ContainerName);
        }

        [Fact]
        public void PathPartsBlobFilenameCorrect()
        {
            const string path = "/assets/products/productImage.jpg";

            var pathParts = new AzureBlobStoragePathParts(path, RoutePrefix);

            Assert.Equal("productImage.jpg", pathParts.BlobFilename);
        }

        [Fact]
        public void PathPartsBlobFilenameWithSubFolderCorrect()
        {
            const string path = "/assets/products/1/productImage.jpg";

            var pathParts = new AzureBlobStoragePathParts(path, RoutePrefix);

            Assert.Equal("1/productImage.jpg", pathParts.BlobFilename);
        }

        [Fact]
        public void PathPartsBlobFilenameNotFoundCorrect()
        {
            const string path = "/assets/products/";

            var pathParts = new AzureBlobStoragePathParts(path, RoutePrefix);

            Assert.Null(pathParts.BlobFilename);
        }

        [Fact]
        public void PathPartsBlobFilenameNotFoundNoTrailingSlashCorrect()
        {
            const string path = "/assets/products";

            var pathParts = new AzureBlobStoragePathParts(path, RoutePrefix);

            Assert.Null(pathParts.BlobFilename);
        }

        [Fact]
        public void PathPartsBlobFilenameWithForwardSlashCorrect()
        {
            const string path = "\\assets\\products\\productImage.jpg";

            var pathParts = new AzureBlobStoragePathParts(path, RoutePrefix);

            Assert.Equal("productImage.jpg", pathParts.BlobFilename);
        }
    }
}
