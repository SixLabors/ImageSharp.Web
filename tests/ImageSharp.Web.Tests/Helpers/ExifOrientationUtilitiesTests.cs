// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Helpers
{
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
                { new Vector2(25F, 25F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.Unknown, new Vector2(25F, 25F) },
                { new Vector2(25F, 25F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.TopLeft, new Vector2(25F, 25F) },
                { new Vector2(25F, 25F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.TopRight, new Vector2(125F, 25F) },
                { new Vector2(25F, 25F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.BottomRight, new Vector2(125F, 75F) },
                { new Vector2(25F, 25F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.BottomLeft, new Vector2(25F, 75F) },
                { new Vector2(25F, 25F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.LeftTop, new Vector2(25F, 25F) },
                { new Vector2(25F, 25F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.RightTop, new Vector2(25F, 125F) },
                { new Vector2(25F, 25F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.RightBottom, new Vector2(75F, 125F) },
                { new Vector2(25F, 25F), Vector2.Zero, new Vector2(150, 100), ExifOrientationMode.LeftBottom, new Vector2(75F, 25F) }
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
                { AnchorPositionMode.Center, ExifOrientationMode.TopLeft, AnchorPositionMode.Center },
                { AnchorPositionMode.Center, ExifOrientationMode.BottomLeft, AnchorPositionMode.Center },
                { AnchorPositionMode.Center, ExifOrientationMode.LeftBottom, AnchorPositionMode.Center },
                { AnchorPositionMode.Top, ExifOrientationMode.BottomLeft, AnchorPositionMode.Bottom },
                { AnchorPositionMode.Top, ExifOrientationMode.BottomRight, AnchorPositionMode.Bottom },
                { AnchorPositionMode.Top, ExifOrientationMode.LeftTop, AnchorPositionMode.Left },
                { AnchorPositionMode.Top, ExifOrientationMode.RightTop, AnchorPositionMode.Left },
                { AnchorPositionMode.Top, ExifOrientationMode.LeftBottom, AnchorPositionMode.Right },
                { AnchorPositionMode.Top, ExifOrientationMode.RightBottom, AnchorPositionMode.Right },
                { AnchorPositionMode.Bottom, ExifOrientationMode.BottomLeft, AnchorPositionMode.Top },
                { AnchorPositionMode.Bottom, ExifOrientationMode.BottomRight, AnchorPositionMode.Top },
                { AnchorPositionMode.Bottom, ExifOrientationMode.LeftTop, AnchorPositionMode.Right },
                { AnchorPositionMode.Bottom, ExifOrientationMode.RightTop, AnchorPositionMode.Right },
                { AnchorPositionMode.Bottom, ExifOrientationMode.LeftBottom, AnchorPositionMode.Left },
                { AnchorPositionMode.Bottom, ExifOrientationMode.RightBottom, AnchorPositionMode.Left },
                { AnchorPositionMode.Left, ExifOrientationMode.TopRight, AnchorPositionMode.Right },
                { AnchorPositionMode.Left, ExifOrientationMode.BottomRight, AnchorPositionMode.Right },
                { AnchorPositionMode.Left, ExifOrientationMode.LeftTop, AnchorPositionMode.Top },
                { AnchorPositionMode.Left, ExifOrientationMode.LeftBottom, AnchorPositionMode.Top },
                { AnchorPositionMode.Left, ExifOrientationMode.RightTop, AnchorPositionMode.Bottom },
                { AnchorPositionMode.Left, ExifOrientationMode.RightBottom, AnchorPositionMode.Bottom },
                { AnchorPositionMode.Right, ExifOrientationMode.TopRight, AnchorPositionMode.Left },
                { AnchorPositionMode.Right, ExifOrientationMode.BottomRight, AnchorPositionMode.Left },
                { AnchorPositionMode.Right, ExifOrientationMode.LeftTop, AnchorPositionMode.Bottom },
                { AnchorPositionMode.Right, ExifOrientationMode.LeftBottom, AnchorPositionMode.Bottom },
                { AnchorPositionMode.Right, ExifOrientationMode.RightTop, AnchorPositionMode.Top },
                { AnchorPositionMode.Right, ExifOrientationMode.RightBottom, AnchorPositionMode.Top },
                { AnchorPositionMode.TopLeft, ExifOrientationMode.TopRight, AnchorPositionMode.TopRight },
                { AnchorPositionMode.TopLeft, ExifOrientationMode.LeftBottom, AnchorPositionMode.TopRight },
                { AnchorPositionMode.TopLeft, ExifOrientationMode.BottomRight, AnchorPositionMode.BottomLeft },
                { AnchorPositionMode.TopLeft, ExifOrientationMode.RightTop, AnchorPositionMode.BottomLeft },
                { AnchorPositionMode.TopLeft, ExifOrientationMode.BottomLeft, AnchorPositionMode.BottomRight },
                { AnchorPositionMode.TopLeft, ExifOrientationMode.RightBottom, AnchorPositionMode.BottomRight },
                { AnchorPositionMode.TopRight, ExifOrientationMode.TopRight, AnchorPositionMode.TopLeft },
                { AnchorPositionMode.TopRight, ExifOrientationMode.RightTop, AnchorPositionMode.TopLeft },
                { AnchorPositionMode.TopRight, ExifOrientationMode.BottomLeft, AnchorPositionMode.BottomRight },
                { AnchorPositionMode.TopRight, ExifOrientationMode.LeftBottom, AnchorPositionMode.BottomRight },
                { AnchorPositionMode.TopRight, ExifOrientationMode.BottomRight, AnchorPositionMode.BottomLeft },
                { AnchorPositionMode.TopRight, ExifOrientationMode.LeftTop, AnchorPositionMode.BottomLeft },
                { AnchorPositionMode.BottomRight, ExifOrientationMode.TopRight, AnchorPositionMode.BottomLeft },
                { AnchorPositionMode.BottomRight, ExifOrientationMode.LeftBottom, AnchorPositionMode.BottomLeft },
                { AnchorPositionMode.BottomRight, ExifOrientationMode.BottomLeft, AnchorPositionMode.TopRight },
                { AnchorPositionMode.BottomRight, ExifOrientationMode.RightTop, AnchorPositionMode.TopRight },
                { AnchorPositionMode.BottomRight, ExifOrientationMode.BottomRight, AnchorPositionMode.TopLeft },
                { AnchorPositionMode.BottomRight, ExifOrientationMode.RightBottom, AnchorPositionMode.TopLeft },
                { AnchorPositionMode.BottomLeft, ExifOrientationMode.TopRight, AnchorPositionMode.BottomRight },
                { AnchorPositionMode.BottomLeft, ExifOrientationMode.RightTop, AnchorPositionMode.BottomRight },
                { AnchorPositionMode.BottomLeft, ExifOrientationMode.BottomLeft, AnchorPositionMode.TopLeft },
                { AnchorPositionMode.BottomLeft, ExifOrientationMode.LeftBottom, AnchorPositionMode.TopLeft },
                { AnchorPositionMode.BottomLeft, ExifOrientationMode.BottomRight, AnchorPositionMode.TopRight },
                { AnchorPositionMode.BottomLeft, ExifOrientationMode.LeftTop, AnchorPositionMode.TopRight },
            };

        [Theory]
        [MemberData(nameof(TransformAnchorData))]
        public void CanTransformAnchor(AnchorPositionMode anchor, ushort orientation, AnchorPositionMode expected)
        {
            AnchorPositionMode actual = ExifOrientationUtilities.Transform(anchor, orientation);

            Assert.Equal(expected, actual);
        }
    }
}
