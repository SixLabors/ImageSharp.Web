// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class ServiceRegistrationExtensionsTests
    {
        [Fact]
        public void CanAddRemoveImageProviders()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                .AddProvider<MockImageProvider>();

            Assert.Contains(services, x => x.ImplementationType == typeof(MockImageProvider));

            builder.RemoveProvider<MockImageProvider>();

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockImageProvider));
        }

        [Fact]
        public void CanAddRemoveFactoryImageProviders()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                                      .AddProvider(_ => new MockImageProvider());

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockImageProvider));

            Assert.Contains(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockImageProvider));

            builder.RemoveProvider<MockImageProvider>();

            Assert.DoesNotContain(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockImageProvider));
        }

        [Fact]
        public void CanAddRemoveImageProcessors()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                                      .AddProcessor<MockWebProcessor>();

            Assert.Contains(services, x => x.ImplementationType == typeof(MockWebProcessor));

            builder.RemoveProcessor<MockWebProcessor>();

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockWebProcessor));
        }

        [Fact]
        public void CanAddRemoveFactoryImageProcessors()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                                  .AddProcessor(_ => new MockWebProcessor());

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockWebProcessor));

            Assert.Contains(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockWebProcessor));

            builder.RemoveProcessor<MockWebProcessor>();

            Assert.DoesNotContain(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockWebProcessor));
        }
    }
}
