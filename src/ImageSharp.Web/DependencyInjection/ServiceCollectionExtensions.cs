// Copyright (c) Six Labors.
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

namespace SixLabors.ImageSharp.Web.DependencyInjection
{
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

            services.TryAddTransient<IConfigureOptions<ImageSharpMiddlewareOptions>, ImageSharpConfiguration>();

            IImageSharpBuilder builder = new ImageSharpBuilder(services);

            AddDefaultServices(builder, setupAction);

            return builder;
        }

        private static void AddDefaultServices(
            IImageSharpBuilder builder,
            Action<ImageSharpMiddlewareOptions> setupAction)
        {
            builder.Services.Configure(setupAction);

            builder.SetFormatUtilitesFromMiddlewareOptions();

            builder.SetRequestParser<QueryCollectionRequestParser>();

            builder.SetCache<PhysicalFileSystemCache>();

            builder.SetCacheHash<CacheHash>();

            builder.AddProvider<PhysicalFileSystemProvider>();

            builder.AddProcessor<ResizeWebProcessor>()
                   .AddProcessor<FormatWebProcessor>()
                   .AddProcessor<BackgroundColorWebProcessor>();
        }
    }
}
