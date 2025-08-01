// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Contains various helper methods for working with image formats based on the given configuration.
/// </summary>
public sealed class FormatUtilities
{
    private readonly List<string> extensions = new();
    private readonly Dictionary<string, string> extensionsByMimeType = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatUtilities" /> class.
    /// </summary>
    /// <param name="options">The middleware options.</param>
    public FormatUtilities(IOptions<ImageSharpMiddlewareOptions> options)
    {
        Guard.NotNull(options, nameof(options));

        foreach (IImageFormat imageFormat in options.Value.Configuration.ImageFormats)
        {
            string[] extensions = imageFormat.FileExtensions.ToArray();

            this.extensions.AddRange(extensions);

            this.extensionsByMimeType[imageFormat.DefaultMimeType] = extensions[0];
        }
    }

    /// <summary>
    /// Gets the file extension for the given image uri.
    /// </summary>
    /// <param name="uri">The full request uri.</param>
    /// <param name="extension">
    /// When this method returns, contains the file extension for the image source,
    /// if the path exists; otherwise, the default value for the type of the path parameter.
    /// This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the uri contains an extension; otherwise, <see langword="false" />.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetExtensionFromUri(string uri, [NotNullWhen(true)] out string? extension)
    {
        // Attempts to extract a valid image file extension from the URI.
        // If the path contains a recognized extension, it is used.
        // If the path lacks an extension and a query string is present,
        // the method checks for a valid 'format' parameter as a fallback.
        // Returns true if a supported extension is found in either location.
        extension = null;
        int query = uri.IndexOf('?');
        ReadOnlySpan<char> path;

        if (query > -1)
        {
            path = uri.AsSpan(0, query);

            if (uri.Contains(FormatWebProcessor.Format, StringComparison.OrdinalIgnoreCase)
                && QueryHelpers.ParseQuery(uri[query..]).TryGetValue(FormatWebProcessor.Format, out StringValues ext))
            {
                // We have a query but is it a valid one?
                ReadOnlySpan<char> extSpan = ext[0].AsSpan();
                foreach (string e in this.extensions)
                {
                    if (extSpan.Equals(e, StringComparison.OrdinalIgnoreCase))
                    {
                        // We've found a valid extension in the query.
                        // Now we need to check the path to see if there is a file extension and validate that.
                        extension = e;
                        break;
                    }
                }
            }
        }
        else
        {
            path = uri;
        }

        int extensionIndex;
        if ((extensionIndex = path.LastIndexOf('.')) != -1)
        {
            ReadOnlySpan<char> pathExtension = path[(extensionIndex + 1)..];

            foreach (string e in this.extensions)
            {
                if (pathExtension.Equals(e, StringComparison.OrdinalIgnoreCase))
                {
                    // We've found a valid extension in the path, however we do not
                    // want to overwrite an existing extension.
                    extension ??= e;
                    return true;
                }
            }

            return false;
        }

        return extension != null;
    }

    /// <summary>
    /// Gets the correct extension for the given content type (mime-type).
    /// </summary>
    /// <param name="contentType">The content type (mime-type).</param>
    /// <returns>The <see cref="string" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetExtensionFromContentType(string contentType) => this.extensionsByMimeType[contentType];
}
