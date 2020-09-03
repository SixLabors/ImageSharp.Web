// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Globalization;
using System.Text;

namespace SixLabors.ImageSharp.Web.Commands.Converters
{
    /// <summary>
    /// The generic converter for simple types that implement <see cref="IConvertible"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to convert to.</typeparam>
    internal sealed class SimpleCommandConverter<T> : ICommandConverter
        where T : IConvertible
    {
        /// <inheritdoc/>
        public Type Type => typeof(T);

        /// <inheritdoc/>
        public object ConvertFrom(
            CommandParser parser,
            CultureInfo culture,
            string value,
            Type propertyType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(T);
            }

            Type t = typeof(T);
            Type u = Nullable.GetUnderlyingType(t);

            try
            {
                if (u != null)
                {
                    return (T)Convert.ChangeType(value, u, culture);
                }

                return (T)Convert.ChangeType(value, t, culture);
            }
            catch (Exception)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(nameof(culture.Name));
                sb.AppendLine(culture.Name);
                sb.AppendLine(nameof(culture.NumberFormat.NumberDecimalSeparator));
                sb.AppendLine(culture.NumberFormat.NumberDecimalSeparator);
                sb.AppendLine(nameof(culture.TextInfo.ListSeparator));
                sb.AppendLine(culture.TextInfo.ListSeparator);
                sb.AppendLine("Value");
                sb.AppendLine(value);
                throw new Exception(sb.ToString());
            }
        }
    }
}
