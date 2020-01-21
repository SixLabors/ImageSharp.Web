// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SixLabors.ImageSharp.Web.Sample
{
    /// <summary>
    /// The running application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point to the running application.
        /// </summary>
        /// <param name="args">Any arguments to pass to the application.</param>
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        /// <summary>
        /// Creates an <see cref="IHostBuilder"/> instance used to create a configured <see cref="IHost"/>.
        /// </summary>
        /// <param name="args">Any arguments to pass to the application.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
