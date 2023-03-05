// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web.Providers;

/// <summary>
/// Configuration options for the <see cref="PhysicalFileSystemProvider" />.
/// </summary>
public class PhysicalFileSystemProviderOptions
{
    /// <summary>
    /// Gets or sets the optional provider root folder path.
    /// <para>
    /// This value can be <see langword="null"/>, a fully qualified absolute path,
    /// or a path relative to the directory that contains the application
    /// content files.
    /// </para>
    /// <para>
    /// If not set, this will default to the directory that contains the web-servable
    /// application content files; commonly 'wwwroot'.
    /// </para>
    /// </summary>
    public string? ProviderRootPath { get; set; }

    /// <summary>
    /// Gets or sets the processing behavior. Defaults to <see cref="ProcessingBehavior.CommandOnly"/>.
    /// </summary>
    public ProcessingBehavior ProcessingBehavior { get; set; } = ProcessingBehavior.CommandOnly;
}
