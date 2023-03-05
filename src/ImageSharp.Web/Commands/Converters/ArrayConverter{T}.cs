// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web.Commands.Converters;

/// <summary>
/// Converts the value of a string to a generic array.
/// </summary>
/// <typeparam name="T">The parameter type to convert to.</typeparam>
public sealed class ArrayConverter<T> : ICommandConverter<T[]>
{
    /// <inheritdoc/>
    public Type Type => typeof(T[]);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ConvertFrom(
        CommandParser parser,
        CultureInfo culture,
        string? value,
        Type propertyType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<T>();
        }

        List<T> result = new();
        foreach (string pill in GetStringArray(value, culture))
        {
            T? item = parser.ParseValue<T>(pill, culture);
            if (item is not null)
            {
                result.Add(item);
            }
        }

        return result.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string[] GetStringArray(string input, CultureInfo culture)
    {
        char separator = ConverterUtility.GetListSeparator(culture);

        // TODO: Can we use StringSplit Enumerator here?
        // https://github.com/dotnet/runtime/issues/934
        return input.Split(separator).Select(s => s.Trim()).ToArray();
    }
}
