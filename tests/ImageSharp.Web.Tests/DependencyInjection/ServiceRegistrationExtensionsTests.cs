﻿using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;
using System;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class ServiceRegistrationExtensionsTests
    {
        [Fact]
        public void CanAddRemoveImageProviders()
        {
            void RemoveServices(IServiceCollection services)
            {
                var builder = services.AddImageSharp()
                                      .AddProvider<MockImageProvider>();

                Assert.Contains(services, x => x.ImplementationType == typeof(MockImageProvider));

                builder.RemoveProvider<MockImageProvider>();

                Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockImageProvider));
            }

            using (TestServer server = ImageSharpTestServer.Create(ImageSharpTestServer.DefaultConfig, RemoveServices))
            {
            }
        }

        [Fact]
        public void CanAddRemoveFactoryImageProviders()
        {
            void RemoveServices(IServiceCollection services)
            {
                var builder = services.AddImageSharp()
                                      .AddProvider(sp => new MockImageProvider());

                Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockImageProvider));

                Assert.Contains(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockImageProvider));

                builder.RemoveProvider<MockImageProvider>();

                Assert.DoesNotContain(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockImageProvider));
            }

            using (TestServer server = ImageSharpTestServer.Create(ImageSharpTestServer.DefaultConfig, RemoveServices))
            {
            }
        }

        [Fact]
        public void CanAddRemoveImageProcessors()
        {
            void RemoveServices(IServiceCollection services)
            {
                var builder = services.AddImageSharp()
                                      .AddProcessor<MockWebProcessor>();

                Assert.Contains(services, x => x.ImplementationType == typeof(MockWebProcessor));

                builder.RemoveProcessor<MockWebProcessor>();

                Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockWebProcessor));
            }

            using (TestServer server = ImageSharpTestServer.Create(ImageSharpTestServer.DefaultConfig, RemoveServices))
            {
            }
        }

        [Fact]
        public void CanAddRemoveFactoryImageProcessors()
        {
            void RemoveServices(IServiceCollection services)
            {
                var builder = services.AddImageSharp()
                                      .AddProcessor(sp => new MockWebProcessor());

                Assert.DoesNotContain(services, x => x.ImplementationType == typeof(MockWebProcessor));

                Assert.Contains(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockWebProcessor));

                builder.RemoveProcessor<MockWebProcessor>();

                Assert.DoesNotContain(services, x => x.ImplementationFactory?.Method.ReturnType == typeof(MockWebProcessor));
            }

            using (TestServer server = ImageSharpTestServer.Create(ImageSharpTestServer.DefaultConfig, RemoveServices))
            {
            }
        }
    }
}
