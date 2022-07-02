// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
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

        [Fact]
        public async Task CanRejectUnauthorizedRequestAsync()
        {
            string url = this.ImageSource;

            // Send an unaugmented request without a token.
            HttpResponseMessage response = await this.HttpClient.GetAsync(url + this.Fixture.Commands[0]);
            Assert.NotNull(response);
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Now send an invalid token
            response = await this.HttpClient.GetAsync(url + this.Fixture.Commands[0] + "&" + HMACUtilities.TokenCommand + "=INVALID");
            Assert.NotNull(response);
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        protected override string AugmentCommand(string command)
        {
            // Mimic the lowecase relative url format used by the token and default options.
            string uri = (this.ImageSource + command).Replace("http://localhost", string.Empty);
            uri = CaseHandlingUriBuilder.Encode(CaseHandlingUriBuilder.CaseHandling.LowerInvariant, uri);

            string token = HMACUtilities.ComputeHMACSHA256(uri, AuthenticatedTestServerFixture.HMACSecretKey);
            return command + "&" + HMACUtilities.TokenCommand + "=" + token;
        }
    }
}
