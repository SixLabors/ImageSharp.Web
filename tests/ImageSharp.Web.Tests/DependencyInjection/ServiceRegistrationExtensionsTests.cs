// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class ServiceRegistrationExtensionsTests
    {
        [Fact]
        public void DefaultServicesAreRegistered()
        {
            var services = new ServiceCollection();
            services.AddImageSharp();

            Assert.Contains(services, x => x.ServiceType == typeof(FormatUtilities));
            Assert.Contains(services, x => x.ServiceType == typeof(IRequestParser) && x.ImplementationType == typeof(QueryCollectionRequestParser));
            Assert.Contains(services, x => x.ServiceType == typeof(IImageCache) && x.ImplementationType == typeof(PhysicalFileSystemCache));
            Assert.Contains(services, x => x.ServiceType == typeof(ICacheHash) && x.ImplementationType == typeof(CacheHash));
            Assert.Contains(services, x => x.ServiceType == typeof(IImageProvider) && x.ImplementationType == typeof(PhysicalFileSystemProvider));
            Assert.Contains(services, x => x.ServiceType == typeof(IImageWebProcessor) && x.ImplementationType == typeof(ResizeWebProcessor));
            Assert.Contains(services, x => x.ServiceType == typeof(IImageWebProcessor) && x.ImplementationType == typeof(FormatWebProcessor));
            Assert.Contains(services, x => x.ServiceType == typeof(IImageWebProcessor) && x.ImplementationType == typeof(BackgroundColorWebProcessor));
        }

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
        public void CanAddRemoveAllImageProviders()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                .AddProvider<MockImageProvider>();

            Assert.Contains(services, x => x.ImplementationType == typeof(MockImageProvider));

            builder.ClearProviders();

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockImageProvider));
        }

        [Fact]
        public void CanAddRemoveAllFactoryImageProviders()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                                      .AddProvider(_ => new MockImageProvider());

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockImageProvider));

            Assert.Contains(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockImageProvider));

            builder.ClearProviders();

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

        [Fact]
        public void CanAddRemoveAllImageProcessors()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                                      .AddProcessor<MockWebProcessor>();

            Assert.Contains(services, x => x.ImplementationType == typeof(MockWebProcessor));

            builder.ClearProcessors();

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockWebProcessor));
        }

        [Fact]
        public void CanAddRemoveAllFactoryImageProcessors()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                                  .AddProcessor(_ => new MockWebProcessor());

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockWebProcessor));

            Assert.Contains(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockWebProcessor));

            builder.ClearProcessors();

            Assert.DoesNotContain(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockWebProcessor));
        }
    }
}
