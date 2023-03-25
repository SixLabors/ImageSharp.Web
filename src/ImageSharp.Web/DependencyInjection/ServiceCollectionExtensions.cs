// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Synchronization;

namespace SixLabors.ImageSharp.Web.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to simplify middleware service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ImageSharp services to the specified <see cref="IServiceCollection" /> with the default options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>An <see cref="IImageSharpBuilder"/> that can be used to further configure the ImageSharp services.</returns>
    public static IImageSharpBuilder AddImageSharp(this IServiceCollection services)
        => AddImageSharp(services, _ => { });

    /// <summary>
    /// Adds ImageSharp services to the specified <see cref="IServiceCollection" /> with the given options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="setupAction">An <see cref="Action{ImageSharpMiddlewareOptions}"/> to configure the provided <see cref="ImageSharpMiddlewareOptions"/>.</param>
    /// <returns>An <see cref="IImageSharpBuilder"/> that can be used to further configure the ImageSharp services.</returns>
    public static IImageSharpBuilder AddImageSharp(
        this IServiceCollection services,
        Action<ImageSharpMiddlewareOptions> setupAction)
    {
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(setupAction, nameof(setupAction));

        IImageSharpBuilder builder = new ImageSharpBuilder(services);

        AddDefaultServices(builder, setupAction);

        return builder;
    }

    private static void AddDefaultServices(
        IImageSharpBuilder builder,
        Action<ImageSharpMiddlewareOptions> setupAction)
    {
        builder.Services.Configure(setupAction);

        builder.Services.AddSingleton<FormatUtilities>();

        builder.Services.AddSingleton<AsyncKeyReaderWriterLock<string>>();

        builder.SetRequestParser<QueryCollectionRequestParser>();

        builder.Services.AddSingleton<RequestAuthorizationUtilities>();

        builder.SetCache<PhysicalFileSystemCache>();

        builder.SetCacheKey<UriRelativeLowerInvariantCacheKey>();

        builder.SetCacheHash<SHA256CacheHash>();

        builder.AddProvider<PhysicalFileSystemProvider>();

        builder.AddProcessor<ResizeWebProcessor>()
               .AddProcessor<FormatWebProcessor>()
               .AddProcessor<BackgroundColorWebProcessor>()
               .AddProcessor<QualityWebProcessor>()
               .AddProcessor<AutoOrientWebProcessor>();

        builder.AddConverter<IntegralNumberConverter<sbyte>>();
        builder.AddConverter<IntegralNumberConverter<byte>>();
        builder.AddConverter<IntegralNumberConverter<short>>();
        builder.AddConverter<IntegralNumberConverter<ushort>>();
        builder.AddConverter<IntegralNumberConverter<int>>();
        builder.AddConverter<IntegralNumberConverter<uint>>();
        builder.AddConverter<IntegralNumberConverter<long>>();
        builder.AddConverter<IntegralNumberConverter<ulong>>();

        builder.AddConverter<SimpleCommandConverter<decimal>>();
        builder.AddConverter<SimpleCommandConverter<float>>();
        builder.AddConverter<SimpleCommandConverter<double>>();
        builder.AddConverter<SimpleCommandConverter<string>>();
        builder.AddConverter<SimpleCommandConverter<bool>>();

        builder.AddConverter<ArrayConverter<sbyte>>();
        builder.AddConverter<ArrayConverter<byte>>();
        builder.AddConverter<ArrayConverter<short>>();
        builder.AddConverter<ArrayConverter<ushort>>();
        builder.AddConverter<ArrayConverter<int>>();
        builder.AddConverter<ArrayConverter<uint>>();
        builder.AddConverter<ArrayConverter<long>>();
        builder.AddConverter<ArrayConverter<ulong>>();
        builder.AddConverter<ArrayConverter<decimal>>();
        builder.AddConverter<ArrayConverter<float>>();
        builder.AddConverter<ArrayConverter<double>>();
        builder.AddConverter<ArrayConverter<string>>();
        builder.AddConverter<ArrayConverter<bool>>();

        builder.AddConverter<ListConverter<sbyte>>();
        builder.AddConverter<ListConverter<byte>>();
        builder.AddConverter<ListConverter<short>>();
        builder.AddConverter<ListConverter<ushort>>();
        builder.AddConverter<ListConverter<int>>();
        builder.AddConverter<ListConverter<uint>>();
        builder.AddConverter<ListConverter<long>>();
        builder.AddConverter<ListConverter<ulong>>();
        builder.AddConverter<ListConverter<decimal>>();
        builder.AddConverter<ListConverter<float>>();
        builder.AddConverter<ListConverter<double>>();
        builder.AddConverter<ListConverter<string>>();
        builder.AddConverter<ListConverter<bool>>();

        builder.AddConverter<ColorConverter>();
        builder.AddConverter<EnumConverter>();

        builder.Services.AddSingleton<CommandParser>();
    }
}
