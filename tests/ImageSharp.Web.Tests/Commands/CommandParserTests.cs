// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Commands
{
    public class CommandParserTests
    {
        public static TheoryData<object, string> IntegralValues = new TheoryData<object, string>
        {
            { (sbyte)1, "1" },
            { (byte)1, "1" },
            { (short)1, "1" },
            { (ushort)1, "1" },
            { 1, "1" },
            { 1U, "1" },
            { 1L, "1" },
            { 1UL, "1" },
            { 1F, "1" },
            { 1D, "1" },
            { 1M, "1" },
        };

        private const double Pi = 3.14159265358979;
        private static readonly string PiString = Pi.ToString(CultureInfo.InvariantCulture);
        private static readonly double RoundedPi = Math.Round(Pi, MidpointRounding.AwayFromZero);

        public static TheoryData<object, string> RealValues = new TheoryData<object, string>
        {
            { (sbyte)RoundedPi, PiString },
            { (byte)RoundedPi, PiString },
            { (short)RoundedPi, PiString },
            { (ushort)RoundedPi, PiString },
            { (int)RoundedPi, PiString },
            { (uint)RoundedPi, PiString },
            { (long)RoundedPi, PiString },
            { (ulong)RoundedPi, PiString },
            { (float)Pi, PiString },
            { (double)Pi, PiString },
            { (decimal)Pi, PiString },
        };

        public static TheoryData<object, string> EnumValues = new TheoryData<object, string>
        {
            { ResizeMode.Max, "max" },
            { ResizeMode.Crop, "this is not an enum value" }, // Unknown returns default
        };

        public static TheoryData<object, string> IntegralArrays = new TheoryData<object, string>
        {
            { new[] { 1, 2, 3, 4 }, "1,2,3,4" },
        };

        public static TheoryData<object, string> RealArrays = new TheoryData<object, string>
        {
            { new[] { 1.667F, 2.667F, 3.667F, 4.667F }, "1.667,2.667,3.667,4.667" },
        };

        public static TheoryData<object, string> IntegralLists = new TheoryData<object, string>
        {
            { new List<int> { 1, 2, 3, 4 }, "1,2,3,4" },
        };

        public static TheoryData<object, string> RealLists = new TheoryData<object, string>
        {
            { new List<float> { 1.667F, 2.667F, 3.667F, 4.667F }, "1.667,2.667,3.667,4.667" },
        };

        public static TheoryData<object, string> Rgba32Values = new TheoryData<object, string>
        {
            { Color.White, "255,255,255" },
            { Color.Transparent, "0,0,0,0" },
            { Color.Orange, "orange" },
            { Color.RoyalBlue, "4169E1FF" },
            { Color.Lime, "00FF00FF" },
            { Color.YellowGreen, "9ACD32FF" },
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
