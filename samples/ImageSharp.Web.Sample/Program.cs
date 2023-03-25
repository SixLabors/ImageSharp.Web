// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace ImageSharp.Web.Sample;

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

        // Add services to the container.
        IServiceCollection services = builder.Services;
        services.AddRazorPages();

        // TODO: Enable HMAC
        services.AddImageSharp(options => options.HMACSecretKey = new byte[] { 1, 2, 3, 4, 5 })
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
            .Configure<PhysicalFileSystemProviderOptions>(options => options.ProviderRootPath = null)
            .AddProvider<PhysicalFileSystemProvider>()
            .AddProcessor<ResizeWebProcessor>()
            .AddProcessor<FormatWebProcessor>()
            .AddProcessor<BackgroundColorWebProcessor>()
            .AddProcessor<QualityWebProcessor>()
            .AddProcessor<AutoOrientWebProcessor>();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseImageSharp();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
