// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace SixLabors.ImageSharp.Web.Tests.Helpers;

public class ExifOrientationUtilitiesTests
{
    //                  150
    // ┌─────┬─────┬─────┬─────┬─────┬─────┐
    // │     │     │     │     │     │     │
    // │     │    FFFFFFFFFFFFFFF    │     │
    // ├─────X────F┌─────┬─────┬─────┼─────┤
    // │     │    F│     │     │     │     │
    // │     │    FFFFFFFFFFFFFFF    │     │
    // ├─────┼────F┌─────┬─────┬─────┼─────┤ 100
    // │     │    F│     │     │     │     │
    // │     │    F│     │     │     │     │
    // ├─────┼────F├─────┼─────┼─────┼─────┤
    // │     │    F│     │     │     │     │
    // │     │     │     │     │     │     │
    // └─────┴─────┴─────┴─────┴─────┴─────┘
    public static TheoryData<Vector2, Vector2, Vector2, ushort, Vector2> TransformVectorData =
        new()
        {
            { new Vector2(24F, 26F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.Unknown, new Vector2(24F, 26F) },
            { new Vector2(24F, 26F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.TopLeft, new Vector2(24F, 26F) },
            { new Vector2(24F, 26F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.TopRight, new Vector2(126F, 26F) },
            { new Vector2(24F, 26F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.BottomRight, new Vector2(126F, 74F) },
            { new Vector2(24F, 26F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.BottomLeft, new Vector2(24F, 74F) },
            { new Vector2(24F, 26F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.LeftTop, new Vector2(26F, 24F) },
            { new Vector2(24F, 26F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.RightTop, new Vector2(26F, 126F) },
            { new Vector2(24F, 26F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.RightBottom, new Vector2(74F, 126F) },
            { new Vector2(24F, 26F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.LeftBottom, new Vector2(74F, 24F) },
        };

    [Theory]
    [MemberData(nameof(TransformVectorData))]
    public void CanTransformVector(Vector2 position, Vector2 min, Vector2 max, ushort orientation, Vector2 expected)
    {
        Vector2 actual = ExifOrientationUtilities.Transform(position, min, max, orientation);

        Assert.Equal(expected.X, actual.X, 4);
        Assert.Equal(expected.Y, actual.Y, 4);
    }

    public static TheoryData<Size, ushort, Size> TransformSizeData =
        new()
        {
            { new Size(150, 100), ExifOrientationMode.Unknown, new Size(150, 100) },
            { new Size(150, 100), ExifOrientationMode.TopLeft, new Size(150, 100) },
            { new Size(150, 100), ExifOrientationMode.TopRight, new Size(150, 100) },
            { new Size(150, 100), ExifOrientationMode.BottomRight, new Size(150, 100) },
            { new Size(150, 100), ExifOrientationMode.BottomLeft, new Size(150, 100) },
            { new Size(150, 100), ExifOrientationMode.LeftTop, new Size(100, 150) },
            { new Size(150, 100), ExifOrientationMode.RightTop, new Size(100, 150) },
            { new Size(150, 100), ExifOrientationMode.RightBottom, new Size(100, 150) },
            { new Size(150, 100), ExifOrientationMode.LeftBottom, new Size(100, 150) },
        };

    [Theory]
    [MemberData(nameof(TransformSizeData))]
    public void CanTransformSize(Size size, ushort orientation, Size expected)
    {
        Size actual = ExifOrientationUtilities.Transform(size, orientation);

        Assert.Equal(expected, actual);
    }

    public static TheoryData<AnchorPositionMode, ushort, AnchorPositionMode> TransformAnchorData =
        new()
        {
            { AnchorPositionMode.Center, ExifOrientationMode.Unknown, AnchorPositionMode.Center },
            { AnchorPositionMode.Center, ExifOrientationMode.TopLeft, AnchorPositionMode.Center },
            { AnchorPositionMode.Center, ExifOrientationMode.TopRight, AnchorPositionMode.Center },
            { AnchorPositionMode.Center, ExifOrientationMode.BottomRight, AnchorPositionMode.Center },
            { AnchorPositionMode.Center, ExifOrientationMode.BottomLeft, AnchorPositionMode.Center },
            { AnchorPositionMode.Center, ExifOrientationMode.LeftTop, AnchorPositionMode.Center },
            { AnchorPositionMode.Center, ExifOrientationMode.RightTop, AnchorPositionMode.Center },
            { AnchorPositionMode.Center, ExifOrientationMode.RightBottom, AnchorPositionMode.Center },
            { AnchorPositionMode.Center, ExifOrientationMode.LeftBottom, AnchorPositionMode.Center },
            { AnchorPositionMode.Top, ExifOrientationMode.Unknown, AnchorPositionMode.Top },
            { AnchorPositionMode.Top, ExifOrientationMode.TopLeft, AnchorPositionMode.Top },
            { AnchorPositionMode.Top, ExifOrientationMode.TopRight, AnchorPositionMode.Top },
            { AnchorPositionMode.Top, ExifOrientationMode.BottomRight, AnchorPositionMode.Bottom },
            { AnchorPositionMode.Top, ExifOrientationMode.BottomLeft, AnchorPositionMode.Bottom },
            { AnchorPositionMode.Top, ExifOrientationMode.LeftTop, AnchorPositionMode.Left },
            { AnchorPositionMode.Top, ExifOrientationMode.RightTop, AnchorPositionMode.Left },
            { AnchorPositionMode.Top, ExifOrientationMode.RightBottom, AnchorPositionMode.Right },
            { AnchorPositionMode.Top, ExifOrientationMode.LeftBottom, AnchorPositionMode.Right },
            { AnchorPositionMode.Bottom, ExifOrientationMode.Unknown, AnchorPositionMode.Bottom },
            { AnchorPositionMode.Bottom, ExifOrientationMode.TopLeft, AnchorPositionMode.Bottom },
            { AnchorPositionMode.Bottom, ExifOrientationMode.TopRight, AnchorPositionMode.Bottom },
            { AnchorPositionMode.Bottom, ExifOrientationMode.BottomRight, AnchorPositionMode.Top },
            { AnchorPositionMode.Bottom, ExifOrientationMode.BottomLeft, AnchorPositionMode.Top },
            { AnchorPositionMode.Bottom, ExifOrientationMode.LeftTop, AnchorPositionMode.Right },
            { AnchorPositionMode.Bottom, ExifOrientationMode.RightTop, AnchorPositionMode.Right },
            { AnchorPositionMode.Bottom, ExifOrientationMode.RightBottom, AnchorPositionMode.Left },
            { AnchorPositionMode.Bottom, ExifOrientationMode.LeftBottom, AnchorPositionMode.Left },
            { AnchorPositionMode.Left, ExifOrientationMode.Unknown, AnchorPositionMode.Left },
            { AnchorPositionMode.Left, ExifOrientationMode.TopLeft, AnchorPositionMode.Left },
            { AnchorPositionMode.Left, ExifOrientationMode.TopRight, AnchorPositionMode.Right },
            { AnchorPositionMode.Left, ExifOrientationMode.BottomRight, AnchorPositionMode.Right },
            { AnchorPositionMode.Left, ExifOrientationMode.BottomLeft, AnchorPositionMode.Left },
            { AnchorPositionMode.Left, ExifOrientationMode.LeftTop, AnchorPositionMode.Top },
            { AnchorPositionMode.Left, ExifOrientationMode.RightTop, AnchorPositionMode.Bottom },
            { AnchorPositionMode.Left, ExifOrientationMode.RightBottom, AnchorPositionMode.Bottom },
            { AnchorPositionMode.Left, ExifOrientationMode.LeftBottom, AnchorPositionMode.Top },
            { AnchorPositionMode.Right, ExifOrientationMode.Unknown, AnchorPositionMode.Right },
            { AnchorPositionMode.Right, ExifOrientationMode.TopLeft, AnchorPositionMode.Right },
            { AnchorPositionMode.Right, ExifOrientationMode.TopRight, AnchorPositionMode.Left },
            { AnchorPositionMode.Right, ExifOrientationMode.BottomRight, AnchorPositionMode.Left },
            { AnchorPositionMode.Right, ExifOrientationMode.BottomLeft, AnchorPositionMode.Right },
            { AnchorPositionMode.Right, ExifOrientationMode.LeftTop, AnchorPositionMode.Bottom },
            { AnchorPositionMode.Right, ExifOrientationMode.RightTop, AnchorPositionMode.Top },
            { AnchorPositionMode.Right, ExifOrientationMode.RightBottom, AnchorPositionMode.Top },
            { AnchorPositionMode.Right, ExifOrientationMode.LeftBottom, AnchorPositionMode.Bottom },
            { AnchorPositionMode.TopLeft, ExifOrientationMode.Unknown, AnchorPositionMode.TopLeft },
            { AnchorPositionMode.TopLeft, ExifOrientationMode.TopLeft, AnchorPositionMode.TopLeft },
            { AnchorPositionMode.TopLeft, ExifOrientationMode.TopRight, AnchorPositionMode.TopRight },
            { AnchorPositionMode.TopLeft, ExifOrientationMode.BottomRight, AnchorPositionMode.BottomRight },
            { AnchorPositionMode.TopLeft, ExifOrientationMode.BottomLeft, AnchorPositionMode.BottomLeft },
            { AnchorPositionMode.TopLeft, ExifOrientationMode.LeftTop, AnchorPositionMode.TopLeft },
            { AnchorPositionMode.TopLeft, ExifOrientationMode.RightTop, AnchorPositionMode.BottomLeft },
            { AnchorPositionMode.TopLeft, ExifOrientationMode.RightBottom, AnchorPositionMode.BottomRight },
            { AnchorPositionMode.TopLeft, ExifOrientationMode.LeftBottom, AnchorPositionMode.TopRight },
            { AnchorPositionMode.TopRight, ExifOrientationMode.Unknown, AnchorPositionMode.TopRight },
            { AnchorPositionMode.TopRight, ExifOrientationMode.TopLeft, AnchorPositionMode.TopRight },
            { AnchorPositionMode.TopRight, ExifOrientationMode.TopRight, AnchorPositionMode.TopLeft },
            { AnchorPositionMode.TopRight, ExifOrientationMode.BottomRight, AnchorPositionMode.BottomLeft },
            { AnchorPositionMode.TopRight, ExifOrientationMode.BottomLeft, AnchorPositionMode.BottomRight },
            { AnchorPositionMode.TopRight, ExifOrientationMode.LeftTop, AnchorPositionMode.BottomLeft },
            { AnchorPositionMode.TopRight, ExifOrientationMode.RightTop, AnchorPositionMode.TopLeft },
            { AnchorPositionMode.TopRight, ExifOrientationMode.RightBottom, AnchorPositionMode.TopRight },
            { AnchorPositionMode.TopRight, ExifOrientationMode.LeftBottom, AnchorPositionMode.BottomRight },
            { AnchorPositionMode.BottomRight, ExifOrientationMode.Unknown, AnchorPositionMode.BottomRight },
            { AnchorPositionMode.BottomRight, ExifOrientationMode.TopLeft, AnchorPositionMode.BottomRight },
            { AnchorPositionMode.BottomRight, ExifOrientationMode.TopRight, AnchorPositionMode.BottomLeft },
            { AnchorPositionMode.BottomRight, ExifOrientationMode.BottomRight, AnchorPositionMode.TopLeft },
            { AnchorPositionMode.BottomRight, ExifOrientationMode.BottomLeft, AnchorPositionMode.TopRight },
            { AnchorPositionMode.BottomRight, ExifOrientationMode.LeftTop, AnchorPositionMode.BottomRight },
            { AnchorPositionMode.BottomRight, ExifOrientationMode.RightTop, AnchorPositionMode.TopRight },
            { AnchorPositionMode.BottomRight, ExifOrientationMode.RightBottom, AnchorPositionMode.TopLeft },
            { AnchorPositionMode.BottomRight, ExifOrientationMode.LeftBottom, AnchorPositionMode.BottomLeft },
            { AnchorPositionMode.BottomLeft, ExifOrientationMode.Unknown, AnchorPositionMode.BottomLeft },
            { AnchorPositionMode.BottomLeft, ExifOrientationMode.TopLeft, AnchorPositionMode.BottomLeft },
            { AnchorPositionMode.BottomLeft, ExifOrientationMode.TopRight, AnchorPositionMode.BottomRight },
            { AnchorPositionMode.BottomLeft, ExifOrientationMode.BottomRight, AnchorPositionMode.TopRight },
            { AnchorPositionMode.BottomLeft, ExifOrientationMode.BottomLeft, AnchorPositionMode.TopLeft },
            { AnchorPositionMode.BottomLeft, ExifOrientationMode.LeftTop, AnchorPositionMode.TopRight },
            { AnchorPositionMode.BottomLeft, ExifOrientationMode.RightTop, AnchorPositionMode.BottomRight },
            { AnchorPositionMode.BottomLeft, ExifOrientationMode.RightBottom, AnchorPositionMode.BottomLeft },
            { AnchorPositionMode.BottomLeft, ExifOrientationMode.LeftBottom, AnchorPositionMode.TopLeft },
        };

    [Theory]
    [MemberData(nameof(TransformAnchorData))]
    public void CanTransformAnchor(AnchorPositionMode anchor, ushort orientation, AnchorPositionMode expected)
    {
        AnchorPositionMode actual = ExifOrientationUtilities.Transform(anchor, orientation);

        Assert.Equal(expected, actual);
    }
}
