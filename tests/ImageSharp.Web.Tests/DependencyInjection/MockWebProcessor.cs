using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web.Processors;
using System;
using System.Collections.Generic;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class MockWebProcessor : IImageWebProcessor
    {
        public IEnumerable<string> Commands => throw new NotImplementedException();

        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands) => throw new NotImplementedException();
    }
}
