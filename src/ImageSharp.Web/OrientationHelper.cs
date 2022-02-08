// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Helper method for handling orientation parsing/transforms in processors.
    /// </summary>
    public static class OrientationHelper
    {
        /// <summary>
        /// The command constant for the orientation handling mode.
        /// </summary>
        public const string Command = "orient";

        /// <summary>
        /// Gets the orientation.
        /// </summary>
        /// <param name="image">The image to process.</param>
        /// <param name="commands">The ordered collection containing the processing commands.</param>
        /// <param name="parser">The command parser use for parting commands.</param>
        /// <param name="culture">The <see cref="CultureInfo" /> to use as the current parsing culture.</param>
        /// <param name="orientation">The orientation.</param>
        /// <returns>
        /// The <see cref="bool"/> indicating whether orientation should be handled.
        /// </returns>
        public static bool GetOrientation(FormattedImage image, CommandCollection commands, CommandParser parser, CultureInfo culture, out ushort orientation)
        {
            // Browsers now implement 'image-orientation: from-image' by default.
            // https://developer.mozilla.org/en-US/docs/web/css/image-orientation
            // This makes orientation handling confusing for users who expect images to be resized in accordance
            // to what they observe rather than pure (and correct) methods.
            //
            // To accomodate this we parse the dimensions to use based upon decoded EXIF orientation values.
            // We default to 'true' for EXIF orientation handling. By passing 'false' it can be turned off.
            if (commands.Contains(Command) && !parser.ParseValue<bool>(commands.GetValueOrDefault(Command), culture))
            {
                orientation = ExifOrientationMode.Unknown;
                return false;
            }

            return image.TryGetExifOrientation(out orientation) && orientation != ExifOrientationMode.TopLeft;
        }

        /// <summary>
        /// Transforms the specified value depending on the specified <paramref name="orientation" />.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="orientation">The EXIF orientation.</param>
        public static void Transform(ref ResizeOptions value, ushort orientation)
        {
            value.Size = Transform(value.Size, orientation);

            if (value.CenterCoordinates.HasValue)
            {
                value.CenterCoordinates = Transform(value.CenterCoordinates.Value, orientation);
            }

            value.Position = Transform(value.Position, orientation);
        }

        /// <summary>
        /// Transforms the specified value depending on the specified <paramref name="orientation" />.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="orientation">The EXIF orientation.</param>
        /// <returns>
        /// The transformed value.
        /// </returns>
        public static Size Transform(Size value, ushort orientation)
            => orientation switch
            {
                ExifOrientationMode.LeftTop or
                ExifOrientationMode.RightTop or
                ExifOrientationMode.RightBottom or
                ExifOrientationMode.LeftBottom => new Size(value.Height, value.Width),
                _ => value
            };

        /// <summary>
        /// Transforms the specified value depending on the specified <paramref name="orientation" />.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="orientation">The EXIF orientation.</param>
        /// <returns>
        /// The transformed value.
        /// </returns>
        public static PointF Transform(PointF value, ushort orientation)
        {
            AffineTransformBuilder builder = new();
            Size sourceSize = new(1, 1);

            // New XY is calculated based on flipping and rotating the input XY
            switch (orientation)
            {
                case ExifOrientationMode.TopRight:
                    builder.AppendTranslation(new PointF(sourceSize.Width - value.X, 0));
                    break;
                case ExifOrientationMode.BottomRight:
                    builder.AppendRotationDegrees(180);
                    builder.AppendTranslation(new PointF(0, -(sourceSize.Height - value.Y)));
                    break;
                case ExifOrientationMode.BottomLeft:
                    builder.AppendRotationDegrees(180);
                    builder.AppendTranslation(new PointF(sourceSize.Width - value.X, -(sourceSize.Height - value.Y)));
                    break;
                case ExifOrientationMode.LeftTop:
                    builder.AppendRotationDegrees(90);
                    builder.AppendTranslation(new PointF(sourceSize.Width - value.X, 0));
                    break;
                case ExifOrientationMode.RightTop:
                    builder.AppendRotationDegrees(270);
                    break;
                case ExifOrientationMode.RightBottom:
                    builder.AppendRotationDegrees(270);
                    builder.AppendTranslation(new PointF(-(sourceSize.Width - value.X), -(sourceSize.Height - value.Y)));
                    break;
                case ExifOrientationMode.LeftBottom:
                    builder.AppendRotationDegrees(90);
                    builder.AppendTranslation(new PointF(-(sourceSize.Width - value.X), 0));
                    break;
                default:
                    return value;
            }

            Matrix3x2 matrix = builder.BuildMatrix(sourceSize);

            return Vector2.Transform(value, matrix);
        }

        /// <summary>
        /// Transforms the specified value depending on the specified <paramref name="orientation" />.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="orientation">The EXIF orientation.</param>
        /// <returns>
        /// The transformed value.
        /// </returns>
        public static AnchorPositionMode Transform(AnchorPositionMode value, ushort orientation)
             => value switch
             {
                 AnchorPositionMode.Center => value,
                 AnchorPositionMode.Top => orientation switch
                 {
                     ExifOrientationMode.BottomLeft or ExifOrientationMode.BottomRight => AnchorPositionMode.Bottom,
                     ExifOrientationMode.LeftTop or ExifOrientationMode.RightTop => AnchorPositionMode.Left,
                     ExifOrientationMode.LeftBottom or ExifOrientationMode.RightBottom => AnchorPositionMode.Right,
                     _ => value,
                 },
                 AnchorPositionMode.Bottom => orientation switch
                 {
                     ExifOrientationMode.BottomLeft or ExifOrientationMode.BottomRight => AnchorPositionMode.Top,
                     ExifOrientationMode.LeftTop or ExifOrientationMode.RightTop => AnchorPositionMode.Right,
                     ExifOrientationMode.LeftBottom or ExifOrientationMode.RightBottom => AnchorPositionMode.Left,
                     _ => value,
                 },
                 AnchorPositionMode.Left => orientation switch
                 {
                     ExifOrientationMode.TopRight or ExifOrientationMode.BottomRight => AnchorPositionMode.Right,
                     ExifOrientationMode.LeftTop or ExifOrientationMode.LeftBottom => AnchorPositionMode.Top,
                     ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom => AnchorPositionMode.Bottom,
                     _ => value,
                 },
                 AnchorPositionMode.Right => orientation switch
                 {
                     ExifOrientationMode.TopRight or ExifOrientationMode.BottomRight => AnchorPositionMode.Left,
                     ExifOrientationMode.LeftTop or ExifOrientationMode.LeftBottom => AnchorPositionMode.Bottom,
                     ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom => AnchorPositionMode.Top,
                     _ => value,
                 },
                 AnchorPositionMode.TopLeft => orientation switch
                 {
                     ExifOrientationMode.TopRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.TopRight,
                     ExifOrientationMode.BottomRight or ExifOrientationMode.RightTop => AnchorPositionMode.BottomLeft,
                     ExifOrientationMode.BottomLeft or ExifOrientationMode.RightBottom => AnchorPositionMode.BottomRight,
                     _ => value,
                 },
                 AnchorPositionMode.TopRight => orientation switch
                 {
                     ExifOrientationMode.TopRight or ExifOrientationMode.RightTop => AnchorPositionMode.TopLeft,
                     ExifOrientationMode.BottomRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.BottomRight,
                     ExifOrientationMode.BottomLeft or ExifOrientationMode.LeftTop => AnchorPositionMode.BottomLeft,
                     _ => value,
                 },
                 AnchorPositionMode.BottomRight => orientation switch
                 {
                     ExifOrientationMode.TopRight or ExifOrientationMode.LeftBottom => AnchorPositionMode.BottomLeft,
                     ExifOrientationMode.BottomLeft or ExifOrientationMode.RightTop => AnchorPositionMode.TopRight,
                     ExifOrientationMode.BottomRight or ExifOrientationMode.RightBottom => AnchorPositionMode.TopLeft,
                     _ => value,
                 },
                 AnchorPositionMode.BottomLeft => orientation switch
                 {
                     ExifOrientationMode.TopRight or ExifOrientationMode.RightTop => AnchorPositionMode.BottomRight,
                     ExifOrientationMode.BottomLeft or ExifOrientationMode.LeftBottom => AnchorPositionMode.TopLeft,
                     ExifOrientationMode.BottomRight or ExifOrientationMode.LeftTop => AnchorPositionMode.TopRight,
                     _ => value,
                 },
                 _ => value,
             };
    }
}
