// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

#if !NETCOREAPP3_0_OR_GREATER
using Microsoft.AspNetCore.Http.Internal;
#endif

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Contains various helper methods for authorizing image requests.
    /// </summary>
    public sealed class ImageSharpRequestAuthorizationUtilities
    {
        /// <summary>
        /// The command used by image requests for transporting Hash-based Message Authentication Code (HMAC) tokens.
        /// </summary>
        public const string TokenCommand = HMACUtilities.TokenCommand;

        private static readonly Uri FallbackBaseUri = new("http://localhost/");
        private readonly HashSet<string> knownCommands;
        private readonly ImageSharpMiddlewareOptions options;
        private readonly IRequestParser requestParser;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpRequestAuthorizationUtilities"/> class.
        /// </summary>
        /// <param name="options">The middleware configuration options.</param>
        /// <param name="requestParser">An <see cref="IRequestParser"/> instance used to parse image requests for commands.</param>
        /// <param name="processors">A collection of <see cref="IImageWebProcessor"/> instances used to process images.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public ImageSharpRequestAuthorizationUtilities(
            IOptions<ImageSharpMiddlewareOptions> options,
            IRequestParser requestParser,
            IEnumerable<IImageWebProcessor> processors,
            IServiceProvider serviceProvider)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(requestParser, nameof(requestParser));
            Guard.NotNull(processors, nameof(processors));
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            HashSet<string> commands = new(StringComparer.OrdinalIgnoreCase);
            foreach (IImageWebProcessor processor in processors)
            {
                foreach (string command in processor.Commands)
                {
                    commands.Add(command);
                }
            }

            this.knownCommands = commands;
            this.options = options.Value;
        }

        /// <summary>
        /// Strips any unknown commands from the command collection.
        /// </summary>
        /// <param name="commands">The unsanitized command collection.</param>
        public void StripUnknownCommands(CommandCollection commands)
        {
            if (commands?.Count > 0)
            {
                // Strip out any unknown commands, if needed.
                var keys = new List<string>(commands.Keys);
                for (int i = keys.Count - 1; i >= 0; i--)
                {
                    if (!this.knownCommands.Contains(keys[i]))
                    {
                        commands.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Compute a Hash-based Message Authentication Code (HMAC) for request authentication.
        /// </summary>
        /// <param name="uri">The uri to compute the code from.</param>
        /// <returns>The computed HMAC.</returns>
        public string ComputeHMAC(string uri)
            => this.ComputeHMAC(new Uri(uri));

        /// <summary>
        /// Compute a Hash-based Message Authentication Code (HMAC) for request authentication.
        /// </summary>
        /// <param name="uri">The uri to compute the code from.</param>
        /// <returns>The computed HMAC.</returns>
        public string ComputeHMAC(Uri uri)
            => AsyncHelper.RunSync(() => this.ComputeHMACAsync(uri));

        /// <summary>
        /// Compute a Hash-based Message Authentication Code (HMAC) for request authentication.
        /// </summary>
        /// <param name="uri">The uri to compute the code from.</param>
        /// <returns>The computed HMAC.</returns>
        public Task<string> ComputeHMACAsync(string uri)
            => this.ComputeHMACAsync(new Uri(uri));

        /// <summary>
        /// Compute a Hash-based Message Authentication Code (HMAC) for request authentication.
        /// </summary>
        /// <param name="uri">The uri to compute the code from.</param>
        /// <returns>The computed HMAC.</returns>
        public async Task<string> ComputeHMACAsync(Uri uri)
        {
            byte[] secret = this.options.HMACSecretKey;
            if (secret is null || secret.Length == 0)
            {
                return null;
            }

            // We need to generate a HttpRequest to use the rest of the services.
            DefaultHttpContext context = new() { RequestServices = this.serviceProvider };
            HttpRequest request = context.Request;

            ToComponents(
                uri,
                out HostString host,
                out PathString path,
                out QueryString queryString,
                out QueryCollection query);

            request.Host = host;
            request.Path = path;
            request.QueryString = queryString;
            request.Query = query;

            CommandCollection commands = this.requestParser.ParseRequestCommands(context);

            // The provided URI should not contain invalid commands since image URI geneneration
            // should be tightly controlled by the running application but we will strip any out anyway.
            this.StripUnknownCommands(commands);

            ImageCommandContext imageCommandContext = new(context, commands, null, null);
            return await this.options.OnComputeHMACAsync(imageCommandContext, secret);
        }

        private static void ToComponents(
            Uri uri,
            out HostString host,
            out PathString path,
            out QueryString queryString,
            out QueryCollection query)
        {
            if (uri.IsAbsoluteUri)
            {
                host = HostString.FromUriComponent(uri);
                path = PathString.FromUriComponent(uri);
                queryString = QueryString.FromUriComponent(uri);
                query = GetQueryComponent(queryString);
            }
            else
            {
                Uri faux = new(FallbackBaseUri, uri);
                host = default;
                path = PathString.FromUriComponent(faux);
                queryString = QueryString.FromUriComponent(faux);
                query = GetQueryComponent(queryString);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static QueryCollection GetQueryComponent(QueryString query)
            => new(QueryHelpers.ParseQuery(query.Value));
    }
}
