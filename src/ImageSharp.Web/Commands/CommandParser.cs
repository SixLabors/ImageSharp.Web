// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Web.Commands.Converters;

namespace SixLabors.ImageSharp.Web.Commands;

/// <summary>
/// Parses URI derived command values into usable commands for processors.
/// </summary>
public sealed class CommandParser
{
    private readonly ICommandConverter[] converters;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandParser"/> class.
    /// </summary>
    /// <param name="converters">The collection of command converters.</param>
    public CommandParser(IEnumerable<ICommandConverter> converters)
    {
        Guard.NotNull(converters, nameof(converters));
        this.converters = converters.ToArray();
    }

    /// <summary>
    /// Parses the given string value converting it to the given type.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
    /// <typeparam name="T">
    /// The <see cref="Type"/> to convert the string to.
    /// </typeparam>
    /// <returns>The converted instance or the default.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ParseValue<T>(string? value, CultureInfo culture)
    {
        DebugGuard.NotNull(culture, nameof(culture));

        Type type = typeof(T);
        ICommandConverter? converter = Array.Find(this.converters, x => x.Type.Equals(type));

        if (converter != null)
        {
            return ((ICommandConverter<T>)converter).ConvertFrom(
                this,
                culture,
                WebUtility.UrlDecode(value),
                type);
        }

        // This special case allows us to reuse the same converter for infinite enum types
        // if one has not already been configured.
        if (type.IsEnum)
        {
            converter = Array.Find(this.converters, x => x.Type.Equals(typeof(Enum)));
            if (converter != null)
            {
                return (T?)((ICommandConverter<object>)converter).ConvertFrom(
                    this,
                    culture,
                    WebUtility.UrlDecode(value),
                    type);
            }
        }

        return ThrowNotSupported<T>(type);
    }

    [DoesNotReturn]
    private static T ThrowNotSupported<T>(Type type)
        => throw new NotSupportedException($"Cannot convert to {type.FullName}.");
}
