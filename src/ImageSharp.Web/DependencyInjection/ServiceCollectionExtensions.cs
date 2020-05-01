// Copyright (c) Six Labors and contributors.
// Licensed under the GNU Affero General Public License, Version 3.

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
        {
            IImageSharpBuilder builder = AddImageSharpCore(services);

            AddDefaultServices(builder);

            return new ImageSharpBuilder(builder.Services);
        }

        /// <summary>
        /// Adds ImageSharp services to the specified <see cref="IServiceCollection" /> with the given options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{ImageSharpMiddlewareOptions}"/> to configure the provided <see cref="ImageSharpMiddlewareOptions"/>.</param>
        /// <returns>An <see cref="IImageSharpBuilder"/> that can be used to further configure the ImageSharp services.</returns>
        public static IImageSharpBuilder AddImageSharp(this IServiceCollection services, Action<ImageSharpMiddlewareOptions> setupAction)
        {
            IImageSharpBuilder builder = AddImageSharpCore(services, setupAction);

            AddDefaultServices(builder);

            return new ImageSharpBuilder(builder.Services);
        }

        /// <summary>
        /// Provides the means to add essential ImageSharp services to the specified <see cref="IServiceCollection" /> with the given options.
        /// All additional services are required to be configured.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{ImageSharpMiddlewareOptions}"/> to configure the provided <see cref="ImageSharpMiddlewareOptions"/>.</param>
        /// <returns>An <see cref="IImageSharpBuilder"/> that can be used to further configure the ImageSharp services.</returns>
        public static IImageSharpBuilder AddImageSharpCore(this IServiceCollection services, Action<ImageSharpMiddlewareOptions> setupAction)
        {
            Guard.NotNull(services, nameof(services));

            services.TryAddTransient<IConfigureOptions<ImageSharpMiddlewareOptions>, ImageSharpConfiguration>();

            IImageSharpBuilder builder = new ImageSharpBuilder(services);

            builder.Services.Configure(setupAction);

            builder.SetMemoryAllocatorFromMiddlewareOptions();
            builder.SetFormatUtilitesFromMiddlewareOptions();

            return builder;
        }

        /// <summary>
        /// Provides the means to add essential ImageSharp services to the specified <see cref="IServiceCollection" /> with the default options.
        /// All additional services are required to be configured.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>An <see cref="IImageSharpBuilder"/> that can be used to further configure the ImageSharp services.</returns>
        public static IImageSharpBuilder AddImageSharpCore(this IServiceCollection services)
            => AddImageSharpCore(services, _ => { });

        /// <summary>
        /// Adds the default service to the service collection.
        /// </summary>
        /// <param name="builder">The <see cref="IImageSharpBuilder"/> that can be used to further configure the ImageSharp services.</param>
        private static void AddDefaultServices(IImageSharpBuilder builder)
        {
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