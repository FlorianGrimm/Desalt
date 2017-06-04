// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="PointUtil.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using jQueryApi;
    using Tableau.JavaScript.Vql.TypeDefs;

    /// <summary> This class provides methods that operate on Point records. </summary>
    public static class PointUtil
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static Point FromPresModel(PointPresModel pointPM)
        {
            if (Script.IsNullOrUndefined(pointPM)) { return null; }
            return new Point(pointPM.X, pointPM.Y);
        }

        public static PointPresModel ToPresModel(Point pt)
        {
            if (Script.IsNullOrUndefined(pt)) { return null; }
            PointPresModel pointPM = new PointPresModel();
            pointPM.X = pt.X;
            pointPM.Y = pt.Y;
            return pointPM;
        }

        /// <summary>
        /// Converts a <see cref="jQueryPosition"/> to a <see cref="Point"/>.
        /// </summary>
        /// <param name="position">The position to convert.</param>
        /// <returns>A new <see cref="Point"/> representing the same coordinates as <paramref name="position"/>.</returns>
        public static Point FromPosition(jQueryPosition position)
        {
            return new Point(position.Left.RoundToInt(), position.Top.RoundToInt());
        }

        /// <summary>
        /// Adds first and second and returns the result. Normally this would be an operator overload, but Script#
        /// doesn't support them.
        /// </summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <returns>The sum of first and second.</returns>
        public static Point Add(Point first, Point second)
        {
            if (Script.IsNullOrUndefined(first) || Script.IsNullOrUndefined(second))
            {
                return first;
            }

            return new Point(first.X + second.X, first.Y + second.Y);
        }

        /// <summary>
        /// Subtracts first - second and returns the result. Normally this would be an operator overload, but Script#
        /// doesn't support them.
        /// </summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <returns>The difference of first - second.</returns>
        public static Point Subtract(Point first, Point second)
        {
            return new Point(first.X - second.X, first.Y - second.Y);
        }

        /// <summary>
        /// Gets the distance between the first and second points.
        /// </summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <returns>The distance between first and second</returns>
        public static double Distance(Point first, Point second)
        {
            Param.VerifyValue(first, "first");
            Param.VerifyValue(second, "second");

            int diffX = first.X - second.X;
            int diffY = first.Y - second.Y;

            return Math.Sqrt((diffX * diffX) + (diffY * diffY));
        }

        /// <summary>
        /// Quick distance test between two points
        /// </summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <param name="distance">Maximum distance between the two points.</param>
        /// <returns>true if points are less than or equal to the specified distance</returns>
        public static bool IsWithinDistance(Point first, Point second, long distance)
        {
            int diffX = first.X - second.X;
            int diffY = first.Y - second.Y;
            return ((diffX * diffX) + (diffY * diffY)) <= (distance * distance);
        }

        /// <summary>
        /// Tests if two points are equal.
        /// </summary>
        /// <param name="p">The first point to test</param>
        /// <param name="p2">The second point to test</param>
        /// <returns>True if p and p2 are not null and p.x,y equal p2.x,y</returns>
        public static bool Equals(Point p, Point p2)
        {
            return Script.IsValue(p) && Script.IsValue(p2) && p2.X == p.X && p2.Y == p.Y;
        }
    }

    /// <summary> This class provides methods that operate on PointF records. </summary>
    public static class PointFUtil
    {
        /// <summary>
        /// Subtracts first - second and returns the result. Normally this would be an operator overload, but Script#
        /// doesn't support them.
        /// </summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <returns>The difference of first - second.</returns>
        public static PointF Subtract(PointF first, PointF second)
        {
            return new PointF(first.X - second.X, first.Y - second.Y);
        }

        public static PointF TimesScalar(Point p, float scalar)
        {
            return new PointF(p.X * scalar, p.Y * scalar);
        }

        public static Point Round(PointF p)
        {
            return new Point(Math.JsRound(p.X), Math.JsRound(p.Y));
        }
    }
}
