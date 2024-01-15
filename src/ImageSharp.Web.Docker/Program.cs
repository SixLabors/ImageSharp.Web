// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace ImageSharp.Web.Docker;

/// <summary>
/// The running application.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main application entry point.
    /// </summary>
    /// <param name="args">Argument paramateres.</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddImageSharp()
            .SetRequestParser<QueryCollectionRequestParser>()
            .Configure<PhysicalFileSystemCacheOptions>(options => options.CacheFolder = "is-cache")
            .SetCache<PhysicalFileSystemCache>()
            .SetCacheKey<UriRelativeLowerInvariantCacheKey>()
            .SetCacheHash<SHA256CacheHash>()
            .Configure<PhysicalFileSystemProviderOptions>(options => options.ProviderRootPath = "wwwroot")
            .AddProvider<PhysicalFileSystemProvider>()
            .AddProcessor<ResizeWebProcessor>()
            .AddProcessor<FormatWebProcessor>()
            .AddProcessor<BackgroundColorWebProcessor>()
            .AddProcessor<QualityWebProcessor>()
            .AddProcessor<AutoOrientWebProcessor>();

        WebApplication app = builder.Build();

        app.UseImageSharp();

        app.UseStaticFiles();

        app.Run();
    }
}
