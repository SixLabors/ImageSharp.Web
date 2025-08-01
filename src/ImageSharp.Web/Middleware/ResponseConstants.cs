// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Middleware;

/// <summary>
/// Contains constants related to HTTP response codes.
/// </summary>
internal static class ResponseConstants
{
    /// <summary>
    /// The HTTP 200 OK success status response code indicates that the request has succeeded.
    /// </summary>
    internal const int Status200Ok = 200;

    /// <summary>
    /// The HTTP 304 Not Modified client redirection response code indicates that there is no need
    /// to retransmit the requested resources.
    /// </summary>
    internal const int Status304NotModified = 304;

    /// <summary>
    /// The HTTP 412 Precondition Failed client error response code indicates that access to the target
    /// resource has been denied.
    /// </summary>
    internal const int Status412PreconditionFailed = 412;
}
