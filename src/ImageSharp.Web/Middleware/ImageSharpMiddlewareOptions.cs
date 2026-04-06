// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace SixLabors.ImageSharp.Web.Middleware;

/// <summary>
/// Configuration options for the <see cref="ImageSharpMiddleware"/> middleware.
/// </summary>
public class ImageSharpMiddlewareOptions
{
    private static readonly Configuration DefaultConfiguration = CreateDefaultConfiguration();

    private Func<ImageCommandContext, byte[], string> onComputeHMAC = (context, secret) =>
    {
        string uri = CaseHandlingUriBuilder.BuildRelative(
             CaseHandlingUriBuilder.CaseHandling.LowerInvariant,
             context.Context.Request.PathBase,
             context.Context.Request.Path,
             QueryString.Create(context.Commands));

        return HMACUtilities.ComputeHMACSHA256(uri, secret);
    };

    private Func<ImageCommandContext, Task> onParseCommandsAsync = context =>
    {
        // WEBP EXIF orientation is ignored by browsers.
        // https://zpl.fi/exif-orientation-in-different-formats/
        // To ensure that orientation is handled correctly for web use we transparently add the auto-orient command if it is not already present.
        // See issues #304, #375, and #381 for more details.
        if (!context.Commands.Contains(AutoOrientWebProcessor.AutoOrient))
        {
            context.Commands.Insert(0, new KeyValuePair<string, string?>(AutoOrientWebProcessor.AutoOrient, bool.TrueString));
        }

        return Task.CompletedTask;
    };

    private Func<ImageCommandContext, Configuration, Task<DecoderOptions?>> onBeforeLoadAsync = (_, _) => Task.FromResult<DecoderOptions?>(null);
    private Func<FormattedImage, Task> onBeforeSaveAsync = _ => Task.CompletedTask;
    private Func<ImageProcessingContext, Task> onProcessedAsync = _ => Task.CompletedTask;
    private Func<HttpContext, Task> onPrepareResponseAsync = _ => Task.CompletedTask;

    /// <summary>
    /// Gets or sets the base library configuration.
    /// </summary>
    public Configuration Configuration { get; set; } = DefaultConfiguration;

    /// <summary>
    /// Gets a value indicating whether the current configuration is the default configuration.
    /// </summary>
    internal bool HasDefaultConfiguration => ReferenceEquals(this.Configuration, DefaultConfiguration);

    /// <summary>
    /// Gets or sets the recyclable memorystream manager used for managing pooled stream
    /// buffers independently from image buffer pooling.
    /// </summary>
    public RecyclableMemoryStreamManager MemoryStreamManager { get; set; } = new RecyclableMemoryStreamManager();

    /// <summary>
    /// Gets or sets a value indicating whether to use culture-independent (invariant)
    /// conversion when converting commands.
    /// If set to <see langword="false"/> the <see cref="CommandParser"/> will use
    /// the <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    public bool UseInvariantParsingCulture { get; set; } = true;

