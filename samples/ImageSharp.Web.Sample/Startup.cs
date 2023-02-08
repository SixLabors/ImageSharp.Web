// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace SixLabors.ImageSharp.Web.Sample;

/// <summary>
/// Contains application configuration allowing the addition of services to the container.
/// </summary>
public class Startup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration properties.</param>
    public Startup(IConfiguration configuration) => this.AppConfiguration = configuration;

    /// <summary>
    /// Gets the configuration properties.
    /// </summary>
    public IConfiguration AppConfiguration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services">The collection of service descriptors.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddImageSharp()
            .SetRequestParser<QueryCollectionRequestParser>()
            .Configure<PhysicalFileSystemCacheOptions>(options =>
            {
                options.CacheRootPath = null;
                options.CacheFolder = "is-cache";
                options.CacheFolderDepth = 8;
            })
            .SetCache<PhysicalFileSystemCache>()
            .SetCacheKey<UriRelativeLowerInvariantCacheKey>()
            .SetCacheHash<SHA256CacheHash>()
            .Configure<PhysicalFileSystemProviderOptions>(options =>
            {
                options.ProviderRootPath = null;
            })
            .AddProvider<PhysicalFileSystemProvider>()
            .AddProcessor<ResizeWebProcessor>()
            .AddProcessor<FormatWebProcessor>()
            .AddProcessor<BackgroundColorWebProcessor>()
            .AddProcessor<QualityWebProcessor>()
            .AddProcessor<AutoOrientWebProcessor>();

        // Add the default service and options.
        //
        // services.AddImageSharp();

        // Or add the default service and custom options.
        //
        // this.ConfigureDefaultServicesAndCustomOptions(services);

        // Or we can fine-grain control adding the default options and configure all other services.
        //
        // this.ConfigureCustomServicesAndDefaultOptions(services);

        // Or we can fine-grain control adding custom options and configure all other services
        // There are also factory methods for each builder that will allow building from configuration files.
        //
        // this.ConfigureCustomServicesAndCustomOptions(services);
    }

    private void ConfigureDefaultServicesAndCustomOptions(IServiceCollection services)
    {
        services.AddImageSharp(options =>
        {
            options.Configuration = Configuration.Default;
            options.BrowserMaxAge = TimeSpan.FromDays(7);
            options.CacheMaxAge = TimeSpan.FromDays(365);
            options.CacheHashLength = 8;
            options.OnParseCommandsAsync = _ => Task.CompletedTask;
            options.OnBeforeSaveAsync = _ => Task.CompletedTask;
            options.OnProcessedAsync = _ => Task.CompletedTask;
            options.OnPrepareResponseAsync = _ => Task.CompletedTask;
        });
    }

    private void ConfigureCustomServicesAndDefaultOptions(IServiceCollection services)
    {
        services.AddImageSharp()
                .RemoveProcessor<FormatWebProcessor>()
                .RemoveProcessor<BackgroundColorWebProcessor>();
    }

    private void ConfigureCustomServicesAndCustomOptions(IServiceCollection services)
    {
        services.AddImageSharp(options =>
        {
            options.Configuration = Configuration.Default;
            options.BrowserMaxAge = TimeSpan.FromDays(7);
            options.CacheMaxAge = TimeSpan.FromDays(365);
            options.CacheHashLength = 8;
            options.OnParseCommandsAsync = _ => Task.CompletedTask;
            options.OnBeforeSaveAsync = _ => Task.CompletedTask;
            options.OnProcessedAsync = _ => Task.CompletedTask;
            options.OnPrepareResponseAsync = _ => Task.CompletedTask;
        })
            .SetRequestParser<QueryCollectionRequestParser>()
            .Configure<PhysicalFileSystemCacheOptions>(options =>
            {
                options.CacheFolder = "different-cache";
            })
            .SetCache<PhysicalFileSystemCache>()
            .SetCacheKey<UriRelativeLowerInvariantCacheKey>()
            .SetCacheHash<SHA256CacheHash>()
            .ClearProviders()
            .AddProvider<PhysicalFileSystemProvider>()
            .ClearProcessors()
            .AddProcessor<ResizeWebProcessor>()
            .AddProcessor<FormatWebProcessor>()
            .AddProcessor<BackgroundColorWebProcessor>()
            .AddProcessor<QualityWebProcessor>();
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="env">The hosting environment the application is running in.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseDefaultFiles();
        app.UseImageSharp();
        app.UseStaticFiles();
    }
}
