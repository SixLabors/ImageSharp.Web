// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace SixLabors.ImageSharp.Web.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IImageSharpBuilder"/> that allow configuration of services.
    /// </summary>
    public static class ImageSharpBuilderExtensions
    {
        /// <summary>
        /// Sets the given <see cref="IRequestParser"/> adding it to the service collection.
        /// </summary>
        /// <typeparam name="T">The type of class implementing <see cref="IRequestParser"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder SetRequestParser<T>(this IImageSharpBuilder builder)
            where T : class, IRequestParser
        {
            builder.Services.Replace(ServiceDescriptor.Singleton<IRequestParser, T>());

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
        /// <typeparam name="T">The type of class implementing <see cref="IImageCache"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder SetCache<T>(this IImageSharpBuilder builder)
            where T : class, IImageCache
        {
            builder.Services.Replace(ServiceDescriptor.Singleton<IImageCache, T>());

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
        /// <typeparam name="T">The type of class implementing <see cref="ICacheKey"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder SetCacheKey<T>(this IImageSharpBuilder builder)
            where T : class, ICacheKey
        {
            builder.Services.Replace(ServiceDescriptor.Singleton<ICacheKey, T>());

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
        /// <typeparam name="T">The type of class implementing <see cref="ICacheHash"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder SetCacheHash<T>(this IImageSharpBuilder builder)
            where T : class, ICacheHash
        {
            builder.Services.Replace(ServiceDescriptor.Singleton<ICacheHash, T>());

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
        /// <typeparam name="T">The type of class implementing <see cref="IImageProvider"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder AddProvider<T>(this IImageSharpBuilder builder)
            where T : class, IImageProvider
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageProvider, T>());

            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageProvider"/> to the provider collection within the service collection.
        /// </summary>
        /// <typeparam name="T">The type of class implementing <see cref="IImageProvider"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IImageProvider"/>.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder AddProvider<T>(this IImageSharpBuilder builder, Func<IServiceProvider, T> implementationFactory)
            where T : class, IImageProvider
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageProvider>(implementationFactory));

            return builder;
        }

        /// <summary>
        /// Inserts the given <see cref="IImageProvider"/> at the give index into to the provider collection within the service collection.
        /// </summary>
        /// <typeparam name="T">The type of class implementing <see cref="IImageProvider"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <param name="index">The zero-based index at which the provider should be inserted.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder InsertProvider<T>(this IImageSharpBuilder builder, int index)
            where T : class, IImageProvider
        {
            var descriptors = builder.Services.Where(x => x.ServiceType == typeof(IImageProvider)).ToList();
            descriptors.RemoveAll(x => x.GetImplementationType() == typeof(T));
            descriptors.Insert(index, ServiceDescriptor.Singleton<IImageProvider, T>());

            builder.ClearProviders();
            builder.Services.TryAddEnumerable(descriptors);

            return builder;
        }

        /// <summary>
        /// Inserts the given <see cref="IImageProvider"/>  at the give index into the provider collection within the service collection.
        /// </summary>
        /// <typeparam name="T">The type of class implementing <see cref="IImageProvider"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <param name="index">The zero-based index at which the provider should be inserted.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IImageProvider"/>.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder InsertProvider<T>(this IImageSharpBuilder builder, int index, Func<IServiceProvider, T> implementationFactory)
            where T : class, IImageProvider
        {
            var descriptors = builder.Services.Where(x => x.ServiceType == typeof(IImageProvider)).ToList();
            descriptors.RemoveAll(x => x.GetImplementationType() == typeof(T));
            descriptors.Insert(index, ServiceDescriptor.Singleton<IImageProvider>(implementationFactory));

            builder.ClearProviders();
            builder.Services.TryAddEnumerable(descriptors);

            return builder;
        }

        /// <summary>
        /// Removes the given <see cref="IImageProvider"/> from the provider collection within the service collection.
        /// </summary>
        /// <typeparam name="T">The type of class implementing <see cref="IImageProvider"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder RemoveProvider<T>(this IImageSharpBuilder builder)
            where T : class, IImageProvider
        {
            ServiceDescriptor descriptor = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IImageProvider) && x.GetImplementationType() == typeof(T));
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
        /// <typeparam name="T">The type of class implementing <see cref="IImageWebProcessor"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IImageProvider"/>.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder AddProcessor<T>(this IImageSharpBuilder builder, Func<IServiceProvider, T> implementationFactory)
            where T : class, IImageWebProcessor
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageWebProcessor>(implementationFactory));

            return builder;
        }

        /// <summary>
        /// Removes the given <see cref="IImageWebProcessor"/> from the processor collection within the service collection.
        /// </summary>
        /// <typeparam name="T">The type of class implementing <see cref="IImageWebProcessor"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder RemoveProcessor<T>(this IImageSharpBuilder builder)
            where T : class, IImageWebProcessor
        {
            ServiceDescriptor descriptor = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IImageWebProcessor) && x.GetImplementationType() == typeof(T));
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
        /// <typeparam name="T">The type of class implementing <see cref="ICommandConverter"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder AddConverter<T>(this IImageSharpBuilder builder)
            where T : class, ICommandConverter
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ICommandConverter, T>());

            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="ICommandConverter"/> to the converter collection within the service collection.
        /// </summary>
        /// <typeparam name="T">The type of class implementing <see cref="ICommandConverter"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="ICommandConverter"/>.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder AddConverter<T>(this IImageSharpBuilder builder, Func<IServiceProvider, T> implementationFactory)
            where T : class, ICommandConverter
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ICommandConverter>(implementationFactory));

            return builder;
        }

        /// <summary>
        /// Removes the given <see cref="ICommandConverter"/> from the converter collection within the service collection.
        /// </summary>
        /// <typeparam name="T">The type of class implementing <see cref="ICommandConverter"/> to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder RemoveConverter<T>(this IImageSharpBuilder builder)
            where T : class, ICommandConverter
        {
            ServiceDescriptor descriptor = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ICommandConverter) && x.GetImplementationType() == typeof(T));
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
        /// <typeparam name="T">The options type to be configured.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <param name="config">The configuration being bound.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder Configure<T>(this IImageSharpBuilder builder, IConfiguration config)
             where T : class
        {
            builder.Services.Configure<T>(config);

            return builder;
        }

        /// <summary>
        /// Registers an action used to configure a particular type of options.
        /// </summary>
        /// <typeparam name="T">The options type to be configured.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder Configure<T>(this IImageSharpBuilder builder, Action<T> configureOptions)
             where T : class
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }

        /// <summary>
        /// Registers an action used to configure a particular type of options. Note: These are run after all <see cref="Configure{T}(IImageSharpBuilder, Action{T})"/>.
        /// </summary>
        /// <typeparam name="T">The options type to be configured.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder PostConfigure<T>(this IImageSharpBuilder builder, Action<T> configureOptions)
             where T : class
        {
            builder.Services.PostConfigure(configureOptions);

            return builder;
        }

        private static Type GetImplementationType(this ServiceDescriptor descriptor)
            => descriptor.ImplementationType
            ?? descriptor.ImplementationInstance?.GetType()
            ?? descriptor.ImplementationFactory?.GetType().GenericTypeArguments[1];
    }
}
