// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Web.Commands.Converters;

namespace SixLabors.ImageSharp.Web.Commands
{
    /// <summary>
    /// Parses URI derived command values into usable commands for processors.
    /// </summary>
    public sealed class CommandParser
    {
        private readonly ICommandConverter[] converters;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParser"/> class.
        /// </summary>
        /// <param name="converters">The collection of command converters.</param>
        public CommandParser(IEnumerable<ICommandConverter> converters)
        {
            Guard.NotNull(converters, nameof(converters));
            this.converters = converters.ToArray();
        }

        /// <summary>
        /// Parses the given string value converting it to the given type.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
        /// <typeparam name="T">
        /// The <see cref="Type"/> to convert the string to.
        /// </typeparam>
        /// <returns>The converted instance or the default.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ParseValue<T>(string value, CultureInfo culture)
            => (T)this.ParseValue(typeof(T), value, culture);

        /// <summary>
        /// Parses the given string value converting it to the given type.
        /// </summary>
        /// <param name="type"> The type to convert the string to.</param>
        /// <param name="value">The string value to parse.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
        /// <returns>The converted instance or the default value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ParseValue(Type type, string value, CultureInfo culture)
        {
            DebugGuard.NotNull(type, nameof(type));
            DebugGuard.NotNull(culture, nameof(culture));

            // This allows us to reuse the same converter for infinite enum types.
            Type matchType = type.IsEnum ? typeof(Enum) : type;

            ICommandConverter converter = Array.Find(this.converters, x => x.Type.Equals(matchType));

            if (converter is null)
            {
                ThrowNotSupported(type);
            }

            return converter.ConvertFrom(this, culture, WebUtility.UrlDecode(value), type);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowNotSupported(Type type)
            => throw new NotSupportedException($"Cannot convert to {type.FullName}.");
    }
}
