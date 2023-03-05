// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web.Commands.Converters;

/// <summary>
/// The generic converter for simple types that implement <see cref="IConvertible"/>.
/// </summary>
/// <typeparam name="T">The type of object to convert to.</typeparam>
public sealed class SimpleCommandConverter<T> : ICommandConverter<T>
    where T : IConvertible
{
    /// <inheritdoc/>
    public Type Type => typeof(T);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ConvertFrom(
        CommandParser parser,
        CultureInfo culture,
        string? value,
        Type propertyType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        Type t = typeof(T);
        Type? u = Nullable.GetUnderlyingType(t);

        if (u != null)
        {
            return (T)Convert.ChangeType(value, u, culture);
        }

        return (T)Convert.ChangeType(value, t, culture);
    }
}
