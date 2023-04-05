// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities;

public abstract class TestServerFixture : IDisposable
{
    private const int Width = 20;
    private static readonly string Command = "?width=" + Width + "&v=" + Guid.NewGuid().ToString();
    private static readonly string Command2 = "?width=" + (Width + 1) + "&v=" + Guid.NewGuid().ToString();

    private TestServer server;
    private bool isDisposed;

    protected TestServerFixture()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string>("webroot", string.Empty)
        })
        .AddEnvironmentVariables()
        .Build();

        IWebHostBuilder builder = new WebHostBuilder()
            .UseConfiguration(configuration)
            .Configure(this.Configure)
            .ConfigureServices(this.ConfigureServices);

        this.server = new TestServer(builder);
        this.Services = this.server.Host.Services;
        this.HttpClient = this.server.CreateClient();
    }

    public HttpClient HttpClient { get; private set; }

    public IServiceProvider Services { get; }

    public virtual IReadOnlyList<string> Commands { get; } = new List<string>
    {
        Command,
        Command2
    };

    protected void ConfigureServices(IServiceCollection services)
    {
        IImageSharpBuilder builder = services.AddImageSharp(options =>
        {
            Func<ImageCommandContext, Task> onParseCommandsAsync = options.OnParseCommandsAsync;

            options.OnParseCommandsAsync = context =>
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Context);
                Assert.NotNull(context.Commands);
                Assert.NotNull(context.Parser);

                return onParseCommandsAsync.Invoke(context);
            };

            Func<ImageCommandContext, Configuration, Task<DecoderOptions?>> onBeforeLoadAsync = options.OnBeforeLoadAsync;

            options.OnBeforeLoadAsync = (context, configuration) =>
            {
                Assert.NotNull(context);
                Assert.NotNull(configuration);

                return onBeforeLoadAsync.Invoke(context, configuration);
            };

            Func<ImageProcessingContext, Task> onProcessedAsync = options.OnProcessedAsync;

            options.OnProcessedAsync = context =>
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Commands);
                Assert.NotNull(context.ContentType);
                Assert.NotNull(context.Context);
                Assert.NotNull(context.Extension);
                Assert.NotNull(context.Stream);

                return onProcessedAsync.Invoke(context);
            };

            Func<FormattedImage, Task> onBeforeSaveAsync = options.OnBeforeSaveAsync;

            options.OnBeforeSaveAsync = context =>
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Format);
                Assert.NotNull(context.Encoder);
                Assert.NotNull(context.Image);

                return onBeforeSaveAsync.Invoke(context);
            };

            Func<HttpContext, Task> onPrepareResponseAsync = options.OnPrepareResponseAsync;

            options.OnPrepareResponseAsync = context =>
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Response);

                return onPrepareResponseAsync.Invoke(context);
            };

            this.ConfigureImageSharpOptions(options);
        })
        .ClearProviders()
        .AddProcessor<CacheBusterWebProcessor>();

        this.ConfigureCustomServices(services, builder);
    }

    protected virtual void Configure(IApplicationBuilder app) => app.UseImageSharp();

    protected virtual void ConfigureImageSharpOptions(ImageSharpMiddlewareOptions options)
    {
    }

    protected abstract void ConfigureCustomServices(IServiceCollection services, IImageSharpBuilder builder);

    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                this.server.Dispose();
                this.HttpClient.Dispose();
            }

            this.server = null;
            this.HttpClient = null;
            this.isDisposed = true;
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
}
