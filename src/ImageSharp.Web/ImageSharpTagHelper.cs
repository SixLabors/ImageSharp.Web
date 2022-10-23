// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// A TagHelper implementation targeting &lt;img&gt; element that allows the automatic generation of HMAC protected processing commands.
    /// </summary>
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + WidthAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + HeightAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + AnchorAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + RModeAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + XyAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + RColorAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + CompandAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + OrientAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + AutoOrientAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + FormatAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + BgColorAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = SrcAttributeName + "," + QualityAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class ImageSharpTagHelper : UrlResolutionTagHelper
    {
        private const string SrcAttributeName = "src";
        private const string WidthAttributeName = ResizeWebProcessor.Width;
        private const string HeightAttributeName = ResizeWebProcessor.Height;
        private const string AttributePrefix = "is-";
        private const string AnchorAttributeName = AttributePrefix + ResizeWebProcessor.Anchor;
        private const string RModeAttributeName = AttributePrefix + ResizeWebProcessor.Mode;
        private const string XyAttributeName = AttributePrefix + ResizeWebProcessor.Xy;
        private const string RColorAttributeName = AttributePrefix + ResizeWebProcessor.Color;
        private const string CompandAttributeName = AttributePrefix + ResizeWebProcessor.Compand;
        private const string OrientAttributeName = AttributePrefix + ResizeWebProcessor.Orient;
        private const string SamplerAttributeName = AttributePrefix + ResizeWebProcessor.Sampler;
        private const string AutoOrientAttributeName = AttributePrefix + AutoOrientWebProcessor.AutoOrient;
        private const string FormatAttributeName = AttributePrefix + FormatWebProcessor.Format;
        private const string BgColorAttributeName = AttributePrefix + BackgroundColorWebProcessor.Color;
        private const string QualityAttributeName = AttributePrefix + QualityWebProcessor.Quality;

        private readonly ImageSharpMiddlewareOptions options;
        private readonly CultureInfo parserCulture;
        private readonly char separator;
        private readonly RequestAuthorizationUtilities authorizationUtilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpTagHelper"/> class.
        /// </summary>
        /// <param name="options">The middleware configuration options.</param>
        /// <param name="authorizationUtilities">Contains helpers that allow authorization of image requests.</param>
        /// <param name="urlHelperFactory">The URL helper factory.</param>
        /// <param name="htmlEncoder">The HTML encorder.</param>
        public ImageSharpTagHelper(
            IOptions<ImageSharpMiddlewareOptions> options,
            RequestAuthorizationUtilities authorizationUtilities,
            IUrlHelperFactory urlHelperFactory,
            HtmlEncoder htmlEncoder)
            : base(urlHelperFactory, htmlEncoder)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(authorizationUtilities, nameof(authorizationUtilities));

            this.options = options.Value;
            this.parserCulture = this.options.UseInvariantParsingCulture
                ? CultureInfo.InvariantCulture
                : CultureInfo.CurrentCulture;
            this.separator = this.parserCulture.TextInfo.ListSeparator[0];

            this.authorizationUtilities = authorizationUtilities;
        }

        /// <summary>
        /// Gets or sets the src.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases.
        /// </remarks>
        [HtmlAttributeName(SrcAttributeName)]
        public string Src { get; set; }

        /// <summary>
        /// Gets or sets the width in pixel units.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases.
        /// </remarks>
        [HtmlAttributeName(WidthAttributeName)]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height in pixel units.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases.
        /// </remarks>
        [HtmlAttributeName(HeightAttributeName)]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the resize mode.
        /// </summary>
        [HtmlAttributeName(RModeAttributeName)]
        public ResizeMode? ResizeMode { get; set; }

        /// <summary>
        /// Gets or sets the anchor position.
        /// </summary>
        [HtmlAttributeName(AnchorAttributeName)]
        public AnchorPositionMode? Position { get; set; }

        /// <summary>
        /// Gets or sets the center coordinates.
        /// </summary>
        [HtmlAttributeName(XyAttributeName)]
        public PointF? Center { get; set; }

        /// <summary>
        /// Gets or sets the color to use as a background when padding an image.
        /// </summary>
        [HtmlAttributeName(RColorAttributeName)]
        public Color? PadColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to compress
        /// or expand individual pixel colors values on processing.
        /// </summary>
        [HtmlAttributeName(CompandAttributeName)]
        public bool? Compand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to factor embedded
        /// EXIF orientation property values during processing.
        /// </summary>
        /// <remarks>Defaults to <see langword="true"/>.</remarks>
        [HtmlAttributeName(OrientAttributeName)]
        public bool? Orient { get; set; }

        /// <summary>
        /// Gets or sets the sampling algorithm to use when resizing images.
        /// </summary>
        [HtmlAttributeName(SamplerAttributeName)]
        public Resampler? Sampler { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically
        /// rotate/flip the iput image based on embedded EXIF orientation property values
        /// before processing.
        /// </summary>
        [HtmlAttributeName(AutoOrientAttributeName)]
        public bool? AutoOrient { get; set; }

        /// <summary>
        /// Gets or sets the image format to convert to.
        /// </summary>
        [HtmlAttributeName(FormatAttributeName)]
        public Format? Format { get; set; }

        /// <summary>
        /// Gets or sets the background color of the image.
        /// </summary>
        [HtmlAttributeName(BgColorAttributeName)]
        public Color? BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the quality, that will be used to encode the image. Quality
        /// index must be between 0 and 100 (compression from max to min).
        /// </summary>
        [HtmlAttributeName(QualityAttributeName)]
        public int? Quality { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(output, nameof(output));

            string src = output.Attributes[SrcAttributeName]?.Value as string ?? this.Src;
            if (string.IsNullOrWhiteSpace(src))
            {
                return;
            }

            output.CopyHtmlAttribute(SrcAttributeName, context);
            this.ProcessUrlAttribute(SrcAttributeName, output);

            CommandCollection commands = new();
            this.AddProcessingCommands(context, output, commands, this.parserCulture);

            if (commands.Count > 0)
            {
                // Retrieve the TagHelperOutput variation of the "src" attribute in case other TagHelpers in the
                // pipeline have touched the value. If the value is already encoded this helper may
                // not function properly.
                src = output.Attributes[SrcAttributeName].Value as string;
                byte[] secret = this.options.HMACSecretKey;
                if (secret?.Length > 0)
                {
                    string hash = this.authorizationUtilities.ComputeHMAC(src, commands, secret);
                    commands.Add(RequestAuthorizationUtilities.TokenCommand, hash);
                }

                src = AddQueryString(src, commands);
            }

            this.Src = src;
            output.Attributes.SetAttribute(SrcAttributeName, src);
        }

        /// <summary>
        /// Allows the addition of processing commands by inheriting classes.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        /// <param name="commands">The command collection.</param>
        /// <param name="commandCulture">The culture to use when generating and processing commands.</param>
        protected virtual void AddProcessingCommands(
            TagHelperContext context,
            TagHelperOutput output,
            CommandCollection commands,
            CultureInfo commandCulture)
        {
            this.AddResizeCommands(output, commands);
            this.AddAutoOrientCommands(commands);
            this.AddFormatCommands(commands);
            this.AddBackgroundColorCommands(commands);
            this.AddQualityCommands(commands);
        }

        private void AddResizeCommands(TagHelperOutput output, CommandCollection commands)
        {
            if (this.Width.HasValue)
            {
                commands.Add(ResizeWebProcessor.Width, this.Width.Value.ToString(this.parserCulture));
                output.Attributes.SetAttribute(WidthAttributeName, this.Width);
            }

            if (this.Height.HasValue)
            {
                commands.Add(ResizeWebProcessor.Height, this.Height.Value.ToString(this.parserCulture));
                output.Attributes.SetAttribute(HeightAttributeName, this.Height);
            }

            if (this.ResizeMode.HasValue)
            {
                commands.Add(ResizeWebProcessor.Mode, this.ResizeMode.Value.ToString());
            }

            if (this.Position.HasValue)
            {
                commands.Add(ResizeWebProcessor.Anchor, this.Position.Value.ToString());
            }

            if (this.Center.HasValue)
            {
                string xy = $"{this.Center.Value.X.ToString(this.parserCulture)}{this.separator}{this.Center.Value.Y.ToString(this.parserCulture)}";
                commands.Add(ResizeWebProcessor.Xy, xy);
            }

            if (this.PadColor.HasValue)
            {
                commands.Add(ResizeWebProcessor.Color, this.PadColor.Value.ToHex());
            }

            if (this.Compand.HasValue)
            {
                commands.Add(ResizeWebProcessor.Compand, this.Compand.Value.ToString(this.parserCulture));
            }

            if (this.Orient.HasValue)
            {
                commands.Add(ResizeWebProcessor.Orient, this.Orient.Value.ToString(this.parserCulture));
            }

            if (this.Sampler.HasValue)
            {
                commands.Add(ResizeWebProcessor.Sampler, this.Sampler.Value.ToString());
            }
        }

        private void AddAutoOrientCommands(CommandCollection commands)
        {
            if (this.AutoOrient.HasValue)
            {
                commands.Add(AutoOrientWebProcessor.AutoOrient, this.AutoOrient.Value.ToString());
            }
        }

        private void AddFormatCommands(CommandCollection commands)
        {
            if (this.Format.HasValue)
            {
                commands.Add(FormatWebProcessor.Format, this.Format.Value.ToString());
            }
        }

        private void AddBackgroundColorCommands(CommandCollection commands)
        {
            if (this.BackgroundColor.HasValue)
            {
                commands.Add(BackgroundColorWebProcessor.Color, this.BackgroundColor.Value.ToHex());
            }
        }

        private void AddQualityCommands(CommandCollection commands)
        {
            if (this.Quality.HasValue)
            {
                commands.Add(QualityWebProcessor.Quality, this.Quality.Value.ToString(this.parserCulture));
            }
        }

        private static string AddQueryString(
            string uri,
            CommandCollection commands)
        {
            int anchorIndex = uri.IndexOf('#');
            string uriToBeAppended = uri;
            string anchorText = string.Empty;

            // If there is an anchor, then the query string must be inserted before its first occurrence.
            if (anchorIndex != -1)
            {
                anchorText = uri.Substring(anchorIndex);
                uriToBeAppended = uri.Substring(0, anchorIndex);
            }

            int queryIndex = uriToBeAppended.IndexOf('?');
            bool hasQuery = queryIndex != -1;

            StringBuilder sb = new();
            sb.Append(uriToBeAppended);

            foreach (KeyValuePair<string, string> parameter in commands)
            {
                if (parameter.Value == null)
                {
                    continue;
                }

                sb.Append(hasQuery ? '&' : '?');
                sb.Append(UrlEncoder.Default.Encode(parameter.Key));
                sb.Append('=');
                sb.Append(UrlEncoder.Default.Encode(parameter.Value));
                hasQuery = true;
            }

            sb.Append(anchorText);
            return sb.ToString();
        }
    }
}
