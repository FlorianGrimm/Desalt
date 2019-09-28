// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Point.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary> Basic X,Y point.  Use PointUtil to manipulate. </summary>
    [Imported, Serializable]
    public sealed class Point
    {
        public int X;

        public int Y;

        [ObjectLiteral]
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    /// <summary> A variation of Point record that uses floats instead of ints. </summary>
    [Imported, Serializable]
    public sealed class PointD
    {
        public double X;

        public double Y;

        [ObjectLiteral]
        public PointD(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
