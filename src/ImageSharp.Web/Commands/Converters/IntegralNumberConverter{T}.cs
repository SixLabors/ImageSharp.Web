// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;

namespace SixLabors.ImageSharp.Web.Commands.Converters;

/// <summary>
/// The generic converter for integral types.
/// </summary>
/// <inheritdoc/>
public sealed class IntegralNumberConverter<T> : ICommandConverter<T>
    where T : struct, IConvertible, IComparable<T>
{
    /// <inheritdoc/>
    public Type Type => typeof(T);

    /// <inheritdoc/>
    public T ConvertFrom(
        CommandParser parser,
        CultureInfo culture,
        string? value,
        Type propertyType)
    {
        if (string.IsNullOrWhiteSpace(value)
            || Array.IndexOf(TypeConstants.IntegralTypes, propertyType) < 0)
        {
            return default;
        }

        // Round the value to the nearest decimal value
        decimal rounded = Math.Round((decimal)Convert.ChangeType(value, typeof(decimal), culture), MidpointRounding.AwayFromZero);

        // Now clamp it to the type ranges
        if (propertyType == TypeConstants.Sbyte)
        {
            rounded = Math.Clamp(rounded, sbyte.MinValue, sbyte.MaxValue);
        }
        else if (propertyType == TypeConstants.Byte)
        {
            rounded = Math.Clamp(rounded, byte.MinValue, byte.MaxValue);
        }
        else if (propertyType == TypeConstants.Short)
        {
            rounded = Math.Clamp(rounded, short.MinValue, short.MaxValue);
        }
        else if (propertyType == TypeConstants.UShort)
        {
            rounded = Math.Clamp(rounded, ushort.MinValue, ushort.MaxValue);
        }
        else if (propertyType == TypeConstants.Int)
        {
            rounded = Math.Clamp(rounded, int.MinValue, int.MaxValue);
        }
        else if (propertyType == TypeConstants.UInt)
        {
            rounded = Math.Clamp(rounded, uint.MinValue, uint.MaxValue);
        }
        else if (propertyType == TypeConstants.Long)
        {
            rounded = Math.Clamp(rounded, long.MinValue, long.MaxValue);
        }
        else if (propertyType == TypeConstants.ULong)
        {
            rounded = Math.Clamp(rounded, ulong.MinValue, ulong.MaxValue);
        }

        // Now it's rounded an clamped we should be able to correctly parse the string.
        return (T)Convert.ChangeType(rounded.ToString(culture), typeof(T), culture);
    }
}
