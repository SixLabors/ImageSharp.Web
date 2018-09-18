// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.Memory;

namespace SixLabors.ImageSharp.Web.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IImageSharpCoreBuilder"/> that allow configuration of services.
    /// </summary>
    public static class ImageSharpCoreBuilderExtensions
    {
        /// <summary>
        /// Sets the given <see cref="IRequestParser"/> adding it to the service collection.
        /// </summary>
        /// <typeparam name="TParser">The type of class implementing <see cref="IRequestParser"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetRequestParser<TParser>(this IImageSharpCoreBuilder builder)
            where TParser : class, IRequestParser
        {
            builder.Services.AddSingleton<IRequestParser, TParser>();
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IRequestParser"/> adding it to the service collection.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IRequestParser"/>.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetRequestParser(this IImageSharpCoreBuilder builder, Func<IServiceProvider, IRequestParser> implementationFactory)
        {
            builder.Services.AddSingleton(implementationFactory);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="MemoryAllocator"/> adding it to the service collection.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="MemoryAllocator"/>.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetMemoryAllocator(this IImageSharpCoreBuilder builder, Func<IServiceProvider, MemoryAllocator> implementationFactory)
        {
            builder.Services.AddSingleton(implementationFactory);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="MemoryAllocator"/> adding it to the service collection.
        /// </summary>
        /// <typeparam name="TMemoryAllocator">The type of class implementing <see cref="MemoryAllocator"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetMemoryAllocator<TMemoryAllocator>(this IImageSharpCoreBuilder builder)
            where TMemoryAllocator : MemoryAllocator
        {
            builder.Services.AddSingleton<MemoryAllocator, TMemoryAllocator>();
            return builder;
        }

        /// <summary>
        /// Sets the the memory allocator configured in <see cref="Configuration.MemoryAllocator"/> of <see cref="ImageSharpMiddlewareOptions.Configuration"/>.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetMemoryAllocatorFromMiddlewareOptions(this IImageSharpCoreBuilder builder)
        {
            MemoryAllocator AllocatorResolver(IServiceProvider s) => s.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>().Value.Configuration.MemoryAllocator;

            builder.SetMemoryAllocator(AllocatorResolver);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IImageCache"/> adding it to the service collection.
        /// </summary>
        /// <typeparam name="TCache">The type of class implementing <see cref="IImageCache"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetCache<TCache>(this IImageSharpCoreBuilder builder)
            where TCache : class, IImageCache
        {
            builder.Services.AddSingleton<IImageCache, TCache>();
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IImageCache"/> adding it to the service collection.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IImageCache"/>.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetCache(this IImageSharpCoreBuilder builder, Func<IServiceProvider, IImageCache> implementationFactory)
        {
            builder.Services.AddSingleton(implementationFactory);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="ICacheHash"/> adding it to the service collection.
        /// </summary>
        /// <typeparam name="TCacheHash">The type of class implementing <see cref="ICacheHash"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetCacheHash<TCacheHash>(this IImageSharpCoreBuilder builder)
            where TCacheHash : class, ICacheHash
        {
            builder.Services.AddSingleton<ICacheHash, TCacheHash>();
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="ICacheHash"/> adding it to the service collection.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="ICacheHash"/>.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetCacheHash(this IImageSharpCoreBuilder builder, Func<IServiceProvider, ICacheHash> implementationFactory)
        {
            builder.Services.AddSingleton(implementationFactory);
            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageProvider"/> to the resolver collection within the service collection.
        /// </summary>
        /// <typeparam name="TProvider">The type of class implementing <see cref="IImageProvider"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder AddProvider<TProvider>(this IImageSharpCoreBuilder builder)
            where TProvider : class, IImageProvider
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageProvider, TProvider>());
            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageProvider"/> to the resolver collection within the service collection.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IImageProvider"/>.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder AddProvider(this IImageSharpCoreBuilder builder, Func<IServiceProvider, IImageProvider> implementationFactory)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton(implementationFactory));
            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageWebProcessor"/> to the processor collection within the service collection.
        /// </summary>
        /// <typeparam name="TProcessor">The type of class implementing <see cref="IImageWebProcessor"/>to add.</typeparam>
        /// <param name="builder">The core builder.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder AddProcessor<TProcessor>(this IImageSharpCoreBuilder builder)
            where TProcessor : class, IImageWebProcessor
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageWebProcessor, TProcessor>());
            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageWebProcessor"/> to the processor collection within the service collection.
        /// </summary>
        /// <param name="builder">The core builder.</param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IImageProvider"/>.</param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder AddProcessor(this IImageSharpCoreBuilder builder, Func<IServiceProvider, IImageWebProcessor> implementationFactory)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton(implementationFactory));
            return builder;
        }
    }
}