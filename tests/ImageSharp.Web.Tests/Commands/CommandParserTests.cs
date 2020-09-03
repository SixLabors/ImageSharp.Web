// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Commands
{
    public class CommandParserTests
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private static readonly CultureInfo Danish = new CultureInfo("da-DK");
        private const double Pi = 3.14159265358979;
        private static readonly string PiStringInvariant = Pi.ToString(CultureInfo.InvariantCulture);
        private static readonly string PiStringDanish = Pi.ToString(Danish);
        private static readonly double RoundedPi = Math.Round(Pi, MidpointRounding.AwayFromZero);

        public static TheoryData<object, string, CultureInfo> IntegralValuesInvariant
            = new TheoryData<object, string, CultureInfo>
        {
            { (sbyte)1, "1", Invariant },
            { (byte)1, "1", Invariant },
            { (short)1, "1", Invariant },
            { (ushort)1, "1", Invariant },
            { 1, "1", Invariant },
            { 1U, "1", Invariant },
            { 1L, "1", Invariant },
            { 1UL, "1", Invariant },
            { 1F, "1", Invariant },
            { 1D, "1", Invariant },
            { 1M, "1", Invariant },
        };

        public static TheoryData<object, string, CultureInfo> IntegralValuesDanish
            = new TheoryData<object, string, CultureInfo>
        {
                    { (sbyte)1, "1", Danish },
                    { (byte)1, "1", Danish },
                    { (short)1, "1", Danish },
                    { (ushort)1, "1", Danish },
                    { 1, "1", Danish },
                    { 1U, "1", Danish },
                    { 1L, "1", Danish },
                    { 1UL, "1", Danish },
                    { 1F, "1", Danish },
                    { 1D, "1", Danish },
                    { 1M, "1", Danish },
        };

        public static TheoryData<object, string, CultureInfo> RealValuesInvariant
            = new TheoryData<object, string, CultureInfo>
        {
            { (sbyte)RoundedPi, PiStringInvariant, Invariant },
            { (byte)RoundedPi, PiStringInvariant, Invariant },
            { (short)RoundedPi, PiStringInvariant, Invariant },
            { (ushort)RoundedPi, PiStringInvariant, Invariant },
            { (int)RoundedPi, PiStringInvariant, Invariant },
            { (uint)RoundedPi, PiStringInvariant, Invariant },
            { (long)RoundedPi, PiStringInvariant, Invariant },
            { (ulong)RoundedPi, PiStringInvariant, Invariant },
            { (float)Pi, PiStringInvariant, Invariant },
            { (double)Pi, PiStringInvariant, Invariant },
            { (decimal)Pi, PiStringInvariant, Invariant },
        };

        public static TheoryData<object, string, CultureInfo> RealValuesDanish
            = new TheoryData<object, string, CultureInfo>
        {
            { (sbyte)RoundedPi, PiStringDanish, Danish },
            { (byte)RoundedPi, PiStringDanish, Danish },
            { (short)RoundedPi, PiStringDanish, Danish },
            { (ushort)RoundedPi, PiStringDanish, Danish },
            { (int)RoundedPi, PiStringDanish, Danish },
            { (uint)RoundedPi, PiStringDanish, Danish },
            { (long)RoundedPi, PiStringDanish, Danish },
            { (ulong)RoundedPi, PiStringDanish, Danish },
            { (float)Pi, PiStringDanish, Danish },
            { (double)Pi, PiStringDanish, Danish },
            { (decimal)Pi, PiStringDanish, Danish },
        };

        public static TheoryData<ResizeMode, string, CultureInfo> EnumValues
            = new TheoryData<ResizeMode, string, CultureInfo>
        {
            { ResizeMode.Max, "max", Invariant },
            { ResizeMode.Crop, "this is not, an enum value", Invariant }, // Unknown returns default
        };

        public static TheoryData<int[], string, CultureInfo> IntegralArrays
            = new TheoryData<int[], string, CultureInfo>
        {
            { new[] { 1, 2, 3, 4 }, "1,2,3,4", Invariant },
        };

        public static TheoryData<float[], string, CultureInfo> RealArraysInvariant
            = new TheoryData<float[], string, CultureInfo>
        {
            { new[] { 1.667F, 2.667F, 3.667F, 4.667F }, "1.667,2.667,3.667,4.667", Invariant },
        };

        public static TheoryData<float[], string, CultureInfo> RealArraysDanish
            = new TheoryData<float[], string, CultureInfo>
        {
            { new[] { 1.667F, 2.667F, 3.667F, 4.667F }, "1,667;2,667;3,667;4,667", Danish },
        };

        public static TheoryData<object, string, CultureInfo> IntegralLists
            = new TheoryData<object, string, CultureInfo>
        {
            { new List<int> { 1, 2, 3, 4 }, "1,2,3,4", Invariant },
        };

        public static TheoryData<List<float>, string, CultureInfo> RealLists
            = new TheoryData<List<float>, string, CultureInfo>
        {
            { new List<float> { 1.667F, 2.667F, 3.667F, 4.667F }, "1.667,2.667,3.667,4.667", Invariant },
        };

        public static TheoryData<Color, string, CultureInfo> ColorValuesInvariant
            = new TheoryData<Color, string, CultureInfo>
        {
            { default, string.Empty, Invariant },
            { Color.White, "255,255,255", Invariant },
            { Color.Transparent, "0,0,0,0", Invariant },
            { Color.Orange, "orange", Invariant },
            { Color.RoyalBlue, "4169E1FF", Invariant },
            { Color.Lime, "00FF00FF", Invariant },
            { Color.YellowGreen, "9ACD32FF", Invariant },
        };

        public static TheoryData<Color, string, CultureInfo> ColorValuesDanish
            = new TheoryData<Color, string, CultureInfo>
        {
            { default, string.Empty, Danish },
            { Color.White, "255;255;255", Danish },
            { Color.Transparent, "0;0;0;0", Danish },
            { Color.Orange, "orange", Danish },
            { Color.RoyalBlue, "4169E1FF", Danish },
            { Color.Lime, "00FF00FF", Danish },
            { Color.YellowGreen, "9ACD32FF", Danish },
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
    }
}
