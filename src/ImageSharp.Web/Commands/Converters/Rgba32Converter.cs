﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Web.Commands.Converters
{
    /// <summary>
    /// Allows the conversion of strings into rgba32 pixel colors.
    /// </summary>
    internal class Rgba32Converter : CommandConverter
    {
        /// <summary>
        /// The web color hexadecimal regex. Matches strings arranged
        /// in rgb, rgba, rrggbb, or rrggbbaa format to match web syntax.
        /// </summary>
        private static readonly Regex HexColorRegex = new Regex("([0-9a-fA-F]{3}){1,2}", RegexOptions.Compiled);

        /// <summary>
        /// The number color regex.
        /// </summary>
        private static readonly Regex NumberRegex = new Regex(@"\d+", RegexOptions.Compiled);

        /// <summary>
        /// The color constants table map.
        /// </summary>
        private static readonly Lazy<IDictionary<string, Rgba32>> ColorConstantsTable = new Lazy<IDictionary<string, Rgba32>>(InitializeRgba32ConstantsTable);

        /// <inheritdoc/>
        public override object ConvertFrom(CultureInfo culture, string value, Type propertyType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(Rgba32);
            }

            // Special case. HTML requires LightGrey, but NamedColors has LightGray to conform with System.Drawing
            // Check early on.
            if (value.Equals("LightGrey", StringComparison.OrdinalIgnoreCase))
            {
                return Rgba32.LightGray;
            }

            // Numeric r,g,b - r,g,b,a
            char separator = culture.TextInfo.ListSeparator[0];

            if (value.Contains(separator.ToString()))
            {
                string[] components = value.Split(separator);

                bool convert = true;
                foreach (string component in components)
                {
                    if (!NumberRegex.IsMatch(component))
                    {
                        convert = false;
                    }
                }

                if (convert)
                {
                    List<byte> rgba = CommandParser.Instance.ParseValue<List<byte>>(value);

                    return rgba.Count == 4
                        ? new Rgba32(rgba[0], rgba[1], rgba[2], rgba[3])
                        : new Rgba32(rgba[0], rgba[1], rgba[2]);
                }
            }

            // Hex colors rgb, rrggbb, rrggbbaa
            if (HexColorRegex.IsMatch(value))
            {
                return Rgba32.FromHex(value);
            }

            // Named colors
            IDictionary<string, Rgba32> table = ColorConstantsTable.Value;
            return table.ContainsKey(value) ? table[value] : base.ConvertFrom(culture, value, propertyType);
        }

        /// <summary>
        /// Initializes color table mapping color constants.
        /// </summary>
        /// <returns>The <see cref="IDictionary{String, Rgba32}"/>.</returns>
        private static IDictionary<string, Rgba32> InitializeRgba32ConstantsTable()
        {
            IDictionary<string, Rgba32> table = new Dictionary<string, Rgba32>(StringComparer.OrdinalIgnoreCase);
            foreach (FieldInfo field in TypeConstants.Rgba32.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == TypeConstants.Rgba32)
                {
                    table[field.Name] = (Rgba32)field.GetValue(null);
                }
            }

            return table;
        }
    }
}