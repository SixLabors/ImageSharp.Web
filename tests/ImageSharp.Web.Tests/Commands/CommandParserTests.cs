// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Commands
{
    public class CommandParserTests
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static readonly CultureInfo Dk = new CultureInfo("da-DK");

        private const double Pi = 3.14159265358979;
        private static readonly string PiStringInvariant = Pi.ToString(CultureInfo.InvariantCulture);
        private static readonly string PiStringDanish = Pi.ToString(Dk);
        private static readonly double RoundedPi = Math.Round(Pi, MidpointRounding.AwayFromZero);

        public static TheoryData<object, string, CultureInfo> IntegralValuesInvariant
            = new TheoryData<object, string, CultureInfo>
        {
            { (sbyte)1, "1", Inv },
            { (byte)1, "1", Inv },
            { (short)1, "1", Inv },
            { (ushort)1, "1", Inv },
            { 1, "1", Inv },
            { 1U, "1", Inv },
            { 1L, "1", Inv },
            { 1UL, "1", Inv },
            { 1F, "1", Inv },
            { 1D, "1", Inv },
            { 1M, "1", Inv },
        };

        public static TheoryData<object, string, CultureInfo> IntegralValuesDanish
            = new TheoryData<object, string, CultureInfo>
        {
                    { (sbyte)1, "1", Dk },
                    { (byte)1, "1", Dk },
                    { (short)1, "1", Dk },
                    { (ushort)1, "1", Dk },
                    { 1, "1", Dk },
                    { 1U, "1", Dk },
                    { 1L, "1", Dk },
                    { 1UL, "1", Dk },
                    { 1F, "1", Dk },
                    { 1D, "1", Dk },
                    { 1M, "1", Dk },
        };

        public static TheoryData<object, string, CultureInfo> RealValuesInvariant
            = new TheoryData<object, string, CultureInfo>
        {
            { (sbyte)RoundedPi, PiStringInvariant, Inv },
            { (byte)RoundedPi, PiStringInvariant, Inv },
            { (short)RoundedPi, PiStringInvariant, Inv },
            { (ushort)RoundedPi, PiStringInvariant, Inv },
            { (int)RoundedPi, PiStringInvariant, Inv },
            { (uint)RoundedPi, PiStringInvariant, Inv },
            { (long)RoundedPi, PiStringInvariant, Inv },
            { (ulong)RoundedPi, PiStringInvariant, Inv },
            { (float)Pi, PiStringInvariant, Inv },
            { (double)Pi, PiStringInvariant, Inv },
            { (decimal)Pi, PiStringInvariant, Inv },
        };

        public static TheoryData<object, string, CultureInfo> RealValuesDanish
            = new TheoryData<object, string, CultureInfo>
        {
            { (sbyte)RoundedPi, PiStringDanish, Dk },
            { (byte)RoundedPi, PiStringDanish, Dk },
            { (short)RoundedPi, PiStringDanish, Dk },
            { (ushort)RoundedPi, PiStringDanish, Dk },
            { (int)RoundedPi, PiStringDanish, Dk },
            { (uint)RoundedPi, PiStringDanish, Dk },
            { (long)RoundedPi, PiStringDanish, Dk },
            { (ulong)RoundedPi, PiStringDanish, Dk },
            { (float)Pi, PiStringDanish, Dk },
            { (double)Pi, PiStringDanish, Dk },
            { (decimal)Pi, PiStringDanish, Dk },
        };

        public static TheoryData<ResizeMode, string, CultureInfo> EnumValues
            = new TheoryData<ResizeMode, string, CultureInfo>
        {
            { ResizeMode.Max, "max", Inv },
            { ResizeMode.Crop, "this is not, an enum value", Inv }, // Unknown returns default
        };

        public static TheoryData<int[], string, CultureInfo> IntegralArrays
            = new TheoryData<int[], string, CultureInfo>
        {
            { new[] { 1, 2, 3, 4 }, ToNumericList(Inv, 1, 2, 3, 4), Inv },
        };

        public static TheoryData<float[], string, CultureInfo> RealArraysInvariant
            = new TheoryData<float[], string, CultureInfo>
        {
            { new[] { 1.667F, 2.667F, 3.667F, 4.667F }, ToNumericList(Inv, 1.667F, 2.667F, 3.667F, 4.667F), Inv },
        };

        public static TheoryData<float[], string, CultureInfo> RealArraysDanish
            = new TheoryData<float[], string, CultureInfo>
        {
            { new[] { 1.667F, 2.667F, 3.667F, 4.667F }, ToNumericList(Dk, 1.667F, 2.667F, 3.667F, 4.667F), Dk },
        };

        public static TheoryData<object, string, CultureInfo> IntegralLists
            = new TheoryData<object, string, CultureInfo>
        {
            { new List<int> { 1, 2, 3, 4 }, ToNumericList(Inv, 1, 2, 3, 4), Inv },
        };

        public static TheoryData<List<float>, string, CultureInfo> RealLists
            = new TheoryData<List<float>, string, CultureInfo>
        {
            { new List<float> { 1.667F, 2.667F, 3.667F, 4.667F }, "1.667,2.667,3.667,4.667", Inv },
        };

        public static TheoryData<Color, string, CultureInfo> ColorValuesInvariant
            = new TheoryData<Color, string, CultureInfo>
        {
            { default, string.Empty, Inv },
            { Color.White, ToNumericList<byte>(Inv, 255, 255, 255), Inv },
            { Color.Transparent, ToNumericList<byte>(Inv, 0, 0, 0, 0),  Inv },
            { Color.Orange, "orange", Inv },
            { Color.RoyalBlue, "4169E1FF", Inv },
            { Color.Lime, "00FF00FF", Inv },
            { Color.YellowGreen, "9ACD32FF", Inv },
        };

        public static TheoryData<Color, string, CultureInfo> ColorValuesDanish
            = new TheoryData<Color, string, CultureInfo>
        {
            { default, string.Empty, Dk },
            { Color.White, ToNumericList<byte>(Dk, 255, 255, 255), Dk },
            { Color.Transparent, ToNumericList<byte>(Dk, 0, 0, 0, 0), Dk },
            { Color.Orange, "orange", Dk },
            { Color.RoyalBlue, "4169E1FF", Dk },
            { Color.Lime, "00FF00FF", Dk },
            { Color.YellowGreen, "9ACD32FF", Dk },
        };

        private static readonly CommandParser Parser = GetCommandParser();

        [Theory]
        [MemberData(nameof(IntegralValuesInvariant))]
        [MemberData(nameof(IntegralValuesDanish))]
        [MemberData(nameof(RealValuesInvariant))]
        [MemberData(nameof(RealValuesDanish))]
        [MemberData(nameof(EnumValues))]
        [MemberData(nameof(IntegralArrays))]
        [MemberData(nameof(RealArraysInvariant))]
        [MemberData(nameof(RealArraysDanish))]
        [MemberData(nameof(IntegralLists))]
        [MemberData(nameof(RealLists))]
        [MemberData(nameof(ColorValuesInvariant))]
        [MemberData(nameof(ColorValuesDanish))]
        public void CommandParserCanConvert<T>(T expected, string param, CultureInfo culture)
        {
            T sb = Parser.ParseValue<T>(param, culture);
            Assert.IsType<T>(sb);
            Assert.Equal(expected, sb);
        }

        [Fact]
        public void CommandParseThrowsCorrectly()
        {
            var emptyParser = new CommandParser(Array.Empty<ICommandConverter>());

            Assert.Throws<NotSupportedException>(
                () => emptyParser.ParseValue<bool>("true", CultureInfo.InvariantCulture));
        }

        private static CommandParser GetCommandParser()
        {
            var commands = new List<ICommandConverter>
            {
                new IntegralNumberConverter<sbyte>(),
                new IntegralNumberConverter<byte>(),
                new IntegralNumberConverter<short>(),
                new IntegralNumberConverter<ushort>(),
                new IntegralNumberConverter<int>(),
                new IntegralNumberConverter<uint>(),
                new IntegralNumberConverter<long>(),
                new IntegralNumberConverter<ulong>(),

                new SimpleCommandConverter<decimal>(),
                new SimpleCommandConverter<float>(),
                new SimpleCommandConverter<double>(),
                new SimpleCommandConverter<string>(),
                new SimpleCommandConverter<bool>(),

                new ArrayConverter<sbyte>(),
                new ArrayConverter<byte>(),
                new ArrayConverter<short>(),
                new ArrayConverter<ushort>(),
                new ArrayConverter<int>(),
                new ArrayConverter<uint>(),
                new ArrayConverter<long>(),
                new ArrayConverter<ulong>(),
                new ArrayConverter<decimal>(),
                new ArrayConverter<float>(),
                new ArrayConverter<double>(),
                new ArrayConverter<string>(),
                new ArrayConverter<bool>(),

                new ListConverter<sbyte>(),
                new ListConverter<byte>(),
                new ListConverter<short>(),
                new ListConverter<ushort>(),
                new ListConverter<int>(),
                new ListConverter<uint>(),
                new ListConverter<long>(),
                new ListConverter<ulong>(),
                new ListConverter<decimal>(),
                new ListConverter<float>(),
                new ListConverter<double>(),
                new ListConverter<string>(),
                new ListConverter<bool>(),

                new ColorConverter(),
                new EnumConverter()
            };

            return new CommandParser(commands);
        }

        private static string ToNumericList<T>(CultureInfo culture, params T[] values)
            where T : IConvertible
        {
            var sb = new StringBuilder();
            var ls = culture.TextInfo.ListSeparator[0];

            for (int i = 0; i < values.Length; i++)
            {
                sb.AppendFormat(values[i].ToString(culture) + ls);
            }

            return sb.ToString().TrimEnd(ls);
        }
    }
}
