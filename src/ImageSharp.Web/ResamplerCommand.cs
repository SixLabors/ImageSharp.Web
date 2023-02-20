// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Represents a command for setting the type of sampler to use for resampling operations.
/// </summary>
public readonly struct ResamplerCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResamplerCommand"/> struct.
    /// </summary>
    /// <param name="name">The name of the resampler command.</param>
    public ResamplerCommand(string name) => this.Name = name;

    /// <summary>
    /// Gets the name of the resampler command.
    /// </summary>
    public string Name { get; }
}
