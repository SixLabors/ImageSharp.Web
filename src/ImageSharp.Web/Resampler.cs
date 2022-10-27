// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Processing;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Contains reusable static instances of known resampling algorithms.
    /// </summary>
    public static class Resampler
    {
        /// <inheritdoc cref="KnownResamplers.Bicubic"/>
        public static ResamplerCommand Bicubic { get; } = new(nameof(Bicubic));

        /// <inheritdoc cref="KnownResamplers.Box"/>
        public static ResamplerCommand Box { get; } = new(nameof(Box));

        /// <inheritdoc cref="KnownResamplers.CatmullRom"/>
        public static ResamplerCommand CatmullRom { get; } = new(nameof(CatmullRom));

        /// <inheritdoc cref="KnownResamplers.Hermite"/>
        public static ResamplerCommand Hermite { get; } = new(nameof(Hermite));

        /// <inheritdoc cref="KnownResamplers.Lanczos2"/>
        public static ResamplerCommand Lanczos2 { get; } = new(nameof(Lanczos2));

        /// <inheritdoc cref="KnownResamplers.Lanczos3"/>
        public static ResamplerCommand Lanczos3 { get; } = new(nameof(Lanczos3));

        /// <inheritdoc cref="KnownResamplers.Lanczos5"/>
        public static ResamplerCommand Lanczos5 { get; } = new(nameof(Lanczos5));

        /// <inheritdoc cref="KnownResamplers.Lanczos8"/>
        public static ResamplerCommand Lanczos8 { get; } = new(nameof(Lanczos8));

        /// <inheritdoc cref="KnownResamplers.MitchellNetravali"/>
        public static ResamplerCommand MitchellNetravali { get; } = new(nameof(MitchellNetravali));

        /// <inheritdoc cref="KnownResamplers.NearestNeighbor"/>
        public static ResamplerCommand NearestNeighbor { get; } = new(nameof(NearestNeighbor));

        /// <inheritdoc cref="KnownResamplers.Robidoux"/>
        public static ResamplerCommand Robidoux { get; } = new(nameof(Robidoux));

        /// <inheritdoc cref="KnownResamplers.RobidouxSharp"/>
        public static ResamplerCommand RobidouxSharp { get; } = new(nameof(RobidouxSharp));

        /// <inheritdoc cref="KnownResamplers.Spline"/>
        public static ResamplerCommand Spline { get; } = new(nameof(Spline));

        /// <inheritdoc cref="KnownResamplers.Triangle"/>
        public static ResamplerCommand Triangle { get; } = new(nameof(Triangle));

        /// <inheritdoc cref="KnownResamplers.Welch"/>
        public static ResamplerCommand Welch { get; } = new(nameof(Welch));
    }
}