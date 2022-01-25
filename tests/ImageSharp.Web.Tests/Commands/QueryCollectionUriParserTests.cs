// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Commands;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Commands
{
    public class QueryCollectionUriParserTests
    {
        [Fact]
        public void QueryCollectionParserExtractsCommands()
        {
            IDictionary<string, string> expected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "width", "400" },
                { "height", "200" }
            };

            HttpContext context = CreateHttpContext();
            IDictionary<string, string> actual = new QueryCollectionRequestParser().ParseRequestCommands(context);

            Assert.Equal(expected, actual);
        }

        private static HttpContext CreateHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/testwebsite.com/image-12345.jpeg";
            httpContext.Request.QueryString = new QueryString("?width=400&height=200");
            return httpContext;
        }
    }
}
