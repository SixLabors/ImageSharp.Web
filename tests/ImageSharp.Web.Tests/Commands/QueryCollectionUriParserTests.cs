// Copyright (c) Six Labors and contributors.
// Licensed under the GNU Affero General Public License, Version 3.

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

            HttpContext context = TestHelpers.CreateHttpContext();
            IDictionary<string, string> actual = new QueryCollectionRequestParser().ParseRequestCommands(context);

            Assert.Equal(expected, actual);
        }
    }
}