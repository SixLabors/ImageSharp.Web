// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace SixLabors.ImageSharp.Web.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IImageSharpBuilder"/> that allow configuration of services.
/// </summary>
public static class ImageSharpBuilderExtensions
{
    /// <summary>
    /// Sets the given <see cref="IRequestParser"/> adding it to the service collection.
    /// </summary>
    /// <typeparam name="TParser">The type of class implementing <see cref="IRequestParser"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder SetRequestParser<TParser>(this IImageSharpBuilder builder)
        where TParser : class, IRequestParser
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IRequestParser, TParser>());

        return builder;
    }

    /// <summary>
    /// Sets the given <see cref="IRequestParser"/> adding it to the service collection.
    /// </summary>
    /// <param name="builder">The core builder.</param>
    /// <param name="implementationFactory">The factory method for returning a <see cref="IRequestParser"/>.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder SetRequestParser(this IImageSharpBuilder builder, Func<IServiceProvider, IRequestParser> implementationFactory)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton(implementationFactory));

        return builder;
    }

    /// <summary>
    /// Sets the given <see cref="IImageCache"/> adding it to the service collection.
    /// </summary>
    /// <typeparam name="TCache">The type of class implementing <see cref="IImageCache"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder SetCache<TCache>(this IImageSharpBuilder builder)
        where TCache : class, IImageCache
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IImageCache, TCache>());

        return builder;
    }

    /// <summary>
    /// Sets the given <see cref="IImageCache"/> adding it to the service collection.
    /// </summary>
    /// <param name="builder">The core builder.</param>
    /// <param name="implementationFactory">The factory method for returning a <see cref="IImageCache"/>.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder SetCache(this IImageSharpBuilder builder, Func<IServiceProvider, IImageCache> implementationFactory)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton(implementationFactory));

        return builder;
    }

    /// <summary>
    /// Sets the given <see cref="ICacheKey"/> adding it to the service collection.
    /// </summary>
    /// <typeparam name="TCacheKey">The type of class implementing <see cref="ICacheKey"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder SetCacheKey<TCacheKey>(this IImageSharpBuilder builder)
        where TCacheKey : class, ICacheKey
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<ICacheKey, TCacheKey>());

        return builder;
    }

    /// <summary>
    /// Sets the given <see cref="ICacheKey"/> adding it to the service collection.
    /// </summary>
    /// <param name="builder">The core builder.</param>
    /// <param name="implementationFactory">The factory method for returning a <see cref="ICacheKey"/>.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder SetCacheKey(this IImageSharpBuilder builder, Func<IServiceProvider, ICacheKey> implementationFactory)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton(implementationFactory));

        return builder;
    }

    /// <summary>
    /// Sets the given <see cref="ICacheHash"/> adding it to the service collection.
    /// </summary>
    /// <typeparam name="TCacheHash">The type of class implementing <see cref="ICacheHash"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder SetCacheHash<TCacheHash>(this IImageSharpBuilder builder)
        where TCacheHash : class, ICacheHash
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<ICacheHash, TCacheHash>());

        return builder;
    }

    /// <summary>
    /// Sets the given <see cref="ICacheHash"/> adding it to the service collection.
    /// </summary>
    /// <param name="builder">The core builder.</param>
    /// <param name="implementationFactory">The factory method for returning a <see cref="ICacheHash"/>.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder SetCacheHash(this IImageSharpBuilder builder, Func<IServiceProvider, ICacheHash> implementationFactory)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton(implementationFactory));

        return builder;
    }

    /// <summary>
    /// Adds the given <see cref="IImageProvider"/> to the provider collection within the service collection.
    /// </summary>
    /// <typeparam name="TProvider">The type of class implementing <see cref="IImageProvider"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder AddProvider<TProvider>(this IImageSharpBuilder builder)
        where TProvider : class, IImageProvider
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageProvider, TProvider>());

        return builder;
    }

    /// <summary>
    /// Adds the given <see cref="IImageProvider"/> to the provider collection within the service collection.
    /// </summary>
    /// <typeparam name="TProvider">The type of class implementing <see cref="IImageProvider"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <param name="implementationFactory">The factory method for returning a <see cref="IImageProvider"/>.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder AddProvider<TProvider>(this IImageSharpBuilder builder, Func<IServiceProvider, TProvider> implementationFactory)
        where TProvider : class, IImageProvider
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageProvider>(implementationFactory));

        return builder;
    }

    /// <summary>
    /// Inserts the given <see cref="IImageProvider"/> at the give index into to the provider collection within the service collection.
    /// </summary>
    /// <typeparam name="TProvider">The type of class implementing <see cref="IImageProvider"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <param name="index">The zero-based index at which the provider should be inserted.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder InsertProvider<TProvider>(this IImageSharpBuilder builder, int index)
        where TProvider : class, IImageProvider
    {
        var descriptors = builder.Services.Where(x => x.ServiceType == typeof(IImageProvider)).ToList();
        descriptors.RemoveAll(x => x.GetImplementationType() == typeof(TProvider));
        descriptors.Insert(index, ServiceDescriptor.Singleton<IImageProvider, TProvider>());

        builder.ClearProviders();
        builder.Services.TryAddEnumerable(descriptors);

        return builder;
    }

    /// <summary>
    /// Inserts the given <see cref="IImageProvider"/>  at the give index into the provider collection within the service collection.
    /// </summary>
    /// <typeparam name="TProvider">The type of class implementing <see cref="IImageProvider"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <param name="index">The zero-based index at which the provider should be inserted.</param>
    /// <param name="implementationFactory">The factory method for returning a <see cref="IImageProvider"/>.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder InsertProvider<TProvider>(this IImageSharpBuilder builder, int index, Func<IServiceProvider, TProvider> implementationFactory)
        where TProvider : class, IImageProvider
    {
        var descriptors = builder.Services.Where(x => x.ServiceType == typeof(IImageProvider)).ToList();
        descriptors.RemoveAll(x => x.GetImplementationType() == typeof(TProvider));
        descriptors.Insert(index, ServiceDescriptor.Singleton<IImageProvider>(implementationFactory));

        builder.ClearProviders();
        builder.Services.TryAddEnumerable(descriptors);

        return builder;
    }

    /// <summary>
    /// Removes the given <see cref="IImageProvider"/> from the provider collection within the service collection.
    /// </summary>
    /// <typeparam name="TProvider">The type of class implementing <see cref="IImageProvider"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder RemoveProvider<TProvider>(this IImageSharpBuilder builder)
        where TProvider : class, IImageProvider
    {
        ServiceDescriptor? descriptor = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IImageProvider) && x.GetImplementationType() == typeof(TProvider));
        if (descriptor != null)
        {
            builder.Services.Remove(descriptor);
        }

        return builder;
    }

    /// <summary>
    /// Removes all <see cref="IImageProvider"/> instances from the provider collection within the service collection.
    /// </summary>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder ClearProviders(this IImageSharpBuilder builder)
    {
        builder.Services.RemoveAll<IImageProvider>();

        return builder;
    }

    /// <summary>
    /// Adds the given <see cref="IImageWebProcessor"/> to the processor collection within the service collection.
    /// </summary>
    /// <typeparam name="T">The type of class implementing <see cref="IImageWebProcessor"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder AddProcessor<T>(this IImageSharpBuilder builder)
        where T : class, IImageWebProcessor
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageWebProcessor, T>());

        return builder;
    }

    /// <summary>
    /// Adds the given <see cref="IImageWebProcessor"/> to the processor collection within the service collection.
    /// </summary>
    /// <typeparam name="TProcessor">The type of class implementing <see cref="IImageWebProcessor"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <param name="implementationFactory">The factory method for returning a <see cref="IImageProvider"/>.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder AddProcessor<TProcessor>(this IImageSharpBuilder builder, Func<IServiceProvider, TProcessor> implementationFactory)
        where TProcessor : class, IImageWebProcessor
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageWebProcessor>(implementationFactory));

        return builder;
    }

    /// <summary>
    /// Removes the given <see cref="IImageWebProcessor"/> from the processor collection within the service collection.
    /// </summary>
    /// <typeparam name="TProcessor">The type of class implementing <see cref="IImageWebProcessor"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder RemoveProcessor<TProcessor>(this IImageSharpBuilder builder)
        where TProcessor : class, IImageWebProcessor
    {
        ServiceDescriptor? descriptor = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IImageWebProcessor) && x.GetImplementationType() == typeof(TProcessor));
        if (descriptor != null)
        {
            builder.Services.Remove(descriptor);
        }

        return builder;
    }

    /// <summary>
    /// Removes all <see cref="IImageWebProcessor"/> instances from the processor collection within the service collection.
    /// </summary>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder ClearProcessors(this IImageSharpBuilder builder)
    {
        builder.Services.RemoveAll<IImageWebProcessor>();

        return builder;
    }

    /// <summary>
    /// Adds the given <see cref="ICommandConverter"/> to the converter collection within the service collection.
    /// </summary>
    /// <typeparam name="TConverter">The type of class implementing <see cref="ICommandConverter"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder AddConverter<TConverter>(this IImageSharpBuilder builder)
        where TConverter : class, ICommandConverter
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ICommandConverter, TConverter>());

        return builder;
    }

    /// <summary>
    /// Adds the given <see cref="ICommandConverter"/> to the converter collection within the service collection.
    /// </summary>
    /// <typeparam name="TConverter">The type of class implementing <see cref="ICommandConverter"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <param name="implementationFactory">The factory method for returning a <see cref="ICommandConverter"/>.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder AddConverter<TConverter>(this IImageSharpBuilder builder, Func<IServiceProvider, TConverter> implementationFactory)
        where TConverter : class, ICommandConverter
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ICommandConverter>(implementationFactory));

        return builder;
    }

    /// <summary>
    /// Removes the given <see cref="ICommandConverter"/> from the converter collection within the service collection.
    /// </summary>
    /// <typeparam name="TConverter">The type of class implementing <see cref="ICommandConverter"/> to add.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder RemoveConverter<TConverter>(this IImageSharpBuilder builder)
        where TConverter : class, ICommandConverter
    {
        ServiceDescriptor? descriptor = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ICommandConverter) && x.GetImplementationType() == typeof(TConverter));
        if (descriptor != null)
        {
            builder.Services.Remove(descriptor);
        }

        return builder;
    }

    /// <summary>
    /// Removes all <see cref="ICommandConverter"/> instances from the converter collection within the service collection.
    /// </summary>
    /// <param name="builder">The core builder.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder ClearConverters(this IImageSharpBuilder builder)
    {
        builder.Services.RemoveAll<ICommandConverter>();

        return builder;
    }

    /// <summary>
    /// Registers an action used to configure a particular type of options.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <param name="config">The configuration being bound.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder Configure<TOptions>(this IImageSharpBuilder builder, IConfiguration config)
         where TOptions : class
    {
        builder.Services.Configure<TOptions>(config);

        return builder;
    }

    /// <summary>
    /// Registers an action used to configure a particular type of options.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="builder">The core builder.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
    public static IImageSharpBuilder Configure<TOptions>(this IImageSharpBuilder builder, Action<TOptions> configureOptions)
         where TOptions : class
    {
        builder.Services.Configure(configureOptions);

        return builder;
    }

    private static Type? GetImplementationType(this ServiceDescriptor descriptor)
        => descriptor.ImplementationType
        ?? descriptor.ImplementationInstance?.GetType()
        ?? descriptor.ImplementationFactory?.GetType().GenericTypeArguments[1];
}
