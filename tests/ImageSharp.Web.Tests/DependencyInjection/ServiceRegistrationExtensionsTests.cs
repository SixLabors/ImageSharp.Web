// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
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

    private static List<ServiceDescriptor> GetCollection<T>(IServiceCollection serviceDescriptors) => [.. serviceDescriptors.Where(x => x.ServiceType == typeof(T))];

    [Fact]
    public void DefaultServicesAreRegistered()
    {
        ServiceCollection services = new();
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
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.SetRequestParser<FakeRequestParser>();
        Assert.Single(services, IsService<IRequestParser>);
        Assert.Single(services, IsServiceImplementationType<IRequestParser, FakeRequestParser>);

        builder.SetRequestParser(_ => new FakeRequestParser());
        Assert.Single(services, IsService<IRequestParser>);
        Assert.Single(services, IsServiceImplementationFactory<IRequestParser, FakeRequestParser>);
    }

    [Fact]
    public void CanSetCache()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.SetCache<FakeImageCache>();
        Assert.Single(services, IsService<IImageCache>);
        Assert.Single(services, IsServiceImplementationType<IImageCache, FakeImageCache>);

        builder.SetCache(_ => new FakeImageCache());
        Assert.Single(services, IsService<IImageCache>);
        Assert.Single(services, IsServiceImplementationFactory<IImageCache, FakeImageCache>);
    }

    [Fact]
    public void CanSetCacheKey()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.SetCacheKey<FakeCacheKey>();
        Assert.Single(services, IsService<ICacheKey>);
        Assert.Single(services, IsServiceImplementationType<ICacheKey, FakeCacheKey>);

        builder.SetCacheKey(_ => new FakeCacheKey());
        Assert.Single(services, IsService<ICacheKey>);
        Assert.Single(services, IsServiceImplementationFactory<ICacheKey, FakeCacheKey>);
    }

    [Fact]
    public void CanSetCacheHash()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.SetCacheHash<FakeCacheHash>();
        Assert.Single(services, IsService<ICacheHash>);
        Assert.Single(services, IsServiceImplementationType<ICacheHash, FakeCacheHash>);

        builder.SetCacheHash(_ => new FakeCacheHash());
        Assert.Single(services, IsService<ICacheHash>);
        Assert.Single(services, IsServiceImplementationFactory<ICacheHash, FakeCacheHash>);
    }

    [Fact]
    public void CanAddRemoveImageProviders()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProvider<FakeImageProvider>();
        Assert.Single(services, IsService<IImageProvider, FakeImageProvider>);
        Assert.Single(services, IsServiceImplementationType<IImageProvider, FakeImageProvider>);

        builder.RemoveProvider<FakeImageProvider>();
        Assert.DoesNotContain(services, IsService<IImageProvider, FakeImageProvider>);
    }

    [Fact]
    public void CanInsertRemoveImageProviders()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.InsertProvider<FakeImageProvider>(0);
        Assert.Single(services, IsService<IImageProvider, FakeImageProvider>);
        Assert.Single(services, IsServiceImplementationType<IImageProvider, FakeImageProvider>);

        List<ServiceDescriptor> providers = GetCollection<IImageProvider>(services);
        Assert.Equal(2, providers.Count);
        Assert.True(IsService<IImageProvider, FakeImageProvider>(providers[0]));
        Assert.True(IsServiceImplementationType<IImageProvider, FakeImageProvider>(providers[0]));

        builder.RemoveProvider<FakeImageProvider>();
        Assert.DoesNotContain(services, IsService<IImageProvider, FakeImageProvider>);
    }

    [Fact]
    public void CanInsertIdempotentImageProviders()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.InsertProvider<FakeImageProvider>(0);
        builder.InsertProvider<FakeImageProvider>(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.InsertProvider<FakeImageProvider>(2));

        Assert.Single(services, IsService<IImageProvider, FakeImageProvider>);
        Assert.Single(services, IsServiceImplementationType<IImageProvider, FakeImageProvider>);

        List<ServiceDescriptor> providers = GetCollection<IImageProvider>(services);
        Assert.Equal(2, providers.Count);
        Assert.True(IsService<IImageProvider, FakeImageProvider>(providers[1]));
        Assert.True(IsServiceImplementationType<IImageProvider, FakeImageProvider>(providers[1]));
    }

    [Fact]
    public void CanAddRemoveFactoryImageProviders()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProvider(_ => new FakeImageProvider());
        Assert.Single(services, IsService<IImageProvider, FakeImageProvider>);
        Assert.Single(services, IsServiceImplementationFactory<IImageProvider, FakeImageProvider>);

        builder.RemoveProvider<FakeImageProvider>();
        Assert.DoesNotContain(services, IsService<IImageProvider, FakeImageProvider>);
    }

    [Fact]
    public void CanInsertIdempotentFactoryImageProviders()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.InsertProvider(0, _ => new FakeImageProvider());
        builder.InsertProvider(1, _ => new FakeImageProvider());

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.InsertProvider(2, _ => new FakeImageProvider()));

        Assert.Single(services, IsService<IImageProvider, FakeImageProvider>);
        Assert.Single(services, IsServiceImplementationFactory<IImageProvider, FakeImageProvider>);

        List<ServiceDescriptor> providers = GetCollection<IImageProvider>(services);
        Assert.Equal(2, providers.Count);
        Assert.True(IsService<IImageProvider, FakeImageProvider>(providers[1]));
        Assert.True(IsServiceImplementationFactory<IImageProvider, FakeImageProvider>(providers[1]));
    }

    [Fact]
    public void CanInsertRemoveFactoryImageProviders()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.InsertProvider(0, _ => new FakeImageProvider());
        Assert.Single(services, IsService<IImageProvider, FakeImageProvider>);
        Assert.Single(services, IsServiceImplementationFactory<IImageProvider, FakeImageProvider>);

        List<ServiceDescriptor> providers = GetCollection<IImageProvider>(services);
        Assert.Equal(2, providers.Count);
        Assert.True(IsService<IImageProvider, FakeImageProvider>(providers[0]));
        Assert.True(IsServiceImplementationFactory<IImageProvider, FakeImageProvider>(providers[0]));

        builder.RemoveProvider<FakeImageProvider>();
        Assert.DoesNotContain(services, IsService<IImageProvider, FakeImageProvider>);
    }

    [Fact]
    public void CanAddRemoveAllImageProviders()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProvider<FakeImageProvider>();
        Assert.Single(services, IsService<IImageProvider, FakeImageProvider>);
        Assert.Single(services, IsServiceImplementationType<IImageProvider, FakeImageProvider>);

        builder.ClearProviders();
        Assert.DoesNotContain(services, IsService<IImageProvider>);
    }

    [Fact]
    public void CanAddRemoveAllFactoryImageProviders()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProvider(_ => new FakeImageProvider());
        Assert.Single(services, IsService<IImageProvider, FakeImageProvider>);
        Assert.Single(services, IsServiceImplementationFactory<IImageProvider, FakeImageProvider>);

        builder.ClearProviders();
        Assert.DoesNotContain(services, IsService<IImageProvider>);
    }

    [Fact]
    public void CanAddRemoveImageProcessors()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProcessor<FakeWebProcessor>();
        Assert.Single(services, IsService<IImageWebProcessor, FakeWebProcessor>);
        Assert.Single(services, IsServiceImplementationType<IImageWebProcessor, FakeWebProcessor>);

        builder.RemoveProcessor<FakeWebProcessor>();
        Assert.DoesNotContain(services, IsService<IImageWebProcessor, FakeWebProcessor>);
    }

    [Fact]
    public void CanAddRemoveFactoryImageProcessors()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProcessor(_ => new FakeWebProcessor());
        Assert.Single(services, IsService<IImageWebProcessor, FakeWebProcessor>);
        Assert.Single(services, IsServiceImplementationFactory<IImageWebProcessor, FakeWebProcessor>);

        builder.RemoveProcessor<FakeWebProcessor>();
        Assert.DoesNotContain(services, IsService<IImageWebProcessor, FakeWebProcessor>);
    }

    [Fact]
    public void CanAddRemoveAllImageProcessors()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProcessor<FakeWebProcessor>();
        Assert.Single(services, IsService<IImageWebProcessor, FakeWebProcessor>);
        Assert.Single(services, IsServiceImplementationType<IImageWebProcessor, FakeWebProcessor>);

        builder.ClearProcessors();
        Assert.DoesNotContain(services, IsService<IImageWebProcessor>);
    }

    [Fact]
    public void CanAddRemoveAllFactoryImageProcessors()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddProcessor(_ => new FakeWebProcessor());
        Assert.Single(services, IsService<IImageWebProcessor, FakeWebProcessor>);
        Assert.Single(services, IsServiceImplementationFactory<IImageWebProcessor, FakeWebProcessor>);

        builder.ClearProcessors();
        Assert.DoesNotContain(services, IsService<IImageWebProcessor>);
    }

    [Fact]
    public void CanAddRemoveCommandConverters()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddConverter<FakeCommandConverter>();
        Assert.Single(services, IsService<ICommandConverter, FakeCommandConverter>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, FakeCommandConverter>);

        builder.RemoveConverter<FakeCommandConverter>();
        Assert.DoesNotContain(services, IsService<ICommandConverter, FakeCommandConverter>);
    }

    [Fact]
    public void CanAddRemoveFactoryCommandConverters()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddConverter(_ => new FakeCommandConverter());
        Assert.Single(services, IsService<ICommandConverter, FakeCommandConverter>);
        Assert.Single(services, IsServiceImplementationFactory<ICommandConverter, FakeCommandConverter>);

        builder.RemoveConverter<FakeCommandConverter>();
        Assert.DoesNotContain(services, IsService<ICommandConverter, FakeCommandConverter>);
    }

    [Fact]
    public void CanAddRemoveAllCommandConverters()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddConverter<FakeCommandConverter>();
        Assert.Single(services, IsService<ICommandConverter, FakeCommandConverter>);
        Assert.Single(services, IsServiceImplementationType<ICommandConverter, FakeCommandConverter>);

        builder.ClearConverters();
        Assert.DoesNotContain(services, IsService<ICommandConverter>);
    }

    [Fact]
    public void CanAddRemoveAllFactoryCommandConverters()
    {
        ServiceCollection services = new();
        IImageSharpBuilder builder = services.AddImageSharp();

        builder.AddConverter(_ => new FakeCommandConverter());
        Assert.Single(services, IsService<ICommandConverter, FakeCommandConverter>);
        Assert.Single(services, IsServiceImplementationFactory<ICommandConverter, FakeCommandConverter>);

        builder.ClearConverters();
        Assert.DoesNotContain(services, IsService<ICommandConverter>);
    }
}
