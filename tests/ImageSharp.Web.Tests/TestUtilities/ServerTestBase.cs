// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Net.Http;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public abstract class ServerTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : TestServerFixture
    {
        protected ServerTestBase(TFixture fixture)
        {
            this.HttpClient = fixture.HttpClient;
        }

        public HttpClient HttpClient { get; }
    }
}
