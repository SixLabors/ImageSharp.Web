// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Represents a command for setting the type of format with which to encode processed images.
/// </summary>
public readonly struct FormatCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormatCommand"/> struct.
    /// </summary>
    /// <param name="name">The name of the resampler command.</param>
    public FormatCommand(string name) => this.Name = name;

    /// <summary>
    /// Gets the name of the resampler command.
    /// </summary>
    public string Name { get; }
}
