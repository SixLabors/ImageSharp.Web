// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Represents the metadata associated with an image file.
/// </summary>
public readonly struct ImageCacheMetadata : IEquatable<ImageCacheMetadata>
{
    private const string ContentTypeKey = "CT";
    private const string SourceLastModifiedKey = "SM";
    private const string CacheLastModifiedKey = "LM";
    private const string CacheControlKey = "MA";
    private const string ContentLengthKey = "CL";

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageCacheMetadata"/> struct.
    /// </summary>
    /// <param name="sourceLastWriteTimeUtc">The date and time in coordinated universal time (UTC) since the source file was last modified.</param>
    /// <param name="cacheLastWriteTimeUtc">The date and time in coordinated universal time (UTC) since the cache file was last modified.</param>
    /// <param name="contentType">The content type for the source file.</param>
    /// <param name="cacheControlMaxAge">The maximum amount of time a resource will be considered fresh.</param>
    /// <param name="contentLength">The length of the image in bytes.</param>
    public ImageCacheMetadata(
        DateTime sourceLastWriteTimeUtc,
        DateTime cacheLastWriteTimeUtc,
        string contentType,
        TimeSpan cacheControlMaxAge,
        long contentLength)
    {
        this.SourceLastWriteTimeUtc = sourceLastWriteTimeUtc;
        this.CacheLastWriteTimeUtc = cacheLastWriteTimeUtc;
        this.ContentType = contentType;
        this.CacheControlMaxAge = cacheControlMaxAge;
        this.ContentLength = contentLength;
    }

    /// <summary>
    /// Gets the date and time in coordinated universal time (UTC) since the source file was last modified.
    /// </summary>
    public DateTime SourceLastWriteTimeUtc { get; }

    /// <summary>
    /// Gets the date and time in coordinated universal time (UTC) since the cached file was last modified.
    /// </summary>
    public DateTime CacheLastWriteTimeUtc { get; }

    /// <summary>
    /// Gets the content type of the source file.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the maximum amount of time a resource will be considered fresh.
    /// </summary>
    public TimeSpan CacheControlMaxAge { get; }

    /// <summary>
    /// Gets the length of the image in bytes.
    /// </summary>
    public long ContentLength { get; }

    /// <summary>
    /// Compares two <see cref="ImageCacheMetadata"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="ImageCacheMetadata"/> on the left side of the operand.</param>
    /// <param name="right">The <see cref="ImageCacheMetadata"/> on the right side of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in ImageCacheMetadata left, in ImageCacheMetadata right)
        => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="ImageCacheMetadata"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="ImageCacheMetadata"/> on the left side of the operand.</param>
    /// <param name="right">The <see cref="ImageCacheMetadata"/> on the right side of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in ImageCacheMetadata left, in ImageCacheMetadata right)
        => !left.Equals(right);

    /// <summary>
    /// Returns a new <see cref="ImageCacheMetadata"/> parsed from the given dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to parse and return the metadata from.</param>
    /// <returns>The <see cref="ImageCacheMetadata"/>.</returns>
    public static ImageCacheMetadata FromDictionary(IDictionary<string, string> dictionary)
    {
        // DateTime.TryParse(null) ==  DateTime.MinValue so no need for conditional;
        dictionary.TryGetValue(SourceLastModifiedKey, out string? sourceLastWriteTimeUtcString);
        DateTime.TryParse(sourceLastWriteTimeUtcString, null, DateTimeStyles.RoundtripKind, out DateTime sourceLastWriteTimeUtc);

        dictionary.TryGetValue(CacheLastModifiedKey, out string? cacheLastWriteTimeUtcString);
        DateTime.TryParse(cacheLastWriteTimeUtcString, null, DateTimeStyles.RoundtripKind, out DateTime cacheLastWriteTimeUtc);

        dictionary.TryGetValue(ContentTypeKey, out string? contentType);
        Guard.NotNull(contentType);

        // int.TryParse(null) == 0 and we want to return TimeSpan.MinValue not TimeSpan.Zero
        TimeSpan cacheControlMaxAge = TimeSpan.MinValue;
        if (dictionary.TryGetValue(CacheControlKey, out string? cacheControlMaxAgeString))
        {
            _ = int.TryParse(cacheControlMaxAgeString, out int maxAge);
            cacheControlMaxAge = TimeSpan.FromSeconds(maxAge);
        }

        dictionary.TryGetValue(ContentLengthKey, out string? contentLengthString);
        _ = long.TryParse(contentLengthString, out long contentLength);

        return new ImageCacheMetadata(
            sourceLastWriteTimeUtc,
            cacheLastWriteTimeUtc,
            contentType,
            cacheControlMaxAge,
            contentLength);
    }

    /// <summary>
    /// Asynchronously reads and returns an <see cref="ImageCacheMetadata"/> from the input stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <returns>The <see cref="ImageCacheMetadata"/>.</returns>
    public static async Task<ImageCacheMetadata> ReadAsync(Stream stream)
    {
        Dictionary<string, string> dictionary = new();
        using (StreamReader reader = new(stream, Encoding.UTF8))
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                int idx = line.IndexOf(':');
                if (idx > 0)
                {
                    string key = line[..idx];
                    dictionary[key] = line[(idx + 1)..];
                }
            }
        }

        return FromDictionary(dictionary);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ImageCacheMetadata other)
        => this.SourceLastWriteTimeUtc == other.SourceLastWriteTimeUtc
        && this.CacheLastWriteTimeUtc == other.CacheLastWriteTimeUtc
        && this.ContentType == other.ContentType
        && this.CacheControlMaxAge == other.CacheControlMaxAge
        && this.ContentLength == other.ContentLength;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is ImageCacheMetadata data && this.Equals(data);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
            this.SourceLastWriteTimeUtc,
            this.CacheLastWriteTimeUtc,
            this.ContentType,
            this.CacheControlMaxAge,
            this.ContentLength);

    /// <summary>
    /// Returns a new <see cref="Dictionary{String, String}"/> representing the current instance.
    /// </summary>
    /// <returns>The <see cref="Dictionary{String, String}"/>.</returns>
    public Dictionary<string, string> ToDictionary()
        => new()
        {
            { SourceLastModifiedKey, this.SourceLastWriteTimeUtc.ToString("o") },
            { CacheLastModifiedKey, this.CacheLastWriteTimeUtc.ToString("o") },
            { ContentTypeKey, this.ContentType },
            { CacheControlKey, this.CacheControlMaxAge.TotalSeconds.ToString(NumberFormatInfo.InvariantInfo) },
            { ContentLengthKey, this.ContentLength.ToString(NumberFormatInfo.InvariantInfo) }
        };

    /// <inheritdoc/>
    public override string ToString()
        => FormattableString.Invariant(
            $"ImageCacheMetaData({this.SourceLastWriteTimeUtc}, {this.CacheLastWriteTimeUtc}, {this.ContentType}, {this.CacheControlMaxAge}, {this.ContentLength})");

    /// <summary>
    /// Asynchronously writes the metadata to the target stream.
    /// </summary>
    /// <param name="stream">The target stream.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task WriteAsync(Stream stream)
    {
        Dictionary<string, string> dictionary = this.ToDictionary();

        await using StreamWriter writer = new(stream, Encoding.UTF8);
        foreach (KeyValuePair<string, string> keyValuePair in dictionary)
        {
            // TODO: string.Create
            await writer.WriteLineAsync($"{keyValuePair.Key}:{keyValuePair.Value}");
        }

        await writer.FlushAsync();
    }
}
