// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Text;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;

namespace SixLabors.ImageSharp.Web.Tests.Commands;

public class CommandParserTests
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly CultureInfo Dk = new CultureInfo("da-DK");

    private const double Pi = 3.14159265358979;
    private static readonly string PiStringInv = Pi.ToString(CultureInfo.InvariantCulture);
    private static readonly string PiStringDk = Pi.ToString(Dk);
    private static readonly double RoundedPi = Math.Round(Pi, MidpointRounding.AwayFromZero);

    public static TheoryData<object, string, CultureInfo> IntegralValuesInv { get; }
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

    public static TheoryData<object, string, CultureInfo> IntegralValuesDk { get; }
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

    public static TheoryData<object, string, CultureInfo> RealValuesInv { get; }
        = new TheoryData<object, string, CultureInfo>
    {
        { (sbyte)RoundedPi, PiStringInv, Inv },
        { (byte)RoundedPi, PiStringInv, Inv },
        { (short)RoundedPi, PiStringInv, Inv },
        { (ushort)RoundedPi, PiStringInv, Inv },
        { (int)RoundedPi, PiStringInv, Inv },
        { (uint)RoundedPi, PiStringInv, Inv },
        { (long)RoundedPi, PiStringInv, Inv },
        { (ulong)RoundedPi, PiStringInv, Inv },
        { (float)Pi, PiStringInv, Inv },
        { (double)Pi, PiStringInv, Inv },
        { (decimal)Pi, PiStringInv, Inv },
    };

    public static TheoryData<object, string, CultureInfo> RealValuesDanish { get; }
        = new TheoryData<object, string, CultureInfo>
    {
        { (sbyte)RoundedPi, PiStringDk, Dk },
        { (byte)RoundedPi, PiStringDk, Dk },
        { (short)RoundedPi, PiStringDk, Dk },
        { (ushort)RoundedPi, PiStringDk, Dk },
        { (int)RoundedPi, PiStringDk, Dk },
        { (uint)RoundedPi, PiStringDk, Dk },
        { (long)RoundedPi, PiStringDk, Dk },
        { (ulong)RoundedPi, PiStringDk, Dk },
        { (float)Pi, PiStringDk, Dk },
        { (double)Pi, PiStringDk, Dk },
        { (decimal)Pi, PiStringDk, Dk },
    };

    public static TheoryData<ResizeMode, string, CultureInfo> EnumValues { get; }
        = new TheoryData<ResizeMode, string, CultureInfo>
    {
        { ResizeMode.Max, "max", Inv },
        { ResizeMode.Crop, "this is not, an enum value", Inv }, // Unknown returns default
    };

    public static TheoryData<int[], string, CultureInfo> IntegralArrays { get; }
        = new TheoryData<int[], string, CultureInfo>
    {
        { new[] { 1, 2, 3, 4 }, ToNumericList(Inv, 1, 2, 3, 4), Inv },
    };

    public static TheoryData<float[], string, CultureInfo> RealArraysInv { get; }
        = new TheoryData<float[], string, CultureInfo>
    {
        { new[] { 1.667F, 2.667F, 3.667F, 4.667F }, ToNumericList(Inv, 1.667F, 2.667F, 3.667F, 4.667F), Inv },
    };

    public static TheoryData<float[], string, CultureInfo> RealArraysDk { get; }
        = new TheoryData<float[], string, CultureInfo>
    {
        { new[] { 1.667F, 2.667F, 3.667F, 4.667F }, ToNumericList(Dk, 1.667F, 2.667F, 3.667F, 4.667F), Dk },
    };

    public static TheoryData<object, string, CultureInfo> IntegralLists { get; }
        = new TheoryData<object, string, CultureInfo>
    {
        { new List<int> { 1, 2, 3, 4 }, ToNumericList(Inv, 1, 2, 3, 4), Inv },
    };

    public static TheoryData<List<float>, string, CultureInfo> RealLists { get; }
        = new TheoryData<List<float>, string, CultureInfo>
    {
        { new List<float> { 1.667F, 2.667F, 3.667F, 4.667F }, "1.667,2.667,3.667,4.667", Inv },
    };

    public static TheoryData<Color, string, CultureInfo> ColorValuesInv { get; }
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

    public static TheoryData<Color, string, CultureInfo> ColorValuesDk { get; }
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
    [MemberData(nameof(IntegralValuesInv))]
    [MemberData(nameof(IntegralValuesDk))]
    [MemberData(nameof(RealValuesInv))]
    [MemberData(nameof(RealValuesDanish))]
    [MemberData(nameof(EnumValues))]
    [MemberData(nameof(IntegralArrays))]
    [MemberData(nameof(RealArraysInv))]
    [MemberData(nameof(RealArraysDk))]
    [MemberData(nameof(IntegralLists))]
    [MemberData(nameof(RealLists))]
    [MemberData(nameof(ColorValuesInv))]
    [MemberData(nameof(ColorValuesDk))]
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
        var converters = new List<ICommandConverter>
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

            new ColorConverter(),
            new EnumConverter(),

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
            new ListConverter<bool>()
        };

        return new CommandParser(converters);
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
