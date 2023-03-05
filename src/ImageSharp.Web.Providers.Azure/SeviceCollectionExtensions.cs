// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching.Azure;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Providers.Azure;

namespace SixLabors.ImageSharp.Web;

public static class SeviceCollectionExtensions
{
    /// <summary>
    /// Adds AzureBlobStorage as Cache provider.
    /// </summary>
    /// <param name="imageSharpBuilder">The builder</param>
    /// <param name="setupAction">The setup action</param>
    /// <returns>An <see cref="IImageSharpBuilder"/> that can be used to further configure the ImageSharp services.</returns>
    public static IImageSharpBuilder AddAzureBlobCache(this IImageSharpBuilder imageSharpBuilder, Action<AzureBlobStorageCacheOptions> setupAction) =>
        imageSharpBuilder.Configure(setupAction)
            .SetCache<AzureBlobStorageCache>();

    /// <summary>
    /// Adds the AzureBlobStorageImageProvider.
    /// </summary>
    /// <param name="imageSharpBuilder">The builder</param>
    /// <param name="setupAction">The setup action</param>
    /// <returns>An <see cref="IImageSharpBuilder"/> that can be used to further configure the ImageSharp services.</returns>
    public static IImageSharpBuilder AddAzureBlobImageProvider(this IImageSharpBuilder imageSharpBuilder, Action<AzureBlobStorageImageProviderOptions> setupAction) =>
        imageSharpBuilder.Configure(setupAction)
            .AddProvider<AzureBlobStorageImageProvider>();
}
