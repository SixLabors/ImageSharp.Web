// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.AspNetCore.Http;

namespace SixLabors.ImageSharp.Web.Tests
{
    public static class TestHelpers
    {
        public static HttpContext CreateHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/testwebsite.com/image-12345.jpeg";
            httpContext.Request.QueryString = new QueryString("?width=400&height=200");
            return httpContext;
        }

        public static HttpContext CreateHttpContext(string path, string query)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = path;
            httpContext.Request.QueryString = new QueryString(query);
            return httpContext;
        }
    }
}
