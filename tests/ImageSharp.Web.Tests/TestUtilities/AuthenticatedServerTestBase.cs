// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities
{
    public abstract class AuthenticatedServerTestBase<TFixture> : ServerTestBase<TFixture>
         where TFixture : AuthenticatedTestServerFixture
    {
        private readonly ImageSharpRequestAuthorizationUtilities authorizationUtilities;
        private readonly string relativeImageSouce;

        protected AuthenticatedServerTestBase(TFixture fixture, ITestOutputHelper outputHelper, string imageSource)
            : base(fixture, outputHelper, imageSource)
        {
            this.authorizationUtilities =
                       this.Fixture.Services.GetRequiredService<ImageSharpRequestAuthorizationUtilities>();

            this.relativeImageSouce = this.ImageSource.Replace("http://localhost", string.Empty);
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
            string uri = this.relativeImageSouce + command;
            string token = this.authorizationUtilities.ComputeHMAC(uri, CommandHandling.Sanitize);
            return command + "&" + ImageSharpRequestAuthorizationUtilities.TokenCommand + "=" + token;
        }
    }
}
