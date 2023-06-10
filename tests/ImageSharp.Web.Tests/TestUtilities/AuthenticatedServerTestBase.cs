// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities;

public abstract class AuthenticatedServerTestBase<TFixture> : ServerTestBase<TFixture>
     where TFixture : AuthenticatedTestServerFixture
{
    private readonly RequestAuthorizationUtilities authorizationUtilities;
    private readonly string relativeImageSource;

    protected AuthenticatedServerTestBase(TFixture fixture, ITestOutputHelper outputHelper, string imageSource)
        : base(fixture, outputHelper, imageSource)
    {
        this.authorizationUtilities =
                   this.Fixture.Services.GetRequiredService<RequestAuthorizationUtilities>();

        this.relativeImageSource = this.ImageSource.Replace("http://localhost", string.Empty);
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
        response = await this.HttpClient.GetAsync(url + this.Fixture.Commands[0] + "&" + RequestAuthorizationUtilities.TokenCommand + "=INVALID");
        Assert.NotNull(response);
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    protected override string AugmentCommand(string command)
    {
        string uri = this.relativeImageSource + command;
        string token = this.GetToken(uri);
        return command + "&" + RequestAuthorizationUtilities.TokenCommand + "=" + token;
    }

    private string GetToken(string uri)
    {
        string tokenSync = this.authorizationUtilities.ComputeHMAC(uri, CommandHandling.Sanitize);
        Assert.NotNull(tokenSync);
        return tokenSync;
    }
}
