using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp.Web.Helpers;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Helpers
{
    public class FormatHelpersTests
    {
        public static IEnumerable<object[]> DefaultExtensions = 
            Configuration.Default.ImageFormats.SelectMany(f => f.FileExtensions.Select(e => new object[] { e, e }));

        [Theory]
        [MemberData(nameof(DefaultExtensions))]
        public void GetExtensionShouldMatchDefaultExtensions(string expected, string ext)
        {
            string uri = $"http://www.example.org/some/path/to/image.{ext}?width=300";
            Assert.Equal(expected, FormatHelper.GetExtension(Configuration.Default, uri));
        }

        [Fact]
        public void GetExtensionShouldNotMatchExtensionWithoutDotPrefix()
        {
            string uri = "http://www.example.org/some/path/to/bmpimage";
            Assert.Null(FormatHelper.GetExtension(Configuration.Default, uri));
        }

        [Fact]
        public void GetExtensionShouldIgnoreQueryStringWithoutFormatParamter()
        {
            string uri = "http://www.example.org/some/path/to/image.bmp?width=300&foo=.png";
            Assert.Equal("bmp", FormatHelper.GetExtension(Configuration.Default, uri));
        }

        [Fact]
        public void GetExtensionShouldAcknowledgeQueryStringFormatParameter()
        {
            string uri = "http://www.example.org/some/path/to/image.bmp?width=300&format=png";
            Assert.Equal("png", FormatHelper.GetExtension(Configuration.Default, uri));
        }
    }
}
