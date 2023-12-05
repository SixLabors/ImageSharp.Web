// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web.Commands.Converters;

/// <summary>
/// The enum converter. Allows conversion to enumerations.
/// </summary>
public sealed class EnumConverter : ICommandConverter<object>
{
    /// <inheritdoc/>
    public Type Type => typeof(Enum);

    /// <inheritdoc/>
    /// <remarks>
    /// Unlike other converters the <see cref="Type"/> property does not
    /// match the <paramref name="propertyType"/> value.
    /// This allows us to reuse the same converter for infinite enum types.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public object? ConvertFrom(
        CommandParser parser,
        CultureInfo culture,
        string? value,
        Type propertyType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Activator.CreateInstance(propertyType);
        }

        try
        {
            char separator = ConverterUtility.GetListSeparator(culture);
            if (value.Contains(separator))
            {
                long convertedValue = 0;
                foreach (string pill in GetStringArray(value, separator))
                {
                    convertedValue |= Convert.ToInt64((Enum)Enum.Parse(propertyType, pill, true), culture);
                }

                return Enum.ToObject(propertyType, convertedValue);
            }

            return Enum.Parse(propertyType, value, true);
        }
        catch
        {
            // Just return the default value
            return Activator.CreateInstance(propertyType);
        }
    }

    /// <summary>
    /// Splits a string by separator to return an array of string values.
    /// </summary>
    /// <param name="input">The input string to split.</param>
    /// <param name="separator">The separator to split string by.</param>
    /// <returns>The <see cref="T:String[]"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string[] GetStringArray(string input, char separator)

        // TODO: Can we use StringSplit Enumerator here?
        // https://github.com/dotnet/runtime/issues/934
        => input.Split(separator).Select(s => s.Trim()).ToArray();
}
