// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities;

public class AuthenticatedTestServerFixture : TestServerFixture
{
    public static byte[] HMACSecretKey { get; } = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };

    protected override void ConfigureImageSharpOptions(ImageSharpMiddlewareOptions options)
    {
        base.ConfigureImageSharpOptions(options);
        options.HMACSecretKey = HMACSecretKey;
    }

    protected override void ConfigureCustomServices(IServiceCollection services, IImageSharpBuilder builder)
    {
    }
}
