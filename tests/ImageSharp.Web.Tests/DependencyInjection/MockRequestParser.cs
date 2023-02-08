// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection;

public class MockRequestParser : IRequestParser
{
    public CommandCollection ParseRequestCommands(HttpContext context) => null;
}
