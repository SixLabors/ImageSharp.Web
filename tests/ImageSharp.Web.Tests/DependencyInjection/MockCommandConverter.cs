// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection;

public class MockCommandConverter : ICommandConverter<object>
{
    public Type Type => typeof(MockCommandConverter);

    public object ConvertFrom(
        CommandParser parser,
        CultureInfo culture,
        string value,
        Type propertyType)
        => null;
}
