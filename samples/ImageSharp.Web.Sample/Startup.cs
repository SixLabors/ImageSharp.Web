using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.Memory;

namespace SixLabors.ImageSharp.Web.Sample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddImageSharpCore()
                .SetRequestParser<QueryCollectionRequestParser>()
                .SetMemoryAllocatorFromMiddlewareOptions()
                .SetCache(provider => new PhysicalFileSystemCache(
                    provider.GetRequiredService<IHostingEnvironment>(),
                    provider.GetRequiredService<MemoryAllocator>(),
                    provider.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>())
                {
                    Settings =
                    {
                        [PhysicalFileSystemCache.Folder] = PhysicalFileSystemCache.DefaultCacheFolder,
                        [PhysicalFileSystemCache.CheckSourceChanged] = "true"
                    }
                })
                .SetCacheHash<CacheHash>()
                .SetAsyncKeyLock<AsyncKeyLock>()
                .AddResolver<PhysicalFileSystemResolver>()
                .AddProcessor<ResizeWebProcessor>()
                .AddProcessor<FormatWebProcessor>()
                .AddProcessor<BackgroundColorWebProcessor>();

            // Add the default service and options.
            //
            //services.AddImageSharp();

            // Or add the default service and custom options.
            //services.AddImageSharp(
            //    options =>
            //        {
            //            options.Configuration = Configuration.Default;
            //            options.MaxBrowserCacheDays = 7;
            //            options.MaxCacheDays = 365;
            //            options.CachedNameLength = 8;
            //            options.OnValidate = _ => { };
            //            options.OnBeforeSave = _ => { };
            //            options.OnProcessed = _ => { };
            //            options.OnPrepareResponse = _ => { };
            //        });

            // Or we can fine-grain control adding the default options and configure all other services.
            //
            //services.AddImageSharpCore()
            //        .SetRequestParser<QueryCollectionRequestParser>()
            //        .SetMemoryAllocator<ArrayPoolMemoryAllocator>()
            //        .SetCache<PhysicalFileSystemCache>()
            //        .SetCacheHash<CacheHash>()
            //        .SetAsyncKeyLock<AsyncKeyLock>()
            //        .AddResolver<PhysicalFileSystemResolver>()
            //        .AddProcessor<ResizeWebProcessor>()
            //        .AddProcessor<FormatWebProcessor>()
            //        .AddProcessor<BackgroundColorWebProcessor>();


            // Or we can fine-grain control adding custom options and configure all other services
            // There are also factory methods for each builder that will allow building from configuration files.
            //
            //services.AddImageSharpCore(
            //    options =>
            //        {
            //            options.Configuration = Configuration.Default;
            //            options.MaxBrowserCacheDays = 7;
            //            options.MaxCacheDays = 365;
            //            options.CachedNameLength = 8;
            //            options.OnValidate = _ => { };
            //            options.OnBeforeSave = _ => { };
            //            options.OnProcessed = _ => { };
            //            options.OnPrepareResponse = _ => { };
            //        })
            //    .SetRequestParser<QueryCollectionRequestParser>()
            //    .SetMemoryAllocator(provider => ArrayPoolMemoryAllocator.CreateWithMinimalPooling())
            //    .SetCache(provider =>
            //      {
            //          var p = new PhysicalFileSystemCache(
            //              provider.GetRequiredService<IHostingEnvironment>(),
            //              provider.GetRequiredService<MemoryAllocator>(),
            //              provider.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>());

            //          p.Settings[PhysicalFileSystemCache.Folder] = PhysicalFileSystemCache.DefaultCacheFolder;
            //          p.Settings[PhysicalFileSystemCache.CheckSourceChanged] = "true";

            //          return p;
            //      })
            //    .SetCacheHash<CacheHash>()
            //    .SetAsyncKeyLock<AsyncKeyLock>()
            //    .AddResolver<PhysicalFileSystemResolver>()
            //    .AddProcessor<ResizeWebProcessor>()
            //    .AddProcessor<FormatWebProcessor>()
            //    .AddProcessor<BackgroundColorWebProcessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            app.UseDeveloperExceptionPage();
            app.UseDefaultFiles();
            app.UseImageSharp();
            app.UseStaticFiles();
        }
    }
}
