// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Processing;

namespace SixLabors.ImageSharp.Web;

/// <summary>
/// Contains reusable static instances of known resampling algorithms.
/// </summary>
public static class Resampler
{
    /// <inheritdoc cref="KnownResamplers.Bicubic"/>
    public static ResamplerCommand Bicubic { get; } = new("bicubic");

    /// <inheritdoc cref="KnownResamplers.Box"/>
    public static ResamplerCommand Box { get; } = new("box");

    /// <inheritdoc cref="KnownResamplers.CatmullRom"/>
    public static ResamplerCommand CatmullRom { get; } = new("catmullrom");

    /// <inheritdoc cref="KnownResamplers.Hermite"/>
    public static ResamplerCommand Hermite { get; } = new("hermite");

    /// <inheritdoc cref="KnownResamplers.Lanczos2"/>
    public static ResamplerCommand Lanczos2 { get; } = new("lanczos2");

    /// <inheritdoc cref="KnownResamplers.Lanczos3"/>
    public static ResamplerCommand Lanczos3 { get; } = new("lanczos3");

    /// <inheritdoc cref="KnownResamplers.Lanczos5"/>
    public static ResamplerCommand Lanczos5 { get; } = new("lanczos5");

    /// <inheritdoc cref="KnownResamplers.Lanczos8"/>
    public static ResamplerCommand Lanczos8 { get; } = new("lanczos8");

    /// <inheritdoc cref="KnownResamplers.MitchellNetravali"/>
    public static ResamplerCommand MitchellNetravali { get; } = new("mitchellnetravali");

    /// <inheritdoc cref="KnownResamplers.NearestNeighbor"/>
    public static ResamplerCommand NearestNeighbor { get; } = new("nearestneighbor");

    /// <inheritdoc cref="KnownResamplers.Robidoux"/>
    public static ResamplerCommand Robidoux { get; } = new("robidoux");

    /// <inheritdoc cref="KnownResamplers.RobidouxSharp"/>
    public static ResamplerCommand RobidouxSharp { get; } = new("robidouxsharp");

    /// <inheritdoc cref="KnownResamplers.Spline"/>
    public static ResamplerCommand Spline { get; } = new("spline");

    /// <inheritdoc cref="KnownResamplers.Triangle"/>
    public static ResamplerCommand Triangle { get; } = new("triangle");

    /// <inheritdoc cref="KnownResamplers.Welch"/>
    public static ResamplerCommand Welch { get; } = new("welch");
}
