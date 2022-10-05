// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Provides enumeration of known resampler algorithms.
    /// </summary>
    public enum Resampler
    {
        /// <summary>
        /// Bicubic sampler that implements the bicubic kernel algorithm W(x).
        /// </summary>
        Bicubic,

        /// <summary>
        /// Similar to nearest neighbor when upscaling.
        /// When downscaling the pixels will average, merging pixels together.
        /// </summary>
        Box,

        /// <summary>
        /// A well known standard Cubic Filter often used as a interpolation function
        /// </summary>
        CatmullRom,

        /// <summary>
        /// A type of smoothed triangular interpolation filter that rounds off strong edges while
        /// preserving flat 'color levels' in the original image.
        /// </summary>
        Hermite,

        /// <summary>
        /// Lanczos kernel sampler that implements smooth interpolation with a radius of 2 pixels.
        /// This algorithm provides sharpened results when compared to others when downsampling.
        /// </summary>
        Lanczos2,

        /// <summary>
        /// Lanczos kernel sampler that implements smooth interpolation with a radius of 3 pixels.
        /// This algorithm provides sharpened results when compared to others when downsampling.
        /// </summary>
        Lanczos3,

        /// <summary>
        /// Lanczos kernel sampler that implements smooth interpolation with a radius of 5 pixels.
        /// This algorithm provides sharpened results when compared to others when downsampling.
        /// </summary>
        Lanczos5,

        /// <summary>
        /// Lanczos kernel sampler that implements smooth interpolation with a radius of 8 pixels.
        /// This algorithm provides sharpened results when compared to others when downsampling.
        /// </summary>
        Lanczos8,

        /// <summary>
        /// This seperable cubic algorithm yields a very good equilibrium between
        /// detail preservation (sharpness) and smoothness.
        /// </summary>
        MitchellNetravali,

        /// <summary>
        /// This uses a very fast, unscaled filter
        /// which will select the closest pixel to the new pixels position.
        /// </summary>
        NearestNeighbor,

        /// <summary>
        /// This algorithm developed by Nicolas Robidoux providing a very good equilibrium between
        /// detail preservation (sharpness) and smoothness comparable to <see cref="MitchellNetravali"/>.
        /// </summary>
        Robidoux,

        /// <summary>
        /// A sharpened form of the <see cref="Robidoux"/> sampler.
        /// </summary>
        RobidouxSharp,

        /// <summary>
        /// A separable cubic algorithm similar to <see cref="MitchellNetravali"/> but yielding smoother results.
        /// </summary>
        Spline,

        /// <summary>
        /// This interpolation algorithm can be used where perfect image transformation
        /// with pixel matching is impossible, so that one can calculate and assign appropriate intensity values to pixels.
        /// </summary>
        Triangle,

        /// <summary>
        /// A high speed algorithm that delivers very sharpened results.
        /// </summary>
        Welch,
    }
}
