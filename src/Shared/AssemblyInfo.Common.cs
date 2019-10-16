// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Runtime.CompilerServices;

// Ensure the internals can be built and tested.
[assembly: InternalsVisibleTo("SixLabors.ImageSharp.Web.Tests")]
[assembly: InternalsVisibleTo("SixLabors.ImageSharp.Web.Providers.Azure")]
[assembly: InternalsVisibleTo("SixLabors.ImageSharp.Web.Providers.AWS")]
[assembly: InternalsVisibleTo("ImageSharp.Web.Benchmarks")]
