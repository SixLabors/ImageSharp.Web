// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Middleware;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Helpers
{
    public class FormatUtilitiesTests
    {
        public static IEnumerable<object[]> DefaultExtensions =
            Configuration.Default.ImageFormats.SelectMany(f => f.FileExtensions.Select(e => new object[] { e, e }));

        private static readonly FormatUtilities FormatUtilities = new(Options.Create(new ImageSharpMiddlewareOptions()));

        [Theory]
        [MemberData(nameof(DefaultExtensions))]
        public void GetExtensionShouldMatchDefaultExtensions(string expected, string ext)
        {
            string uri = $"http://www.example.org/some/path/to/image.{ext}?width=300";
            Assert.Equal(expected, FormatUtilities.GetExtensionFromUri(uri));
        }

        [Fact]
        public void GetExtensionShouldNotMatchExtensionWithoutDotPrefix()
        {
            const string uri = "http://www.example.org/some/path/to/bmpimage";
            Assert.Null(FormatUtilities.GetExtensionFromUri(uri));
        }

        [Fact]
        public void GetExtensionShouldIgnoreQueryStringWithoutFormatParamter()
        {
            const string uri = "http://www.example.org/some/path/to/image.bmp?width=300&foo=.png";
            Assert.Equal("bmp", FormatUtilities.GetExtensionFromUri(uri));
        }

        [Fact]
        public void GetExtensionShouldAcknowledgeQueryStringFormatParameter()
        {
            const string uri = "http://www.example.org/some/path/to/image.bmp?width=300&format=png";
            Assert.Equal("png", FormatUtilities.GetExtensionFromUri(uri));
        }

        [Fact]
        public void GetExtensionShouldRejectInvalidQueryStringFormatParameter()
        {
            const string uri = "http://www.example.org/some/path/to/image.bmp?width=300&format=invalid";
            Assert.Null(FormatUtilities.GetExtensionFromUri(uri));
        }
    }
}
