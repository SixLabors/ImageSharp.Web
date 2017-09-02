// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Globalization;

namespace SixLabors.ImageSharp.Web.Commands.Converters
{
    /// <summary>
    /// The generic converter for simple types that implement <see cref="IConvertible"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to convert to.</typeparam>
    internal sealed class SimpleCommandConverter<T> : CommandConverter
        where T : IConvertible
    {
        /// <inheritdoc/>
        public override object ConvertFrom(CultureInfo culture, string value, Type propertyType)
        {
            if (value == null)
            {
                return base.ConvertFrom(culture, null, propertyType);
            }

            Type t = typeof(T);
            Type u = Nullable.GetUnderlyingType(t);

            if (u != null)
            {
                return (T)Convert.ChangeType(value, u);
            }

            return (T)Convert.ChangeType(value, t, culture);
        }
    }
}