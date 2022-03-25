// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Contains various helper methods for working with EXIF orientation values.
    /// </summary>
    public static class ExifOrientationUtilities
    {
        /// <summary>
        /// Transforms the specified vector value depending on the specified <paramref name="orientation" />.
        /// </summary>
        /// <param name="position">The input vector value.</param>
        /// <param name="min">The minimum bounds of the area of interest.</param>
        /// <param name="max">The maximum bounds of the area of interest.</param>
        /// <param name="orientation">The EXIF orientation.</param>
        /// <returns>
        /// The transformed vector.
        /// </returns>
        public static Vector2 Transform(Vector2 position, Vector2 min, Vector2 max, ushort orientation)
        {
            if (orientation is >= ExifOrientationMode.Unknown and <= ExifOrientationMode.TopLeft)
            {
                return position;
            }

            // New XY is calculated based on flipping and rotating the input XY.
            // Coordinates are normalized to a range of 0-1 so we can pass a constant integer size to the transform builder.
            Vector2 normalized = Normalize(position, min, max);
            AffineTransformBuilder builder = new();
            Size size = new(1, 1);
            switch (orientation)
            {
                case ExifOrientationMode.TopRight:
                    builder.AppendTranslation(new Vector2(FlipNormalized(normalized.X), 0));
                    break;
                case ExifOrientationMode.BottomRight:
                    builder.AppendRotationDegrees(180);
                    break;
                case ExifOrientationMode.BottomLeft:
                    builder.AppendTranslation(new Vector2(0, FlipNormalized(normalized.Y)));
                    break;
                case ExifOrientationMode.LeftTop:
                    builder.AppendTranslation(new Vector2(FlipNormalized(normalized.X), 0));
                    builder.AppendRotationDegrees(270);
                    break;
                case ExifOrientationMode.RightTop:
                    builder.AppendRotationDegrees(270);
                    break;
                case ExifOrientationMode.RightBottom:
                    builder.AppendTranslation(new Vector2(FlipNormalized(normalized.X), 0));
                    builder.AppendRotationDegrees(90);
                    break;
                case ExifOrientationMode.LeftBottom:
                    builder.AppendRotationDegrees(90);
                    break;
                default:
                    return position;
            }

            Matrix3x2 matrix = builder.BuildMatrix(size);
            return DeNormalize(Vector2.Transform(normalized, matrix), min, max);
        }

        /// <summary>
        /// Transforms the specified size value depending on the specified <paramref name="orientation" />.
        /// </summary>
        /// <param name="size">The input size value.</param>
        /// <param name="orientation">The EXIF orientation.</param>
        /// <returns>
        /// The transformed size.
        /// </returns>
        public static Size Transform(Size size, ushort orientation)
            => IsExifOrientationRotated(orientation)
            ? new Size(size.Height, size.Width)
            : size;

        /// <summary>
        /// Transforms the specified anchor value depending on the specified <paramref name="orientation" />.
        /// </summary>
        /// <param name="anchor">The input anchor value.</param>
        /// <param name="orientation">The EXIF orientation.</param>
        /// <returns>
        /// The transformed anchor.
        /// </returns>
        public static AnchorPositionMode Transform(AnchorPositionMode anchor, ushort orientation)
        {
            if (orientation is >= ExifOrientationMode.Unknown and <= ExifOrientationMode.TopLeft)
            {
                return anchor;
            }

            /*
                New anchor position is determined by calculating the direction of the anchor relative to the source image.
                In the following example, the TopRight anchor becomes BottomRight

                            T                              L
                +-----------------------+           +--------------+
                |                      *|           |              |
                |                       |           |              |
              L |           TL          | R         |              |
                |                       |           |              |
                |                       |         B |      LB      | T
                |                       |           |              |
                +-----------------------+           |              |
                            B                       |              |
                                                    |              |
                                                    |             *|
                                                    +--------------+
                                                            R
          */
            return anchor switch
            {
                AnchorPositionMode.Center => anchor,
                AnchorPositionMode.Top => orientation switch
                {
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.BottomRight => AnchorPositionMode.Bottom,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.RightTop => AnchorPositionMode.Left,
                    ExifOrientationMode.LeftBottom or ExifOrientationMode.RightBottom => AnchorPositionMode.Right,
                    _ => anchor,
                },
                AnchorPositionMode.Bottom => orientation switch
                {
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.BottomRight => AnchorPositionMode.Top,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.RightTop => AnchorPositionMode.Right,
                    ExifOrientationMode.LeftBottom or ExifOrientationMode.RightBottom => AnchorPositionMode.Left,
                    _ => anchor,
                },
                AnchorPositionMode.Left => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.BottomRight => AnchorPositionMode.Right,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.LeftBottom => AnchorPositionMode.Top,
                    ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom => AnchorPositionMode.Bottom,
                    _ => anchor,
                },
                AnchorPositionMode.Right => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.BottomRight => AnchorPositionMode.Left,
                    ExifOrientationMode.LeftTop or ExifOrientationMode.LeftBottom => AnchorPositionMode.Bottom,
                    ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom => AnchorPositionMode.Top,
                    _ => anchor,
                },
                AnchorPositionMode.TopLeft => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.TopRight,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.RightTop => AnchorPositionMode.BottomLeft,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.RightBottom => AnchorPositionMode.BottomRight,
                    _ => anchor,
                },
                AnchorPositionMode.TopRight => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.RightTop => AnchorPositionMode.TopLeft,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.LeftBottom => AnchorPositionMode.BottomRight,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.LeftTop => AnchorPositionMode.BottomLeft,
                    _ => anchor,
                },
                AnchorPositionMode.BottomRight => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.BottomLeft,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.RightTop => AnchorPositionMode.TopRight,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.RightBottom => AnchorPositionMode.TopLeft,
                    _ => anchor,
                },
                AnchorPositionMode.BottomLeft => orientation switch
                {
                    ExifOrientationMode.TopRight or ExifOrientationMode.RightTop => AnchorPositionMode.BottomRight,
                    ExifOrientationMode.BottomLeft or ExifOrientationMode.LeftBottom => AnchorPositionMode.TopLeft,
                    ExifOrientationMode.BottomRight or ExifOrientationMode.LeftTop => AnchorPositionMode.TopRight,
                    _ => anchor,
                },
                _ => anchor,
            };
        }

        /// <summary>
        /// Returns a value indicating whether an EXIF orientation is rotated (not flipped).
        /// </summary>
        /// <param name="orientation">The EXIF orientation.</param>
        /// <returns><see langword="true"/> if the orientation value indicates rotation; otherwise <see langword="false"/>.</returns>
        public static bool IsExifOrientationRotated(ushort orientation)
            => orientation switch
            {
                ExifOrientationMode.LeftTop
                or ExifOrientationMode.RightTop
                or ExifOrientationMode.RightBottom
                or ExifOrientationMode.LeftBottom => true,
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 Normalize(Vector2 x, Vector2 min, Vector2 max) => (x - min) / (max - min);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 DeNormalize(Vector2 x, Vector2 min, Vector2 max) => min + (x * (max - min));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float FlipNormalized(float origin) => (2F * -origin) + 1F;
    }
}
