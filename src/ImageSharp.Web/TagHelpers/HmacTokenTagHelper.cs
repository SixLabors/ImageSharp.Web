// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Middleware;

namespace SixLabors.ImageSharp.Web.TagHelpers;

/// <summary>
/// A <see cref="TagHelper"/> implementation targeting &lt;img&gt; element that allows the automatic generation of HMAC image processing protection tokens.
/// </summary>
[HtmlTargetElement("img", Attributes = SrcAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public class HmacTokenTagHelper : UrlResolutionTagHelper
{
    private const string SrcAttributeName = "src";

    private readonly ImageSharpMiddlewareOptions options;
    private readonly RequestAuthorizationUtilities authorizationUtilities;

    /// <summary>
    /// Initializes a new instance of the <see cref="HmacTokenTagHelper" /> class.
    /// </summary>
    /// <param name="options">The middleware configuration options.</param>
    /// <param name="authorizationUtilities">Contains helpers that allow authorization of image requests.</param>
    /// <param name="urlHelperFactory">The URL helper factory.</param>
    /// <param name="htmlEncoder">The HTML encoder.</param>
    public HmacTokenTagHelper(
        IOptions<ImageSharpMiddlewareOptions> options,
        RequestAuthorizationUtilities authorizationUtilities,
        IUrlHelperFactory urlHelperFactory,
        HtmlEncoder htmlEncoder)
        : base(urlHelperFactory, htmlEncoder)
    {
        Guard.NotNull(options, nameof(options));
        Guard.NotNull(authorizationUtilities, nameof(authorizationUtilities));

        this.options = options.Value;
        this.authorizationUtilities = authorizationUtilities;
    }

    /// <inheritdoc/>
    public override int Order => 2;

    /// <summary>
    /// Gets or sets the source of the image.
    /// </summary>
    /// <remarks>
    /// Passed through to the generated HTML in all cases.
    /// </remarks>
    [HtmlAttributeName(SrcAttributeName)]
    public string? Src { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        Guard.NotNull(context, nameof(context));
        Guard.NotNull(output, nameof(output));

        output.CopyHtmlAttribute(SrcAttributeName, context);
        this.ProcessUrlAttribute(SrcAttributeName, output);

        byte[] secret = this.options.HMACSecretKey;
        if (secret is null || secret.Length == 0)
        {
            return;
        }

        // Retrieve the TagHelperOutput variation of the "src" attribute in case other TagHelpers in the
        // pipeline have touched the value. If the value is already encoded this ImageTagHelper may
        // not function properly.
        string? src = output.Attributes[SrcAttributeName]?.Value as string;
        if (string.IsNullOrWhiteSpace(src))
        {
            return;
        }

        string? hmac = this.authorizationUtilities.ComputeHMAC(src, CommandHandling.Sanitize);
        if (hmac is not null)
        {
            this.Src = AddQueryString(src, hmac);
            output.Attributes.SetAttribute(SrcAttributeName, this.Src);
        }
    }

    private static string AddQueryString(
        ReadOnlySpan<char> uri,
        string hmac)
    {
        ReadOnlySpan<char> uriToBeAppended = uri;
        ReadOnlySpan<char> anchorText = default;

        // If there is an anchor, then the query string must be inserted before its first occurrence.
        int anchorIndex = uri.IndexOf('#');
        if (anchorIndex != -1)
        {
            anchorText = uri[anchorIndex..];
            uriToBeAppended = uri[..anchorIndex];
        }

        int queryIndex = uriToBeAppended.IndexOf('?');
        bool hasQuery = queryIndex != -1;

        StringBuilder sb = new();

        sb.Append(uriToBeAppended)
          .Append(hasQuery ? '&' : '?')
          .Append(UrlEncoder.Default.Encode(RequestAuthorizationUtilities.TokenCommand))
          .Append('=')
          .Append(UrlEncoder.Default.Encode(hmac))
          .Append(anchorText);

        return sb.ToString();
    }
}