    /// <summary>
    /// Gets or sets the duration to store images in the browser cache.
    /// If an image provider provides a Max-Age for a source image then that will override
    /// this value.
    /// <para>
    /// Defaults to 7 days.
    /// </para>
    /// </summary>
    public TimeSpan BrowserMaxAge { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets the duration to store images in the image cache.
    /// <para>
    /// Defaults to 365 days.
    /// </para>
    /// </summary>
    public TimeSpan CacheMaxAge { get; set; } = TimeSpan.FromDays(365);

    /// <summary>
    /// Gets or sets the length of the filename to use (minus the extension) when storing
    /// images in the image cache. Defaults to 12 characters.
    /// </summary>
    public uint CacheHashLength { get; set; } = 12;

    /// <summary>
    /// Gets or sets the secret key for Hash-based Message Authentication Code (HMAC) encryption.
    /// </summary>
    /// <remarks>
    /// The key can be any length. However, the recommended size is at least 64 bytes. If the length is zero then no authentication is performed.
    /// </remarks>
    public byte[] HMACSecretKey { get; set; } = [];

    /// <summary>
    /// Gets or sets the method used to compute a Hash-based Message Authentication Code (HMAC) for request authentication.
    /// Defaults to <see cref="HMACUtilities.ComputeHMACSHA256(string, byte[])"/> using an invariant lowercase relative Uri
    /// generated using <see cref="CaseHandlingUriBuilder.BuildRelative(CaseHandlingUriBuilder.CaseHandling, PathString, PathString, QueryString)"/>.
    /// </summary>
    public Func<ImageCommandContext, byte[], string> OnComputeHMAC
    {
        get => this.onComputeHMAC;

        set
        {
            Guard.NotNull(value, nameof(this.onComputeHMAC));
            this.onComputeHMAC = value;
        }
    }

    /// <summary>
    /// Gets or sets the additional command parsing method that can be used to used to augment commands.
    /// This is called once the commands have been gathered and before an <see cref="IImageProvider"/> has been assigned.
    /// </summary>
    public Func<ImageCommandContext, Task> OnParseCommandsAsync
    {
        get => this.onParseCommandsAsync;

        set
        {
            Guard.NotNull(value, nameof(this.OnParseCommandsAsync));
            this.onParseCommandsAsync = value;
        }
    }

    /// <summary>
    /// Gets or sets the method that can be used to used to augment decoder options.
    /// This is called before the image is decoded and loaded for processing.
    /// </summary>
    public Func<ImageCommandContext, Configuration, Task<DecoderOptions?>> OnBeforeLoadAsync
    {
        get => this.onBeforeLoadAsync;

        set
        {
            Guard.NotNull(value, nameof(this.OnBeforeLoadAsync));
            this.onBeforeLoadAsync = value;
        }
    }

    /// <summary>
    /// Gets or sets the additional method that can be used for final manipulation before the image is saved.
    /// This is called after image has been processed, but before the image has been saved to the output stream for caching.
    /// This can be used to alter the metadata of the resultant image.
    /// </summary>
    public Func<FormattedImage, Task> OnBeforeSaveAsync
    {
        get => this.onBeforeSaveAsync;

        set
        {
            Guard.NotNull(value, nameof(this.OnBeforeSaveAsync));
            this.onBeforeSaveAsync = value;
        }
    }

    /// <summary>
    /// Gets or sets the additional processing method.
    /// This is called after image has been processed, but before the result has been cached.
    /// This can be used to further optimize the resultant image.
    /// </summary>
    public Func<ImageProcessingContext, Task> OnProcessedAsync
    {
        get => this.onProcessedAsync;

        set
        {
            Guard.NotNull(value, nameof(this.OnProcessedAsync));
            this.onProcessedAsync = value;
        }
    }

    /// <summary>
    /// Gets or sets the additional response method.
    /// This is called after the status code and headers have been set, but before the body has been written.
    /// This can be used to add or change the response headers.
    /// </summary>
    public Func<HttpContext, Task> OnPrepareResponseAsync
    {
        get => this.onPrepareResponseAsync;

        set
        {
            Guard.NotNull(value, nameof(this.OnPrepareResponseAsync));
            this.onPrepareResponseAsync = value;
        }
    }

    private static Configuration CreateDefaultConfiguration()
    {
        // Build a Configuration for the requests that replaces the default JPEG, PNG, and WebP encoders
        // with ones with compression options that are more suitable for web use.
        // We do not skip metadata as that can affect things like orientation.
        Configuration configuration = Configuration.Default.Clone();
        configuration.ImageFormatsManager.SetEncoder(JpegFormat.Instance, new JpegEncoder()
        {
            Quality = 75,
            Progressive = true,
            Interleaved = true,
            ColorType = JpegColorType.YCbCrRatio420,
        });

        configuration.ImageFormatsManager.SetEncoder(PngFormat.Instance, new PngEncoder()
        {
            CompressionLevel = PngCompressionLevel.BestCompression,
            FilterMethod = PngFilterMethod.Adaptive,
        });

        configuration.ImageFormatsManager.SetEncoder(WebpFormat.Instance, new WebpEncoder()
        {
            Quality = 75,
            Method = WebpEncodingMethod.BestQuality,
        });

        return configuration;
    }
}
