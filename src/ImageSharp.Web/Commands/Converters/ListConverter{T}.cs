// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web.Commands.Converters;

/// <summary>
/// Converts the value of a string to a generic list.
/// </summary>
/// <typeparam name="T">The type of result to return.</typeparam>
public sealed class ListConverter<T> : ICommandConverter<List<T>>
{
    /// <inheritdoc/>
    public Type Type => typeof(List<T>);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> ConvertFrom(
        CommandParser parser,
        CultureInfo culture,
        string? value,
        Type propertyType)
    {
        List<T> result = new();
        if (string.IsNullOrWhiteSpace(value))
        {
            return result;
        }

        foreach (string pill in GetStringArray(value, culture))
        {
            T? item = parser.ParseValue<T>(pill, culture);
            if (item is not null)
            {
                result.Add(item);
            }
        }

        return result;
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
