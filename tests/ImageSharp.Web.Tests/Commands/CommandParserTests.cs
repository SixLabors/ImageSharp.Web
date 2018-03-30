// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.ImageSharp.Web.Commands;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Commands
{
    public class CommandParserTests
    {
        public static TheoryData<object, string> IntegralValues = new TheoryData<object, string> {
            { (sbyte)1, "1" },
            { (byte)1, "1" },
            { (short)1, "1" },
            { (ushort)1, "1" },
            { (int)1, "1" },
            { (uint)1, "1" },
            { (long)1, "1" },
            { (ulong)1, "1" },
            { (float)1, "1" },
            { (double)1, "1" },
            { (decimal)1, "1" },
        };

        const double Pi = 3.14159265358979;
        static readonly string PiString = Pi.ToString(CultureInfo.InvariantCulture);
        static readonly double RoundedPi = Math.Round(Pi, MidpointRounding.AwayFromZero);

        public static TheoryData<object, string> RealValues = new TheoryData<object, string> {
            { (sbyte)RoundedPi, PiString },
            { (byte)RoundedPi, PiString},
            { (short)RoundedPi, PiString },
            { (ushort)RoundedPi, PiString },
            { (int)RoundedPi, PiString },
            { (uint)RoundedPi, PiString },
            { (long)RoundedPi, PiString },
            { (ulong)RoundedPi, PiString },
            { (float)Pi, PiString },
            { (double)Pi, PiString},
            { (decimal)Pi, PiString},
        };

        public static TheoryData<object, string> EnumValues = new TheoryData<object, string> {
            { ResizeMode.Max, "max" },
            { ResizeMode.Crop, "this is not an enum value" }, // Unknown returns default
        };

        public static TheoryData<object, string> IntegralArrays = new TheoryData<object, string> {
            { new[] { 1, 2, 3, 4 }, "1,2,3,4" },
        };

        public static TheoryData<object, string> RealArrays = new TheoryData<object, string> {
            { new[] { 1.667F, 2.667F, 3.667F, 4.667F }, "1.667,2.667,3.667,4.667" },
        };

        public static TheoryData<object, string> IntegralLists = new TheoryData<object, string> {
            { new List<int> { 1, 2, 3, 4 }, "1,2,3,4" },
        };

        public static TheoryData<object, string> RealLists = new TheoryData<object, string> {
            { new List<float> { 1.667F, 2.667F, 3.667F, 4.667F }, "1.667,2.667,3.667,4.667" },
        };

        public static TheoryData<object, string> Rgba32Values = new TheoryData<object, string> {
            {Rgba32.White , "255,255,255" },
            {Rgba32.Transparent , "255,255,255,0" }, // ??is this right??
            {Rgba32.Orange , "orange" },
            {Rgba32.Lime , "00FF00" },
        };

        [Theory]
        [MemberData(nameof(IntegralValues))]
        [MemberData(nameof(RealValues))]
        [MemberData(nameof(EnumValues))]
        [MemberData(nameof(IntegralArrays))]
        [MemberData(nameof(RealArrays))]
        [MemberData(nameof(IntegralLists))]
        [MemberData(nameof(RealLists))]
        [MemberData(nameof(Rgba32Values))]
        public void CommandParses<T>(T expected, string param)
        {
            T sb = CommandParser.Instance.ParseValue<T>(param);
            Assert.IsType<T>(sb);
            Assert.Equal(expected, sb);
        }
    }
}