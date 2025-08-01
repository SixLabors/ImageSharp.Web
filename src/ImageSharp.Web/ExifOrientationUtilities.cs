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
        // New XY is calculated based on flipping and rotating the input XY.
        Vector2 bounds = max - min;
        return orientation switch
        {
            // 0 degrees, mirrored: image has been flipped back-to-front.
            ExifOrientationMode.TopRight => new Vector2(Flip(position.X, bounds.X), position.Y),

            // 180 degrees: image is upside down.
            ExifOrientationMode.BottomRight => new Vector2(Flip(position.X, bounds.X), Flip(position.Y, bounds.Y)),

            // 180 degrees, mirrored: image has been flipped back-to-front and is upside down.
            ExifOrientationMode.BottomLeft => new Vector2(position.X, Flip(position.Y, bounds.Y)),

            // 90 degrees: image has been flipped back-to-front and is on its side.
            ExifOrientationMode.LeftTop => new Vector2(position.Y, position.X),

            // 90 degrees, mirrored: image is on its side.
            ExifOrientationMode.RightTop => new Vector2(position.Y, Flip(position.X, bounds.X)),

            // 270 degrees: image has been flipped back-to-front and is on its far side.
            ExifOrientationMode.RightBottom => new Vector2(Flip(position.Y, bounds.Y), Flip(position.X, bounds.X)),

            // 270 degrees, mirrored: image is on its far side.
            ExifOrientationMode.LeftBottom => new Vector2(Flip(position.Y, bounds.Y), position.X),

            // 0 degrees: the correct orientation, no adjustment is required.
            _ => position,
        };
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
    private static float Flip(float offset, float max) => max - offset;
}
