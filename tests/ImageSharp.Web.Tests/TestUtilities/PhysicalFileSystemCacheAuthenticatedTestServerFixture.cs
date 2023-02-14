// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Providers;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities;

public class PhysicalFileSystemCacheAuthenticatedTestServerFixture : AuthenticatedTestServerFixture
{
    protected override void ConfigureCustomServices(IServiceCollection services, IImageSharpBuilder builder)
        => builder
        .AddProvider<PhysicalFileSystemProvider>()
        .SetCache<PhysicalFileSystemCache>();
}
