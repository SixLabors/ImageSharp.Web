// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection;

public class ServiceRegistrationExtensionsTests
{
    private static bool IsService<TService>(ServiceDescriptor descriptor)
        => descriptor.ServiceType == typeof(TService);

    private static bool IsService<TService, TImplementation>(ServiceDescriptor descriptor)
        where TImplementation : TService
        => IsServiceImplementationType<TService, TImplementation>(descriptor) ||
           IsServiceImplementationInstance<TService, TImplementation>(descriptor) ||
           IsServiceImplementationFactory<TService, TImplementation>(descriptor);

    private static bool IsServiceImplementationType<TService, TImplementation>(ServiceDescriptor descriptor)
        where TImplementation : TService
        => IsService<TService>(descriptor) &&
           descriptor.ImplementationType == typeof(TImplementation);

    private static bool IsServiceImplementationInstance<TService, TImplementation>(ServiceDescriptor descriptor)
        where TImplementation : TService
        => IsService<TService>(descriptor) &&
           descriptor.ImplementationInstance?.GetType() == typeof(TImplementation);

    private static bool IsServiceImplementationFactory<TService, TImplementation>(ServiceDescriptor descriptor)
        where TImplementation : TService
        => IsService<TService>(descriptor) &&
           (descriptor.ImplementationFactory?.GetMethodInfo().ReturnType == typeof(TImplementation) ||
           descriptor.ImplementationFactory?.Invoke(null)?.GetType() == typeof(TImplementation)); // OK to invoke the factory in tests

    private static IReadOnlyList<ServiceDescriptor> GetCollection<T>(IServiceCollection serviceDescriptors) => serviceDescriptors.Where(x => x.ServiceType == typeof(T)).ToList();

    [Fact]
    public void DefaultServicesAreRegistered()
    {
        var services = new ServiceCollection();
        services.AddImageSharp();

        Assert.Single(services, IsService<FormatUtilities>);
        Assert.Single(services, IsServiceImplementationType<IRequestParser, QueryCollectionRequestParser>);
        Assert.Single(services, IsServiceImplementationType<IImageCache, PhysicalFileSystemCache>);
        Assert.Single(services, IsServiceImplementationType<ICacheKey, UriRelativeLowerInvariantCacheKey>);
        Assert.Single(services, IsServiceImplementationType<ICacheHash, SHA256CacheHash>);
        Assert.Single(services, IsServiceImplementationType<IImageProvider, PhysicalFileSystemProvider>);
        Assert.Single(services, IsServiceImplementationType<IImageWebProcessor, ResizeWebProcessor>);
        Assert.Single(services, IsServiceImplementationType<IImageWebProcessor, FormatWebProcessor>);
        Assert.Single(services, IsServiceImplementationType<IImageWebProcessor, BackgroundColorWebProcessor>);
        Assert.Single(services, IsServiceImplementationType<IImageWebProcessor, QualityWebProcessor>);
        Assert.Single(services, IsService<CommandParser>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, IntegralNumberConverter<sbyte>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, IntegralNumberConverter<byte>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, IntegralNumberConverter<short>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, IntegralNumberConverter<ushort>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, IntegralNumberConverter<int>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, IntegralNumberConverter<uint>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, IntegralNumberConverter<long>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, IntegralNumberConverter<ulong>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, SimpleCommandConverter<decimal>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, SimpleCommandConverter<float>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, SimpleCommandConverter<double>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, SimpleCommandConverter<string>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, SimpleCommandConverter<bool>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<sbyte>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<byte>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<short>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<ushort>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<int>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<uint>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<long>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<ulong>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<decimal>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<float>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<double>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<string>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ArrayConverter<bool>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<sbyte>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<byte>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<short>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<ushort>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<int>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<uint>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<long>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<ulong>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<decimal>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<float>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<double>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<string>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ListConverter<bool>>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, ColorConverter>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, EnumConverter>);
    }

    [Fact]
    public void CanSetRequestParser()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.SetRequestParser<MockRequestParser>();
        Assert.Single(services, IsService<IRequestParser>);
        Assert.Single(services, IsServiceImplementationType<IRequestParser, MockRequestParser>);

