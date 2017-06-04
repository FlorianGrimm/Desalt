// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Rect.cs" company="Tableau Software">
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

    /// <summary> Rectangle record for a (left, top) based clipRect. Four script fields: l, t, w, h. </summary>
    [Imported, Serializable]
    public sealed class Rect
    {
        [ScriptName("l")]
        public int Left;

        [ScriptName("t")]
        public int Top;

        [ScriptName("w")]
        public int Width;

        [ScriptName("h")]
        public int Height;

        [ObjectLiteral]
        public Rect(int left, int top, int width, int height)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
        }
    }

    /// <summary> Rectangle record for a (x, y) based clipRect. Four script fields: x, y, w, h. </summary>
    [Imported, Serializable]
    public sealed class RectXY
    {
        public int X;
        public int Y;

        [ScriptName("w")]
        public int Width;

        [ScriptName("h")]
        public int Height;

        [ObjectLiteral]
        public RectXY(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }

    /// <summary> Rectangle record for a (x, y) based rect. Four script fields: x, y, w, h. </summary>
    [Imported, Serializable]
    public sealed class DoubleRectXY
    {
        public double X;
        public double Y;

        [ScriptName("w")]
        public double Width;

        [ScriptName("h")]
        public double Height;

        [ObjectLiteral]
        public DoubleRectXY(double x, double y, double width, double height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }

    /// <summary> BBoxRectF record for a bounding box rect. Four script fields: x, y, w, h. </summary>
    [Imported, Serializable]
    public sealed class BBoxRectF
    {
        public float MinX;
        public float MinY;
        public float MaxX;
        public float MaxY;

        [ObjectLiteral]
        public BBoxRectF(float minX, float minY, float maxX, float maxY)
        {
            this.MinX = minX;
            this.MinY = minY;
            this.MaxX = maxX;
            this.MaxY = maxY;
        }
    }

    /// <summary> RegionRect is a LTWH rectangle with a region name. Script fields: l, t, w, h, r. </summary>
    [Imported, Serializable]
    public sealed class RegionRect
    {
        public int X;

        public int Y;

        [ScriptName("w")]
        public int Width;

        [ScriptName("h")]
        public int Height;

        [ScriptName("r")]
        public string RegionType;

        [ObjectLiteral]
        public RegionRect(int x, int y, int width, int height, string regionType)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.RegionType = regionType;
        }
    }
}
