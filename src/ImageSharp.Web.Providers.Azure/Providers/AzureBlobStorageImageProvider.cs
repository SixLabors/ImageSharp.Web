// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Resolvers;
using SixLabors.ImageSharp.Web.Resolvers.Azure;

namespace SixLabors.ImageSharp.Web.Providers.Azure;

/// <summary>
/// Returns images stored in Azure Blob Storage.
/// </summary>
public class AzureBlobStorageImageProvider : IImageProvider
{
    /// <summary>
    /// Character array to remove from paths.
    /// </summary>
    private static readonly char[] SlashChars = { '\\', '/' };

    /// <summary>
    /// The containers for the blob services.
    /// </summary>
    private readonly Dictionary<string, BlobContainerClient> containers
        = new();

    /// <summary>
    /// Contains various helper methods based on the current configuration.
    /// </summary>
    private readonly FormatUtilities formatUtilities;

    /// <summary>
    /// A match function used by the resolver to identify itself as the correct resolver to use.
    /// </summary>
    private Func<HttpContext, bool>? match;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobStorageImageProvider"/> class.
    /// </summary>
    /// <param name="storageOptions">The blob storage options.</param>
    /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
    /// <param name="serviceProvider">The current service provider</param>
    public AzureBlobStorageImageProvider(
        IOptions<AzureBlobStorageImageProviderOptions> storageOptions,
        FormatUtilities formatUtilities,
        IServiceProvider serviceProvider)
    {
        Guard.NotNull(storageOptions, nameof(storageOptions));

        this.formatUtilities = formatUtilities;

        foreach (AzureBlobContainerClientOptions container in storageOptions.Value.BlobContainers)
        {
            BlobContainerClient client =
                container.BlobContainerClientFactory?.Invoke(container, serviceProvider)
                ?? new BlobContainerClient(container.ConnectionString, container.ContainerName);

            this.containers.Add(client.Name, client);
        }
    }

    /// <inheritdoc/>
    public ProcessingBehavior ProcessingBehavior { get; } = ProcessingBehavior.All;

    /// <inheritdoc/>
    public Func<HttpContext, bool> Match
    {
        get => this.match ?? this.IsMatch;
        set => this.match = value;
    }

    /// <inheritdoc/>
    public async Task<IImageResolver?> GetAsync(HttpContext context)
    {
        // Strip the leading slash and container name from the HTTP request path and treat
        // the remaining path string as the blob name.
        // Path has already been correctly parsed before here.
        string containerName = string.Empty;
        BlobContainerClient? container = null;

        // We want an exact match here to ensure that container names starting with
        // the same prefix are not mixed up.
        string? path = context.Request.Path.Value?.TrimStart(SlashChars);

        if (path is null)
        {
            return null;
        }

        int index = path.IndexOfAny(SlashChars);
        string nameToMatch = index != -1 ? path[..index] : path;

        foreach (string key in this.containers.Keys)
        {
            if (nameToMatch.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                containerName = key;
                container = this.containers[key];
                break;
            }
        }

        // Something has gone horribly wrong for this to happen but check anyway.
        if (container is null)
        {
            return null;
        }

        // Blob name should be the remaining path string.
        string blobName = path[containerName.Length..].TrimStart(SlashChars);

        if (string.IsNullOrWhiteSpace(blobName))
        {
            return null;
        }

        BlobClient blob = container.GetBlobClient(blobName);

        if (!await blob.ExistsAsync())
        {
            return null;
        }

        return new AzureBlobStorageImageResolver(blob);
    }

    /// <inheritdoc/>
    public bool IsValidRequest(HttpContext context)
        => this.formatUtilities.TryGetExtensionFromUri(context.Request.GetDisplayUrl(), out _);

    private bool IsMatch(HttpContext context)
    {
        // Only match loosely here for performance.
        // Path matching conflicts should be dealt with by configuration.
        string? path = context.Request.Path.Value?.TrimStart(SlashChars);

        if (path is null)
        {
            return false;
        }

        foreach (string container in this.containers.Keys)
        {
            if (path.StartsWith(container, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
