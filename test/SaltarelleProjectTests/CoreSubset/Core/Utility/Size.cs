// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Size.cs" company="Tableau Software">
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

    /// <summary> Basic W,H size.  Use SizeUtil to manipulate. </summary>
    [Imported, Serializable]
    public sealed class Size
    {
        [ScriptName("w")]
        public int Width;

        [ScriptName("h")]
        public int Height;

        [ObjectLiteral]
        public Size(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }

    /// <summary> Basic W,H size.  Use SizeUtil to manipulate. </summary>
    [Imported, Serializable]
    public sealed class SizeF
    {
        [ScriptName("w")]
        public float Width;

        [ScriptName("h")]
        public float Height;

        [ObjectLiteral]
        public SizeF(float width, float height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
