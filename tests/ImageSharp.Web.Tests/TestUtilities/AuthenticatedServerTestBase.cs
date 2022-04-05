// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public abstract class AuthenticatedServerTestBase<TFixture> : ServerTestBase<TFixture>
         where TFixture : AuthenticatedTestServerFixture
    {
        protected AuthenticatedServerTestBase(TFixture fixture, ITestOutputHelper outputHelper, string imageSource)
            : base(fixture, outputHelper, imageSource)
        {
        }

        protected override string AugmentCommand(string command)
        {
            // Mimic the lowecase relative url format used by the token and default options.
            string uri = (this.ImageSource + command).Replace("http://localhost", string.Empty).ToLowerInvariant();
            string token = HMACUtilities.ComputeHMACSHA256(uri, AuthenticatedTestServerFixture.HMACSecretKey);
            return command + "&" + HMACUtilities.TokenCommand + "=" + token;
        }
    }
}
