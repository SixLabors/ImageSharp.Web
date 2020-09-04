// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Globalization;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
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
}
