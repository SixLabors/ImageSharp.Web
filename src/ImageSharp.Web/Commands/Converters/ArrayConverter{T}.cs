// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SixLabors.ImageSharp.Web.Commands.Converters
{
    /// <summary>
    /// Converts the value of an string to a generic array.
    /// </summary>
    /// <typeparam name="T">The parameter type to convert to.</typeparam>
    internal sealed class ArrayConverter<T> : ListConverter<T>
    {
        /// <inheritdoc/>
        public override object ConvertFrom(CultureInfo culture, string value, Type propertyType)
        {
            object result = base.ConvertFrom(culture, value, propertyType);

            var list = result as IList<T>;
            return list?.ToArray() ?? result;
        }
    }
}