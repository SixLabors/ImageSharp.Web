using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp.Web.Helpers;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Helpers
{
    public class FormatHelpersTests
    {
        public static IEnumerable<object[]> DefaultExtensions =
            Configuration.Default.ImageFormats.SelectMany(f => f.FileExtensions.Select(e => new object[] { e, e }));

        private static readonly FormatHelper formatHelper = new FormatHelper(Configuration.Default);

        [Theory]
        [MemberData(nameof(DefaultExtensions))]
        public void GetExtensionShouldMatchDefaultExtensions(string expected, string ext)
        {
            string uri = $"http://www.example.org/some/path/to/image.{ext}?width=300";
            Assert.Equal(expected, formatHelper.GetExtension(uri));
        }

        [Fact]
        public void GetExtensionShouldNotMatchExtensionWithoutDotPrefix()
        {
            const string uri = "http://www.example.org/some/path/to/bmpimage";
            Assert.Null(formatHelper.GetExtension(uri));
        }

        [Fact]
        public void GetExtensionShouldIgnoreQueryStringWithoutFormatParamter()
        {
            const string uri = "http://www.example.org/some/path/to/image.bmp?width=300&foo=.png";
            Assert.Equal("bmp", formatHelper.GetExtension(uri));
        }

        [Fact]
        public void GetExtensionShouldAcknowledgeQueryStringFormatParameter()
        {
            const string uri = "http://www.example.org/some/path/to/image.bmp?width=300&format=png";
            Assert.Equal("png", formatHelper.GetExtension(uri));
        }
    }
}