        builder.SetRequestParser(_ => new MockRequestParser());
        Assert.Single(services, IsService<IRequestParser>);
        Assert.Single(services, IsServiceImplementationFactory<IRequestParser, MockRequestParser>);
    }

    [Fact]
    public void CanSetCache()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.SetCache<MockImageCache>();
        Assert.Single(services, IsService<IImageCache>);
        Assert.Single(services, IsServiceImplementationType<IImageCache, MockImageCache>);

        builder.SetCache(_ => new MockImageCache());
        Assert.Single(services, IsService<IImageCache>);
        Assert.Single(services, IsServiceImplementationFactory<IImageCache, MockImageCache>);
    }

    [Fact]
    public void CanSetCacheKey()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.SetCacheKey<MockCacheKey>();
        Assert.Single(services, IsService<ICacheKey>);
        Assert.Single(services, IsServiceImplementationType<ICacheKey, MockCacheKey>);

        builder.SetCacheKey(_ => new MockCacheKey());
        Assert.Single(services, IsService<ICacheKey>);
        Assert.Single(services, IsServiceImplementationFactory<ICacheKey, MockCacheKey>);
    }

    [Fact]
    public void CanSetCacheHash()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.SetCacheHash<MockCacheHash>();
        Assert.Single(services, IsService<ICacheHash>);
        Assert.Single(services, IsServiceImplementationType<ICacheHash, MockCacheHash>);

        builder.SetCacheHash(_ => new MockCacheHash());
        Assert.Single(services, IsService<ICacheHash>);
        Assert.Single(services, IsServiceImplementationFactory<ICacheHash, MockCacheHash>);
    }

    [Fact]
    public void CanAddRemoveImageProviders()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProvider<MockImageProvider>();
        Assert.Single(services, IsService<IImageProvider, MockImageProvider>);
        Assert.Single(services, IsServiceImplementationType<IImageProvider, MockImageProvider>);

        builder.RemoveProvider<MockImageProvider>();
        Assert.DoesNotContain(services, IsService<IImageProvider, MockImageProvider>);
    }

    [Fact]
    public void CanInsertRemoveImageProviders()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.InsertProvider<MockImageProvider>(0);
        Assert.Single(services, IsService<IImageProvider, MockImageProvider>);
        Assert.Single(services, IsServiceImplementationType<IImageProvider, MockImageProvider>);

        IReadOnlyList<ServiceDescriptor> providers = GetCollection<IImageProvider>(services);
        Assert.Equal(2, providers.Count);
        Assert.True(IsService<IImageProvider, MockImageProvider>(providers[0]));
        Assert.True(IsServiceImplementationType<IImageProvider, MockImageProvider>(providers[0]));

        builder.RemoveProvider<MockImageProvider>();
        Assert.DoesNotContain(services, IsService<IImageProvider, MockImageProvider>);
    }

    [Fact]
    public void CanInsertIdempotentImageProviders()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.InsertProvider<MockImageProvider>(0);
        builder.InsertProvider<MockImageProvider>(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.InsertProvider<MockImageProvider>(2));

        Assert.Single(services, IsService<IImageProvider, MockImageProvider>);
        Assert.Single(services, IsServiceImplementationType<IImageProvider, MockImageProvider>);

        IReadOnlyList<ServiceDescriptor> providers = GetCollection<IImageProvider>(services);
        Assert.Equal(2, providers.Count);
        Assert.True(IsService<IImageProvider, MockImageProvider>(providers[1]));
        Assert.True(IsServiceImplementationType<IImageProvider, MockImageProvider>(providers[1]));
    }

    [Fact]
    public void CanAddRemoveFactoryImageProviders()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProvider(_ => new MockImageProvider());
        Assert.Single(services, IsService<IImageProvider, MockImageProvider>);
        Assert.Single(services, IsServiceImplementationFactory<IImageProvider, MockImageProvider>);

        builder.RemoveProvider<MockImageProvider>();
        Assert.DoesNotContain(services, IsService<IImageProvider, MockImageProvider>);
    }

    [Fact]
    public void CanInsertIdempotentFactoryImageProviders()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.InsertProvider(0, _ => new MockImageProvider());
        builder.InsertProvider(1, _ => new MockImageProvider());

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.InsertProvider(2, _ => new MockImageProvider()));

        Assert.Single(services, IsService<IImageProvider, MockImageProvider>);
        Assert.Single(services, IsServiceImplementationFactory<IImageProvider, MockImageProvider>);

        IReadOnlyList<ServiceDescriptor> providers = GetCollection<IImageProvider>(services);
        Assert.Equal(2, providers.Count);
        Assert.True(IsService<IImageProvider, MockImageProvider>(providers[1]));
        Assert.True(IsServiceImplementationFactory<IImageProvider, MockImageProvider>(providers[1]));
    }

    [Fact]
    public void CanInsertRemoveFactoryImageProviders()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.InsertProvider(0, _ => new MockImageProvider());
        Assert.Single(services, IsService<IImageProvider, MockImageProvider>);
        Assert.Single(services, IsServiceImplementationFactory<IImageProvider, MockImageProvider>);

        IReadOnlyList<ServiceDescriptor> providers = GetCollection<IImageProvider>(services);
        Assert.Equal(2, providers.Count);
        Assert.True(IsService<IImageProvider, MockImageProvider>(providers[0]));
        Assert.True(IsServiceImplementationFactory<IImageProvider, MockImageProvider>(providers[0]));

        builder.RemoveProvider<MockImageProvider>();
        Assert.DoesNotContain(services, IsService<IImageProvider, MockImageProvider>);
    }

    [Fact]
    public void CanAddRemoveAllImageProviders()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProvider<MockImageProvider>();
        Assert.Single(services, IsService<IImageProvider, MockImageProvider>);
        Assert.Single(services, IsServiceImplementationType<IImageProvider, MockImageProvider>);

        builder.ClearProviders();
        Assert.DoesNotContain(services, IsService<IImageProvider>);
    }

    [Fact]
    public void CanAddRemoveAllFactoryImageProviders()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProvider(_ => new MockImageProvider());
        Assert.Single(services, IsService<IImageProvider, MockImageProvider>);
        Assert.Single(services, IsServiceImplementationFactory<IImageProvider, MockImageProvider>);

        builder.ClearProviders();
        Assert.DoesNotContain(services, IsService<IImageProvider>);
    }

    [Fact]
    public void CanAddRemoveImageProcessors()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProcessor<MockWebProcessor>();
        Assert.Single(services, IsService<IImageWebProcessor, MockWebProcessor>);
        Assert.Single(services, IsServiceImplementationType<IImageWebProcessor, MockWebProcessor>);

        builder.RemoveProcessor<MockWebProcessor>();
        Assert.DoesNotContain(services, IsService<IImageWebProcessor, MockWebProcessor>);
    }

    [Fact]
    public void CanAddRemoveFactoryImageProcessors()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProcessor(_ => new MockWebProcessor());
        Assert.Single(services, IsService<IImageWebProcessor, MockWebProcessor>);
        Assert.Single(services, IsServiceImplementationFactory<IImageWebProcessor, MockWebProcessor>);

        builder.RemoveProcessor<MockWebProcessor>();
        Assert.DoesNotContain(services, IsService<IImageWebProcessor, MockWebProcessor>);
    }

    [Fact]
    public void CanAddRemoveAllImageProcessors()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProcessor<MockWebProcessor>();
        Assert.Single(services, IsService<IImageWebProcessor, MockWebProcessor>);
        Assert.Single(services, IsServiceImplementationType<IImageWebProcessor, MockWebProcessor>);

        builder.ClearProcessors();
        Assert.DoesNotContain(services, IsService<IImageWebProcessor>);
    }

    [Fact]
    public void CanAddRemoveAllFactoryImageProcessors()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProcessor(_ => new MockWebProcessor());
        Assert.Single(services, IsService<IImageWebProcessor, MockWebProcessor>);
        Assert.Single(services, IsServiceImplementationFactory<IImageWebProcessor, MockWebProcessor>);

        builder.ClearProcessors();
        Assert.DoesNotContain(services, IsService<IImageWebProcessor>);
    }

    [Fact]
    public void CanAddRemoveCommandConverters()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddConverter<MockCommandConverter>();
        Assert.Single(services, IsService<ICommandConverter, MockCommandConverter>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, MockCommandConverter>);

        builder.RemoveConverter<MockCommandConverter>();
        Assert.DoesNotContain(services, IsService<ICommandConverter, MockCommandConverter>);
    }

    [Fact]
    public void CanAddRemoveFactoryCommandConverters()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddConverter(_ => new MockCommandConverter());
        Assert.Single(services, IsService<ICommandConverter, MockCommandConverter>);
        Assert.Single(services, IsServiceImplementationFactory<ICommandConverter, MockCommandConverter>);

        builder.RemoveConverter<MockCommandConverter>();
        Assert.DoesNotContain(services, IsService<ICommandConverter, MockCommandConverter>);
    }

    [Fact]
    public void CanAddRemoveAllCommandConverters()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddConverter<MockCommandConverter>();
        Assert.Single(services, IsService<ICommandConverter, MockCommandConverter>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, MockCommandConverter>);

        builder.ClearConverters();
        Assert.DoesNotContain(services, IsService<ICommandConverter>);
    }

    [Fact]
    public void CanAddRemoveAllFactoryCommandConverters()
    {
        var services = new ServiceCollection();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddConverter(_ => new MockCommandConverter());
        Assert.Single(services, IsService<ICommandConverter, MockCommandConverter>);
        Assert.Single(services, IsServiceImplementationFactory<ICommandConverter, MockCommandConverter>);

        builder.ClearConverters();
        Assert.DoesNotContain(services, IsService<ICommandConverter>);
    }
}
