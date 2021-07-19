// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

#if NETCOREAPP2_1
#else
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// ApiDescriptionProvider to generate the possibilities for this middleware
    /// </summary>
    public class SwaggerApiDescriptionProvider : IApiDescriptionProvider
    {
        /// <inheritdocs />
        public int Order => -900;

        /// <inheritdocs />
        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
            // no-op
        }

        /// <inheritdocs />
        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            var apiDescription = new ApiDescription();
            apiDescription.HttpMethod = "GET";
            apiDescription.ActionDescriptor = new ActionDescriptor
            {
                RouteValues = new Dictionary<string, string>
                {
                    // Swagger uses this to group endpoints together.
                    // Group methods together using the service name.
                    ["controller"] = "ImageSharp.Web",
                },
            };
            apiDescription.RelativePath = "{image}";
            apiDescription.SupportedRequestFormats.Add(new ApiRequestFormat { MediaType = "text/plain" });

            apiDescription.ParameterDescriptions.Add(new ApiParameterDescription
            {
                Name = "image",
                Source = BindingSource.Path,
                DefaultValue = string.Empty,
            });

            apiDescription.ParameterDescriptions.Add(new ApiParameterDescription
            {
                Name = "width",
                Source = BindingSource.Query,
                DefaultValue = string.Empty,
                Type = typeof(int),
            });

            apiDescription.ParameterDescriptions.Add(new ApiParameterDescription
            {
                Name = "height",
                Source = BindingSource.Query,

                Type = typeof(int),
            });

            apiDescription.ParameterDescriptions.Add(new ApiParameterDescription
            {
                Name = "format",
                Source = BindingSource.Query,
                DefaultValue = string.Empty,
            });

            apiDescription.ParameterDescriptions.Add(new ApiParameterDescription
            {
                Name = "quality",
                Source = BindingSource.Query,
                DefaultValue = string.Empty,
            });

            apiDescription.ParameterDescriptions.Add(new ApiParameterDescription
            {
                Name = "rmode",
                Source = BindingSource.Query,
                DefaultValue = string.Empty,
            });
            context.Results.Add(apiDescription);
        }
    }
}
#endif
