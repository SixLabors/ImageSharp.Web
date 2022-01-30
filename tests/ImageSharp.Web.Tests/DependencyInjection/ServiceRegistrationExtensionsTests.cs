// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
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
            Assert.Contains(services, x => x.ServiceType == typeof(ICacheHash) && x.ImplementationType == typeof(SHA256CacheHash));
            Assert.Contains(services, x => x.ServiceType == typeof(IImageProvider) && x.ImplementationType == typeof(PhysicalFileSystemProvider));
            Assert.Contains(services, x => x.ServiceType == typeof(IImageWebProcessor) && x.ImplementationType == typeof(ResizeWebProcessor));
            Assert.Contains(services, x => x.ServiceType == typeof(IImageWebProcessor) && x.ImplementationType == typeof(FormatWebProcessor));
            Assert.Contains(services, x => x.ServiceType == typeof(IImageWebProcessor) && x.ImplementationType == typeof(BackgroundColorWebProcessor));
            Assert.Contains(services, x => x.ServiceType == typeof(IImageWebProcessor) && x.ImplementationType == typeof(QualityWebProcessor));
            Assert.Contains(services, x => x.ServiceType == typeof(CommandParser));

            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(IntegralNumberConverter<sbyte>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(IntegralNumberConverter<byte>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(IntegralNumberConverter<short>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(IntegralNumberConverter<ushort>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(IntegralNumberConverter<int>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(IntegralNumberConverter<uint>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(IntegralNumberConverter<long>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(IntegralNumberConverter<ulong>));

            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(SimpleCommandConverter<decimal>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(SimpleCommandConverter<float>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(SimpleCommandConverter<double>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(SimpleCommandConverter<string>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(SimpleCommandConverter<bool>));

            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<sbyte>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<byte>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<short>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<ushort>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<int>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<uint>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<long>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<ulong>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<decimal>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<float>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<double>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<string>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ArrayConverter<bool>));

            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<sbyte>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<byte>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<short>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<ushort>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<int>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<uint>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<long>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<ulong>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<decimal>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<float>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<double>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<string>));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ListConverter<bool>));

            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(ColorConverter));
            Assert.Contains(services, x => x.ServiceType == typeof(ICommandConverter) && x.ImplementationType == typeof(EnumConverter));
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

        [Fact]
        public void CanAddRemoveCommandConverters()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                                      .AddConverter<MockCommandConverter>();

            Assert.Contains(services, x => x.ImplementationType == typeof(MockCommandConverter));

            builder.RemoveConverter<MockCommandConverter>();

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockCommandConverter));
        }

        [Fact]
        public void CanAddRemoveFactoryCommandConverters()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                                  .AddConverter(_ => new MockCommandConverter());

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockCommandConverter));

            Assert.Contains(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockCommandConverter));

            builder.RemoveConverter<MockCommandConverter>();

            Assert.DoesNotContain(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockCommandConverter));
        }

        [Fact]
        public void CanAddRemoveAllCommandConverters()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                                      .AddConverter<MockCommandConverter>();

            Assert.Contains(services, x => x.ImplementationType == typeof(MockCommandConverter));

            builder.ClearConverters();

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockCommandConverter));
        }

        [Fact]
        public void CanAddRemoveAllFactoryCommandConverters()
        {
            var services = new ServiceCollection();

            IImageSharpBuilder builder = services.AddImageSharp()
                                  .AddConverter(_ => new MockCommandConverter());

            Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockCommandConverter));

            Assert.Contains(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockCommandConverter));

            builder.ClearConverters();

            Assert.DoesNotContain(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockCommandConverter));
        }
    }
}
