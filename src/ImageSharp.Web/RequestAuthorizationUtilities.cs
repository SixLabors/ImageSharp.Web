// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Contains various helper methods for authorizing image requests.
/// </summary>
public sealed class RequestAuthorizationUtilities
{
    /// <summary>
    /// The command used by image requests for transporting Hash-based Message Authentication Code (HMAC) tokens.
    /// </summary>
    public const string TokenCommand = HMACUtilities.TokenCommand;
    private static readonly Uri FallbackBaseUri = new("http://localhost/");
    private readonly HashSet<string> knownCommands;
    private readonly ImageSharpMiddlewareOptions options;
    private readonly CommandParser commandParser;
    private readonly CultureInfo parserCulture;
    private readonly IRequestParser requestParser;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestAuthorizationUtilities"/> class.
    /// </summary>
    /// <param name="options">The middleware configuration options.</param>
    /// <param name="requestParser">An <see cref="IRequestParser"/> instance used to parse image requests for commands.</param>
    /// <param name="processors">A collection of <see cref="IImageWebProcessor"/> instances used to process images.</param>
    /// <param name="commandParser">The command parser.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public RequestAuthorizationUtilities(
        IOptions<ImageSharpMiddlewareOptions> options,
        IRequestParser requestParser,
        IEnumerable<IImageWebProcessor> processors,
        CommandParser commandParser,
        IServiceProvider serviceProvider)
    {
        Guard.NotNull(options, nameof(options));
        Guard.NotNull(requestParser, nameof(requestParser));
        Guard.NotNull(processors, nameof(processors));
        Guard.NotNull(commandParser, nameof(commandParser));
        Guard.NotNull(serviceProvider, nameof(serviceProvider));

        this.options = options.Value;
        this.requestParser = requestParser;
        this.commandParser = commandParser;
        this.parserCulture = this.options.UseInvariantParsingCulture
            ? CultureInfo.InvariantCulture
            : CultureInfo.CurrentCulture;
        this.serviceProvider = serviceProvider;

        HashSet<string> commands = new(StringComparer.OrdinalIgnoreCase);
        foreach (IImageWebProcessor processor in processors)
        {
            foreach (string command in processor.Commands)
            {
                commands.Add(command);
            }
        }

        this.knownCommands = commands;
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
            List<string> keys = new(commands.Keys);
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
    /// <param name="handling">The command collection handling.</param>
    /// <returns>The computed HMAC.</returns>
    public string? ComputeHMAC(string uri, CommandHandling handling)
        => this.ComputeHMAC(new Uri(uri, UriKind.RelativeOrAbsolute), handling);

    /// <summary>
    /// Compute a Hash-based Message Authentication Code (HMAC) for request authentication.
    /// </summary>
    /// <param name="uri">The uri to compute the code from.</param>
    /// <param name="handling">The command collection handling.</param>
    /// <returns>The computed HMAC.</returns>
    public string? ComputeHMAC(Uri uri, CommandHandling handling)
    {
        ToComponents(
            uri,
            out HostString host,
            out PathString path,
            out QueryString queryString);

        return this.ComputeHMAC(host, path, queryString, handling);
    }

    /// <summary>
    /// Compute a Hash-based Message Authentication Code (HMAC) for request authentication.
    /// </summary>
    /// <param name="host">The host header.</param>
    /// <param name="path">The path or pathbase.</param>
    /// <param name="queryString">The querystring.</param>
    /// <param name="handling">The command collection handling.</param>
    /// <returns>The computed HMAC.</returns>
    public string? ComputeHMAC(HostString host, PathString path, QueryString queryString, CommandHandling handling)
        => this.ComputeHMAC(host, path, queryString, new(QueryHelpers.ParseQuery(queryString.Value)), handling);

    /// <summary>
    /// Compute a Hash-based Message Authentication Code (HMAC) for request authentication.
    /// </summary>
    /// <param name="host">The host header.</param>
    /// <param name="path">The path or pathbase.</param>
    /// <param name="queryString">The querystring.</param>
    /// <param name="query">The query collection.</param>
    /// <param name="handling">The command collection handling.</param>
    /// <returns>The computed HMAC.</returns>
    public string? ComputeHMAC(HostString host, PathString path, QueryString queryString, QueryCollection query, CommandHandling handling)
        => this.ComputeHMAC(this.ToHttpContext(host, path, queryString, query), handling);

    /// <summary>
    /// Compute a Hash-based Message Authentication Code (HMAC) for request authentication.
    /// </summary>
    /// <param name="context">The request HTTP context.</param>
    /// <param name="handling">The command collection handling.</param>
    /// <returns>The computed HMAC.</returns>
    public string? ComputeHMAC(HttpContext context, CommandHandling handling)
    {
        byte[] secret = this.options.HMACSecretKey;
        if (secret is null || secret.Length == 0)
        {
            return null;
        }

        CommandCollection commands = this.requestParser.ParseRequestCommands(context);
        if (handling == CommandHandling.Sanitize)
        {
            this.StripUnknownCommands(commands);
        }

        if (commands.Count == 0)
        {
            return null;
        }

        ImageCommandContext imageCommandContext = new(context, commands, this.commandParser, this.parserCulture);
        return this.options.OnComputeHMAC(imageCommandContext, secret);
    }

    /// <summary>
    /// Compute a Hash-based Message Authentication Code (HMAC) for request authentication.
    /// </summary>
    /// <remarks>
    /// This method is only called by the middleware and only if required.
    /// As such, standard checks are avoided.
    /// </remarks>
    /// <param name="context">Contains information about the current image request and parsed commands.</param>
    /// <returns>The computed HMAC.</returns>
    internal string? ComputeHMAC(ImageCommandContext context)
    {
        if (context.Commands.Count == 0)
        {
            return null;
        }

        return this.options.OnComputeHMAC(context, this.options.HMACSecretKey);
    }

    private static void ToComponents(
        Uri uri,
        out HostString host,
        out PathString path,
        out QueryString queryString)
    {
        if (uri.IsAbsoluteUri)
        {
            host = HostString.FromUriComponent(uri);
            path = PathString.FromUriComponent(uri);
            queryString = QueryString.FromUriComponent(uri);
        }
        else
        {
            Uri faux = new(FallbackBaseUri, uri);
            host = default;
            path = PathString.FromUriComponent(faux);
            queryString = QueryString.FromUriComponent(faux);
        }
    }

    private HttpContext ToHttpContext(HostString host, PathString path, QueryString queryString, QueryCollection query)
    {
        DefaultHttpContext context = new() { RequestServices = this.serviceProvider };
        HttpRequest request = context.Request;
        request.Method = HttpMethods.Get;
        request.Host = host;
        request.Path = path;
        request.QueryString = queryString;
        request.Query = query;

        return context;
    }
}
