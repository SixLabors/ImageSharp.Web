// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Memory;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Resolvers;

namespace SixLabors.ImageSharp.Web.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IImageSharpCoreBuilder"/> that allow configuration of services.
    /// </summary>
    public static class ImageSharpCoreBuilderExtensions
    {
        /// <summary>
        /// Sets the given <see cref="IRequestParser"/> adding it to the service collection
        /// </summary>
        /// <typeparam name="TParser">The type of class implementing <see cref="IRequestParser"/>to add.</typeparam>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetRequestParser<TParser>(this IImageSharpCoreBuilder builder)
            where TParser : class, IRequestParser
        {
            builder.Services.AddSingleton<IRequestParser, TParser>();
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IRequestParser"/> adding it to the service collection
        /// </summary>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IRequestParser"/>"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetRequestParser(this IImageSharpCoreBuilder builder, Func<IServiceProvider, IRequestParser> implementationFactory)
        {
            builder.Services.AddSingleton(implementationFactory);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IBufferManager"/> adding it to the service collection
        /// </summary>
        /// <typeparam name="TBufferManager">The type of class implementing <see cref="IBufferManager"/>to add.</typeparam>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetBufferManager<TBufferManager>(this IImageSharpCoreBuilder builder)
            where TBufferManager : class, IBufferManager
        {
            builder.Services.AddSingleton<IBufferManager, TBufferManager>();
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IBufferManager"/> adding it to the service collection
        /// </summary>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IBufferManager"/>"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetBufferManager(this IImageSharpCoreBuilder builder, Func<IServiceProvider, IBufferManager> implementationFactory)
        {
            builder.Services.AddSingleton(implementationFactory);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IImageCache"/> adding it to the service collection
        /// </summary>
        /// <typeparam name="TCache">The type of class implementing <see cref="IImageCache"/>to add.</typeparam>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetCache<TCache>(this IImageSharpCoreBuilder builder)
            where TCache : class, IImageCache
        {
            builder.Services.AddSingleton<IImageCache, TCache>();
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IImageCache"/> adding it to the service collection
        /// </summary>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IImageCache"/>"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetCache(this IImageSharpCoreBuilder builder, Func<IServiceProvider, IImageCache> implementationFactory)
        {
            builder.Services.AddSingleton(implementationFactory);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="ICacheHash"/> adding it to the service collection
        /// </summary>
        /// <typeparam name="TCacheHash">The type of class implementing <see cref="ICacheHash"/>to add.</typeparam>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetCacheHash<TCacheHash>(this IImageSharpCoreBuilder builder)
            where TCacheHash : class, ICacheHash
        {
            builder.Services.AddSingleton<ICacheHash, TCacheHash>();
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="ICacheHash"/> adding it to the service collection
        /// </summary>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="ICacheHash"/>"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetCacheHash(this IImageSharpCoreBuilder builder, Func<IServiceProvider, ICacheHash> implementationFactory)
        {
            builder.Services.AddSingleton(implementationFactory);
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IAsyncKeyLock"/> adding it to the service collection
        /// </summary>
        /// <typeparam name="TLock">The type of class implementing <see cref="IAsyncKeyLock"/>to add.</typeparam>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetAsyncKeyLock<TLock>(this IImageSharpCoreBuilder builder)
            where TLock : class, IAsyncKeyLock
        {
            builder.Services.AddSingleton<IAsyncKeyLock, TLock>();
            return builder;
        }

        /// <summary>
        /// Sets the given <see cref="IAsyncKeyLock"/> adding it to the service collection
        /// </summary>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IImageCache"/>"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder SetAsyncKeyLock(this IImageSharpCoreBuilder builder, Func<IServiceProvider, IAsyncKeyLock> implementationFactory)
        {
            builder.Services.AddSingleton(implementationFactory);
            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageResolver"/> to the resolver collection within the service collection
        /// </summary>
        /// <typeparam name="TResolver">The type of class implementing <see cref="IImageResolver"/>to add.</typeparam>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder AddResolver<TResolver>(this IImageSharpCoreBuilder builder)
            where TResolver : class, IImageResolver
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageResolver, TResolver>());
            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageResolver"/> to the resolver collection within the service collection
        /// </summary>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IImageResolver"/>"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder AddResolver(this IImageSharpCoreBuilder builder, Func<IServiceProvider, IImageResolver> implementationFactory)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton(implementationFactory));
            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageWebProcessor"/> to the processor collection within the service collection
        /// </summary>
        /// <typeparam name="TProcessor">The type of class implementing <see cref="IImageWebProcessor"/>to add.</typeparam>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder AddProcessor<TProcessor>(this IImageSharpCoreBuilder builder)
            where TProcessor : class, IImageWebProcessor
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IImageWebProcessor, TProcessor>());
            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IImageWebProcessor"/> to the processor collection within the service collection
        /// </summary>
        /// <param name="builder">The <see cref="IImageSharpCoreBuilder"/></param>
        /// <param name="implementationFactory">The factory method for returning a <see cref="IImageResolver"/>"/></param>
        /// <returns>The <see cref="IImageSharpCoreBuilder"/>.</returns>
        public static IImageSharpCoreBuilder AddProcessor(this IImageSharpCoreBuilder builder, Func<IServiceProvider, IImageWebProcessor> implementationFactory)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton(implementationFactory));
            return builder;
        }
    }
}