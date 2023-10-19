// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace SixLabors.ImageSharp.Web;

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
        if (orientation is <= ExifOrientationMode.TopLeft or > ExifOrientationMode.LeftBottom)
        {
            // Short circuit orientations that are not transformed below
            return position;
        }

        // New XY is calculated based on flipping and rotating the input XY.
        // Coordinate ranges are normalized to a range of 0-1 so we can pass a
        // constant integer size to the transform builder.
        Vector2 scaled = Scale(position, min, max);
        AffineTransformBuilder builder = new();
        Size size = new(1, 1);
        switch (orientation)
        {
            case ExifOrientationMode.TopRight:
                builder.AppendTranslation(new Vector2(FlipScaled(scaled.X), 0));
                break;
            case ExifOrientationMode.BottomRight:
                builder.AppendRotationDegrees(180);
                break;
            case ExifOrientationMode.BottomLeft:
                builder.AppendTranslation(new Vector2(0, FlipScaled(scaled.Y)));
                break;
            case ExifOrientationMode.LeftTop:
                builder.AppendTranslation(new Vector2(FlipScaled(scaled.X), 0));
                builder.AppendRotationDegrees(270);
                break;
            case ExifOrientationMode.RightTop:
                builder.AppendRotationDegrees(270);
                break;
            case ExifOrientationMode.RightBottom:
                builder.AppendTranslation(new Vector2(FlipScaled(scaled.X), 0));
                builder.AppendRotationDegrees(90);
                break;
            case ExifOrientationMode.LeftBottom:
                builder.AppendRotationDegrees(90);
                break;
            default:
                // Use identity matrix.
                break;
        }

        Matrix3x2 matrix = builder.BuildMatrix(size);
        return DeScale(Vector2.Transform(scaled, matrix), SwapXY(min, orientation), SwapXY(max, orientation));
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
      => orientation switch
      {
          ExifOrientationMode.TopRight => anchor switch
          {
              AnchorPositionMode.Center => AnchorPositionMode.Center,
              AnchorPositionMode.Top => AnchorPositionMode.Top,
              AnchorPositionMode.Bottom => AnchorPositionMode.Bottom,
              AnchorPositionMode.Left => AnchorPositionMode.Right,
              AnchorPositionMode.Right => AnchorPositionMode.Left,
              AnchorPositionMode.TopLeft => AnchorPositionMode.TopRight,
              AnchorPositionMode.TopRight => AnchorPositionMode.TopLeft,
              AnchorPositionMode.BottomRight => AnchorPositionMode.BottomLeft,
              AnchorPositionMode.BottomLeft => AnchorPositionMode.BottomRight,
              _ => anchor
          },
          ExifOrientationMode.BottomRight => anchor switch
          {
              AnchorPositionMode.Center => AnchorPositionMode.Center,
              AnchorPositionMode.Top => AnchorPositionMode.Bottom,
              AnchorPositionMode.Bottom => AnchorPositionMode.Top,
              AnchorPositionMode.Left => AnchorPositionMode.Right,
              AnchorPositionMode.Right => AnchorPositionMode.Left,
              AnchorPositionMode.TopLeft => AnchorPositionMode.BottomRight,
              AnchorPositionMode.TopRight => AnchorPositionMode.BottomLeft,
              AnchorPositionMode.BottomRight => AnchorPositionMode.TopLeft,
              AnchorPositionMode.BottomLeft => AnchorPositionMode.TopRight,
              _ => anchor
          },
          ExifOrientationMode.BottomLeft => anchor switch
          {
              AnchorPositionMode.Center => AnchorPositionMode.Center,
              AnchorPositionMode.Top => AnchorPositionMode.Bottom,
              AnchorPositionMode.Bottom => AnchorPositionMode.Top,
              AnchorPositionMode.Left => AnchorPositionMode.Left,
              AnchorPositionMode.Right => AnchorPositionMode.Right,
              AnchorPositionMode.TopLeft => AnchorPositionMode.BottomLeft,
              AnchorPositionMode.TopRight => AnchorPositionMode.BottomRight,
              AnchorPositionMode.BottomRight => AnchorPositionMode.TopRight,
              AnchorPositionMode.BottomLeft => AnchorPositionMode.TopLeft,
              _ => anchor
          },
          ExifOrientationMode.LeftTop => anchor switch
          {
              AnchorPositionMode.Center => AnchorPositionMode.Center,
              AnchorPositionMode.Top => AnchorPositionMode.Left,
              AnchorPositionMode.Bottom => AnchorPositionMode.Right,
              AnchorPositionMode.Left => AnchorPositionMode.Top,
              AnchorPositionMode.Right => AnchorPositionMode.Bottom,
              AnchorPositionMode.TopLeft => AnchorPositionMode.TopLeft,
              AnchorPositionMode.TopRight => AnchorPositionMode.BottomLeft,
              AnchorPositionMode.BottomRight => AnchorPositionMode.BottomRight,
              AnchorPositionMode.BottomLeft => AnchorPositionMode.TopRight,
              _ => anchor
          },
          ExifOrientationMode.RightTop => anchor switch
          {
              AnchorPositionMode.Center => AnchorPositionMode.Center,
              AnchorPositionMode.Top => AnchorPositionMode.Left,
              AnchorPositionMode.Bottom => AnchorPositionMode.Right,
              AnchorPositionMode.Left => AnchorPositionMode.Bottom,
              AnchorPositionMode.Right => AnchorPositionMode.Top,
              AnchorPositionMode.TopLeft => AnchorPositionMode.BottomLeft,
              AnchorPositionMode.TopRight => AnchorPositionMode.TopLeft,
              AnchorPositionMode.BottomRight => AnchorPositionMode.TopRight,
              AnchorPositionMode.BottomLeft => AnchorPositionMode.BottomRight,
              _ => anchor
          },
          ExifOrientationMode.RightBottom => anchor switch
          {
              AnchorPositionMode.Center => AnchorPositionMode.Center,
              AnchorPositionMode.Top => AnchorPositionMode.Right,
              AnchorPositionMode.Bottom => AnchorPositionMode.Left,
              AnchorPositionMode.Left => AnchorPositionMode.Bottom,
              AnchorPositionMode.Right => AnchorPositionMode.Top,
              AnchorPositionMode.TopLeft => AnchorPositionMode.BottomRight,
              AnchorPositionMode.TopRight => AnchorPositionMode.TopRight,
              AnchorPositionMode.BottomRight => AnchorPositionMode.TopLeft,
              AnchorPositionMode.BottomLeft => AnchorPositionMode.BottomLeft,
              _ => anchor
          },
          ExifOrientationMode.LeftBottom => anchor switch
          {
              AnchorPositionMode.Center => AnchorPositionMode.Center,
              AnchorPositionMode.Top => AnchorPositionMode.Right,
              AnchorPositionMode.Bottom => AnchorPositionMode.Left,
              AnchorPositionMode.Left => AnchorPositionMode.Top,
              AnchorPositionMode.Right => AnchorPositionMode.Bottom,
              AnchorPositionMode.TopLeft => AnchorPositionMode.TopRight,
              AnchorPositionMode.TopRight => AnchorPositionMode.BottomRight,
              AnchorPositionMode.BottomRight => AnchorPositionMode.BottomLeft,
              AnchorPositionMode.BottomLeft => AnchorPositionMode.TopLeft,
              _ => anchor
          },
          _ => anchor
      };

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
    private static Vector2 Scale(Vector2 x, Vector2 min, Vector2 max) => (x - min) / (max - min);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 DeScale(Vector2 x, Vector2 min, Vector2 max) => min + (x * (max - min));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float FlipScaled(float origin) => (2F * -origin) + 1F;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 SwapXY(Vector2 position, ushort orientation)
        => IsExifOrientationRotated(orientation)
        ? new Vector2(position.Y, position.X)
        : position;
}
