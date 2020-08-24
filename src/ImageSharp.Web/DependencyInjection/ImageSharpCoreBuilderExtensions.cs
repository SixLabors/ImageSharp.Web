// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace SixLabors.ImageSharp.Web.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IImageSharpBuilder"/> that allow configuration of services.
    /// </summary>
    public static class ImageSharpCoreBuilderExtensions
    {
        /// <summary>
        /// Sets the given <see cref="IRequestParser"/> adding it to the service collection.
        /// </summary>
        /// <typeparam name="TParser">The type of class implementing <see cref="IRequestParser"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder SetRequestParser<TParser>(this IImageSharpBuilder builder)
            where TParser : class, IRequestParser
        {
            var descriptor = new ServiceDescriptor(typeof(IRequestParser), typeof(TParser), ServiceLifetime.Singleton);
            builder.Services.Replace(descriptor);
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
            var descriptor = new ServiceDescriptor(typeof(IRequestParser), implementationFactory, ServiceLifetime.Singleton);
            builder.Services.Replace(descriptor);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="MemoryAllocator"/> adding it to the service collection.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="MemoryAllocator"/>.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder SetMemoryAllocator(this IImageSharpBuilder builder, Func<IServiceProvider, MemoryAllocator> implementationFactory)
        {
            var descriptor = new ServiceDescriptor(typeof(MemoryAllocator), implementationFactory, ServiceLifetime.Singleton);
            builder.Services.Replace(descriptor);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="MemoryAllocator"/> adding it to the service collection.
        /// </summary>
        /// <typeparam name="TMemoryAllocator">The type of class implementing <see cref="MemoryAllocator"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder SetMemoryAllocator<TMemoryAllocator>(this IImageSharpBuilder builder)
            where TMemoryAllocator : MemoryAllocator
        {
            var descriptor = new ServiceDescriptor(typeof(MemoryAllocator), typeof(TMemoryAllocator), ServiceLifetime.Singleton);
            builder.Services.Replace(descriptor);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="RecyclableMemoryStreamManager"/> adding it to the service collection.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="RecyclableMemoryStreamManager"/>.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder SetMemoryStreamManager(this IImageSharpBuilder builder, Func<IServiceProvider, RecyclableMemoryStreamManager> implementationFactory)
        {
            var descriptor = new ServiceDescriptor(typeof(RecyclableMemoryStreamManager), implementationFactory, ServiceLifetime.Singleton);
            builder.Services.Replace(descriptor);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IImageCache"/> adding it to the service collection.
        /// </summary>
        /// <typeparam name="TCache">The type of class implementing <see cref="IImageCache"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder SetCache<TCache>(this IImageSharpBuilder builder)
            where TCache : class, IImageCache
        {
            var descriptor = new ServiceDescriptor(typeof(IImageCache), typeof(TCache), ServiceLifetime.Singleton);
            builder.Services.Replace(descriptor);
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
            var descriptor = new ServiceDescriptor(typeof(IImageCache), implementationFactory, ServiceLifetime.Singleton);
            builder.Services.Replace(descriptor);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="ICacheHash"/> adding it to the service collection.
        /// </summary>
        /// <typeparam name="TCacheHash">The type of class implementing <see cref="ICacheHash"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder SetCacheHash<TCacheHash>(this IImageSharpBuilder builder)
            where TCacheHash : class, ICacheHash
        {
            var descriptor = new ServiceDescriptor(typeof(ICacheHash), typeof(TCacheHash), ServiceLifetime.Singleton);
            builder.Services.Replace(descriptor);
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
            var descriptor = new ServiceDescriptor(typeof(ICacheHash), implementationFactory, ServiceLifetime.Singleton);
            builder.Services.Replace(descriptor);
            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageProvider"/> to the provider collection within the service collection.
        /// </summary>
        /// <typeparam name="TProvider">The type of class implementing <see cref="IImageProvider"/>to add.</typeparam>
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
        /// <typeparam name="TProvider">The type of class implementing <see cref="IImageProvider"/>to add.</typeparam>
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
        /// Removes the given <see cref="IImageProvider"/> from the provider collection within the service collection.
        /// </summary>
        /// <typeparam name="TProvider">The type of class implementing <see cref="IImageProvider"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder RemoveProvider<TProvider>(this IImageSharpBuilder builder)
            where TProvider : class, IImageProvider
        {
            ServiceDescriptor descriptor = builder.Services.FirstOrDefault(x =>
                x.ServiceType == typeof(IImageProvider)
                && x.Lifetime == ServiceLifetime.Singleton
                && (x.ImplementationType == typeof(TProvider)
                || (x.ImplementationFactory?.GetMethodInfo().ReturnType == typeof(TProvider))));

            if (descriptor != null)
            {
                builder.Services.Remove(descriptor);
            }

            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageWebProcessor"/> to the processor collection within the service collection.
        /// </summary>
        /// <typeparam name="TProcessor">The type of class implementing <see cref="IImageWebProcessor"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder AddProcessor<TProcessor>(this IImageSharpBuilder builder)
            where TProcessor : class, IImageWebProcessor
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageWebProcessor, TProcessor>());
            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageWebProcessor"/> to the processor collection within the service collection.
        /// </summary>
        /// <typeparam name="TProcessor">The type of class implementing <see cref="IImageWebProcessor"/>to add.</typeparam>
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
        /// <typeparam name="TProcessor">The type of class implementing <see cref="IImageWebProcessor"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder RemoveProcessor<TProcessor>(this IImageSharpBuilder builder)
            where TProcessor : class, IImageWebProcessor
        {
            ServiceDescriptor descriptor = builder.Services.FirstOrDefault(x =>
                x.ServiceType == typeof(IImageWebProcessor)
                && x.Lifetime == ServiceLifetime.Singleton
                && (x.ImplementationType == typeof(TProcessor)
                || (x.ImplementationFactory?.GetMethodInfo().ReturnType == typeof(TProcessor))));

            if (descriptor != null)
            {
                builder.Services.Remove(descriptor);
            }

            return builder;
        }

        /// <summary>
        /// Registers an action used to configure a particular type of options.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <param name="configuration">The configuration being bound.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        public static IImageSharpBuilder Configure<TOptions>(this IImageSharpBuilder builder, IConfiguration configuration)
             where TOptions : class
        {
            builder.Services.Configure<TOptions>(configuration);
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

        /// <summary>
        /// Sets the memory allocator configured in <see cref="Configuration.MemoryAllocator"/> of <see cref="ImageSharpMiddlewareOptions.Configuration"/>.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        internal static IImageSharpBuilder SetMemoryAllocatorFromMiddlewareOptions(this IImageSharpBuilder builder)
        {
            static MemoryAllocator AllocatorFactory(IServiceProvider s)
            {
                return s.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>().Value.Configuration.MemoryAllocator;
            }

            builder.SetMemoryAllocator(AllocatorFactory);
            return builder;
        }

        /// <summary>
        /// Sets the <see cref="RecyclableMemoryStream"/> configured in  <see cref="ImageSharpMiddlewareOptions"/>.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        internal static IImageSharpBuilder SetMemoryStreamManagerFromMiddlewareOptions(this IImageSharpBuilder builder)
        {
            static RecyclableMemoryStreamManager AllocatorFactory(IServiceProvider s)
            {
                return s.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>().Value.MemoryStreamManager;
            }

            builder.SetMemoryStreamManager(AllocatorFactory);
            return builder;
        }

        /// <summary>
        /// Sets the <see cref="FormatUtilities"/> configured by <see cref="ImageSharpMiddlewareOptions.Configuration"/>.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpBuilder"/>.</returns>
        internal static IImageSharpBuilder SetFormatUtilitesFromMiddlewareOptions(this IImageSharpBuilder builder)
        {
            static FormatUtilities FormatUtilitiesFactory(IServiceProvider s)
            {
                return new FormatUtilities(s.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>().Value.Configuration);
            }

            builder.Services.AddSingleton(FormatUtilitiesFactory);
            return builder;
        }
    }
}
