// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Globalization;

namespace SixLabors.ImageSharp.Web.Commands.Converters
{
    /// <summary>
    /// Defines a contract for converting the value of a string into a different data type.
    /// Implementations of this interface should be stateless by design.
    /// </summary>
    public interface ICommandConverter
    {
        /// <summary>
        /// Gets the type of property this converter converts.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Converts the given string to the type of this converter, using the specified culture information.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the converted value.
        /// </returns>
        /// <param name="parser">The command parser use for parting commands.</param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> to use as the current parsing culture.
        /// </param>
        /// <param name="value">The <see cref="string"/> to convert. </param>
        /// <param name="propertyType">The property type that the converter will convert to.</param>
        /// <exception cref="NotSupportedException">The conversion cannot be performed.</exception>
        object ConvertFrom(CommandParser parser, CultureInfo culture, string value, Type propertyType);
    }
}
