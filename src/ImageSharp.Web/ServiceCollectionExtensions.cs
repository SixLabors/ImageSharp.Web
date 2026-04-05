// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Synchronization;

namespace SixLabors.ImageSharp.Web;

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
        _ = builder.Services.Configure(setupAction);

        _ = builder.Services.AddSingleton<FormatUtilities>();

        _ = builder.Services.AddSingleton<AsyncKeyReaderWriterLock<string>>();

        _ = builder.SetRequestParser<QueryCollectionRequestParser>();

        _ = builder.Services.AddSingleton<RequestAuthorizationUtilities>();

        _ = builder.SetCache<PhysicalFileSystemCache>();

        _ = builder.SetCacheKey<UriRelativeLowerInvariantCacheKey>();

        _ = builder.SetCacheHash<SHA256CacheHash>();

        _ = builder.AddProvider<PhysicalFileSystemProvider>();

        _ = builder.AddProcessor<ResizeWebProcessor>()
               .AddProcessor<FormatWebProcessor>()
               .AddProcessor<BackgroundColorWebProcessor>()
               .AddProcessor<QualityWebProcessor>()
               .AddProcessor<AutoOrientWebProcessor>();

        _ = builder.AddConverter<IntegralNumberConverter<sbyte>>();
        _ = builder.AddConverter<IntegralNumberConverter<byte>>();
        _ = builder.AddConverter<IntegralNumberConverter<short>>();
        _ = builder.AddConverter<IntegralNumberConverter<ushort>>();
        _ = builder.AddConverter<IntegralNumberConverter<int>>();
        _ = builder.AddConverter<IntegralNumberConverter<uint>>();
        _ = builder.AddConverter<IntegralNumberConverter<long>>();
        _ = builder.AddConverter<IntegralNumberConverter<ulong>>();

        _ = builder.AddConverter<SimpleCommandConverter<decimal>>();
        _ = builder.AddConverter<SimpleCommandConverter<float>>();
        _ = builder.AddConverter<SimpleCommandConverter<double>>();
        _ = builder.AddConverter<SimpleCommandConverter<string>>();
        _ = builder.AddConverter<SimpleCommandConverter<bool>>();

        _ = builder.AddConverter<ArrayConverter<sbyte>>();
        _ = builder.AddConverter<ArrayConverter<byte>>();
        _ = builder.AddConverter<ArrayConverter<short>>();
        _ = builder.AddConverter<ArrayConverter<ushort>>();
        _ = builder.AddConverter<ArrayConverter<int>>();
        _ = builder.AddConverter<ArrayConverter<uint>>();
        _ = builder.AddConverter<ArrayConverter<long>>();
        _ = builder.AddConverter<ArrayConverter<ulong>>();
        _ = builder.AddConverter<ArrayConverter<decimal>>();
        _ = builder.AddConverter<ArrayConverter<float>>();
        _ = builder.AddConverter<ArrayConverter<double>>();
        _ = builder.AddConverter<ArrayConverter<string>>();
        _ = builder.AddConverter<ArrayConverter<bool>>();

        _ = builder.AddConverter<ListConverter<sbyte>>();
        _ = builder.AddConverter<ListConverter<byte>>();
        _ = builder.AddConverter<ListConverter<short>>();
        _ = builder.AddConverter<ListConverter<ushort>>();
        _ = builder.AddConverter<ListConverter<int>>();
        _ = builder.AddConverter<ListConverter<uint>>();
        _ = builder.AddConverter<ListConverter<long>>();
        _ = builder.AddConverter<ListConverter<ulong>>();
        _ = builder.AddConverter<ListConverter<decimal>>();
        _ = builder.AddConverter<ListConverter<float>>();
        _ = builder.AddConverter<ListConverter<double>>();
        _ = builder.AddConverter<ListConverter<string>>();
        _ = builder.AddConverter<ListConverter<bool>>();

        _ = builder.AddConverter<ColorConverter>();
        _ = builder.AddConverter<EnumConverter>();

        _ = builder.Services.AddSingleton<CommandParser>();
    }
}
