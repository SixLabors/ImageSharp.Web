// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Web.Commands.Converters
{
    /// <summary>
    /// Converts the value of a string to a generic array.
    /// </summary>
    /// <typeparam name="T">The parameter type to convert to.</typeparam>
    internal sealed class ArrayConverter<T> : ICommandConverter<T[]>
    {
        /// <inheritdoc/>
        public Type Type => typeof(T[]);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ConvertFrom(
            CommandParser parser,
            CultureInfo culture,
            string value,
            Type propertyType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<T>();
            }

            var result = new List<T>();
            foreach (string pill in GetStringArray(value, culture))
            {
                T item = parser.ParseValue<T>(pill, culture);
                if (item != null)
                {
                    result.Add(item);
                }
            }

            return result.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string[] GetStringArray(string input, CultureInfo culture)
        {
            char separator = culture.TextInfo.ListSeparator[0];
            return input.Split(separator).Select(s => s.Trim()).ToArray();
        }
    }
}
