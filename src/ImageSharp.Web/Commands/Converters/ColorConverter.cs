// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SixLabors.ImageSharp.Web.Commands.Converters
{
    /// <summary>
    /// Allows the conversion of strings into rgba32 pixel colors.
    /// </summary>
    internal class ColorConverter : CommandConverter
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
        private static readonly Lazy<IDictionary<string, Color>> ColorConstantsTable = new Lazy<IDictionary<string, Color>>(InitializeColorConstantsTable);

        /// <inheritdoc/>
        public override object ConvertFrom(CultureInfo culture, string value, Type propertyType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(Color);
            }

            // Special case. HTML requires LightGrey, but NamedColors has LightGray to conform with System.Drawing
            // Check early on.
            if (value.Equals("LightGrey", StringComparison.OrdinalIgnoreCase))
            {
                return Color.LightGray;
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
                        ? Color.FromRgba(rgba[0], rgba[1], rgba[2], rgba[3])
                        : Color.FromRgb(rgba[0], rgba[1], rgba[2]);
                }
            }

            // Hex colors rgb, rrggbb, rrggbbaa
            if (HexColorRegex.IsMatch(value))
            {
                return Color.ParseHex(value);
            }

            // Named colors
            IDictionary<string, Color> table = ColorConstantsTable.Value;
            return table.ContainsKey(value) ? table[value] : base.ConvertFrom(culture, value, propertyType);
        }

        /// <summary>
        /// Initializes color table mapping color constants.
        /// </summary>
        /// <returns>The <see cref="IDictionary{String, Color}"/>.</returns>
        private static IDictionary<string, Color> InitializeColorConstantsTable()
        {
            IDictionary<string, Color> table = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
            foreach (FieldInfo field in TypeConstants.Color.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == TypeConstants.Color)
                {
                    table[field.Name] = (Color)field.GetValue(null);
                }
            }

            return table;
        }
    }
}
