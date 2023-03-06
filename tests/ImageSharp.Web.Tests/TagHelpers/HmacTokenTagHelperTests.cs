// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.WebEncoders.Testing;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.TagHelpers;

namespace SixLabors.ImageSharp.Web.Tests.TagHelpers;

public sealed class HmacTokenTagHelperTests : IDisposable
{
    public HmacTokenTagHelperTests()
    {
        ServiceCollection services = new();
        services.AddSingleton<IWebHostEnvironment, FakeWebHostEnvironment>();
        services.AddImageSharp(options => options.HMACSecretKey = new byte[] { 1, 2, 3, 4, 5 });
        this.Provider = services.BuildServiceProvider();
    }

    public ServiceProvider Provider { get; }

    [Fact]
    public void RendersHmacTokenTag_SrcIncludes_HMAC()
    {
        // Arrange
        TagHelperContext context = MakeTagHelperContext(
            attributes: new TagHelperAttributeList
            {
                { "src", "testimage.png?width=50" },
                { "width", 50 }
            });

        TagHelperOutput output = MakeImageTagHelperOutput(
            attributes: new TagHelperAttributeList
            {
                { "width", 50 }
            });

        TagHelperOutput expectedOutput = MakeImageTagHelperOutput(
            attributes: new TagHelperAttributeList
            {
                { "src", "testimage.png?width=50&hmac=54edff059ad28d0f0ec2494de1dce0e6152e8d26e53e2efb249cdae93e30acbc" },
                { "width", 50 }
            });

        HmacTokenTagHelper helper = this.GetHelper();
        helper.Src = "testimage.png?width=50";

        // Act
        helper.Process(context, output);

        // Assert
        Assert.Equal(expectedOutput.TagName, output.TagName);
        Assert.Equal(2, output.Attributes.Count);

        for (int i = 0; i < expectedOutput.Attributes.Count; i++)
        {
            TagHelperAttribute expectedAttribute = expectedOutput.Attributes[i];
            TagHelperAttribute actualAttribute = output.Attributes[i];
            Assert.Equal(expectedAttribute.Name, actualAttribute.Name);
            Assert.Equal(expectedAttribute.Value.ToString(), actualAttribute.Value.ToString(), ignoreCase: true);
        }
    }

    private HmacTokenTagHelper GetHelper(
        IUrlHelperFactory urlHelperFactory = null,
        ViewContext viewContext = null)
    {
        urlHelperFactory ??= new FakeUrlHelperFactory();
        viewContext ??= MakeViewContext();

        return new HmacTokenTagHelper(
            this.Provider.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>(),
            this.Provider.GetRequiredService<RequestAuthorizationUtilities>(),
            urlHelperFactory,
            new HtmlTestEncoder())
        {
            ViewContext = viewContext,
        };
    }

    private static TagHelperContext MakeTagHelperContext(
        TagHelperAttributeList attributes)
        => new(
            tagName: "image",
            allAttributes: attributes,
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString("N"));

    private static TagHelperOutput MakeImageTagHelperOutput(TagHelperAttributeList attributes)
    {
        attributes ??= new TagHelperAttributeList();

        return new TagHelperOutput(
            "img",
            attributes,
            getChildContentAsync: (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent(default);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    private static ViewContext MakeViewContext(string requestPathBase = null)
    {
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        if (requestPathBase != null)
        {
            actionContext.HttpContext.Request.PathBase = new PathString(requestPathBase);
        }

        var metadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(metadataProvider, new ModelStateDictionary());
        return new ViewContext(
            actionContext,
            new FakeView(),
            viewData,
            new FakeTempDataDictionary(),
            TextWriter.Null,
            new HtmlHelperOptions());
    }

    public void Dispose() => this.Provider.Dispose();

    private class FakeView : IView
    {
        public string Path { get; }

        public Task RenderAsync(ViewContext context) => throw new NotSupportedException();
    }

    private class FakeTempDataDictionary : Dictionary<string, object>, ITempDataDictionary
    {
        public void Keep() => throw new NotSupportedException();

        public void Keep(string key) => throw new NotSupportedException();

        public void Load() => throw new NotSupportedException();

        public object Peek(string key) => throw new NotSupportedException();

        public void Save() => throw new NotSupportedException();
    }

    private class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; }

        public IFileProvider WebRootFileProvider { get; set; } = new FakeFileProvider();

        public string ApplicationName { get; set; }

        public IFileProvider ContentRootFileProvider { get; set; }

        public string ContentRootPath { get; set; }

        public string EnvironmentName { get; set; }
    }

    private class FakeFileProvider : IFileProvider
    {
        public IDirectoryContents GetDirectoryContents(string subpath) => new FakeDirectoryContents();

        public IFileInfo GetFileInfo(string subpath) => new FakeFileInfo();

        public IChangeToken Watch(string filter) => new FakeFileChangeToken();
    }

    private class FakeFileChangeToken : IChangeToken
    {
        public FakeFileChangeToken(string filter = "") => this.Filter = filter;

        public bool ActiveChangeCallbacks => false;

        public bool HasChanged { get; set; }

        public string Filter { get; }

        public IDisposable RegisterChangeCallback(Action<object> callback, object state) => new NullDisposable();

        private sealed class NullDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public override string ToString() => this.Filter;
    }

    private class FakeDirectoryContents : IDirectoryContents
    {
        public bool Exists { get; }

        public IEnumerator<IFileInfo> GetEnumerator() => Enumerable.Empty<IFileInfo>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    private class FakeFileInfo : IFileInfo
    {
        public bool Exists { get; } = true;

        public bool IsDirectory { get; }

        public DateTimeOffset LastModified { get; }

        public long Length { get; }

        public string Name { get; }

        public string PhysicalPath { get; }

        public Stream CreateReadStream() => new MemoryStream(Encoding.UTF8.GetBytes("Hello World!"));
    }

    private class FakeUrlHelperFactory : IUrlHelperFactory
    {
        public IUrlHelper GetUrlHelper(ActionContext context) => new FakeUrlHelper() { ActionContext = context };
    }

    private class FakeUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext { get; set; }

        public string Action(UrlActionContext actionContext) => throw new NotSupportedException();

        // Ensure expanded path does not look like an absolute path on Linux, avoiding
        // https://github.com/aspnet/External/issues/21
        [return: NotNullIfNotNull("contentPath")]
        public string Content(string contentPath) => contentPath.Replace("~/", "virtualRoot/");

        public bool IsLocalUrl([NotNullWhen(true)] string url) => throw new NotSupportedException();

        public string Link(string routeName, object values) => throw new NotSupportedException();

        public string RouteUrl(UrlRouteContext routeContext) => throw new NotSupportedException();
    }
}
