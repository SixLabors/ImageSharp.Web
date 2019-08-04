using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;
using System;
using System.Threading.Tasks;

namespace SixLabors.ImageSharp.Web.Tests.DependencyInjection
{
    public class MockImageProvider : IImageProvider
    {
        public Func<HttpContext, bool> Match { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task<IImageResolver> GetAsync(HttpContext context) => throw new NotImplementedException();

        public bool IsValidRequest(HttpContext context) => throw new NotImplementedException();
    }
}
