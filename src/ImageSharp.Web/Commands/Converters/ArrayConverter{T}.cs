// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SixLabors.ImageSharp.Web.Commands.Converters
{
    /// <summary>
    /// Converts the value of an string to a generic array.
    /// </summary>
    /// <typeparam name="T">The parameter type to convert to.</typeparam>
    internal sealed class ArrayConverter<T> : ListConverter<T>
    {
        /// <inheritdoc/>
        public override Type Type => typeof(T[]);

        /// <inheritdoc/>
        public override object ConvertFrom(
            CommandParser parser,
            CultureInfo culture,
            string value,
            Type propertyType)
            => ((List<T>)base.ConvertFrom(parser, culture, value, propertyType)).ToArray();
    }
}
