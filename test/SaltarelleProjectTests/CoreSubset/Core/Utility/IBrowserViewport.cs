// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IBrowserViewport.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//   
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Net.Messaging;
    using System.Runtime.CompilerServices;
    using jQueryApi;
    using Tableau.JavaScript.CoreSlim;
    using Tableau.JavaScript.Vql.Bootstrap;
    using Tableau.JavaScript.Vql.Core;
    using Tableau.JavaScript.Vql.TypeDefs;

    /// <summary>
    /// Represents interface to the viewport of the browser.
    /// </summary>
    public interface IBrowserViewport
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the dimensions of this viewport.
        /// </summary>
        Rect Dimensions { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Translates the point (in document coordinates) into viewport coordinates.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>The point relative to the given viewport</returns>
        Point TranslatePositionToViewport(Point p);

        /// <summary>
        /// Gets the amount of visible room around a given point.  Visible room is the amount
        /// of space in each cardinal direction that is within the current viewport.
        /// Warning: if the point itself is not visible, the results may not be meaningful.
        /// </summary>
        /// <param name="position">A point in document space</param>
        /// <param name="padding">The amount of padding to include on all sides.  Defaults to 0</param>
        VisibleRoom GetVisibleRoom(Point position, int padding = 0);

        /// <summary>
        /// Gets a rectangle in document space enclosing a given point that expresses the portion of the document visible in the browser viewport.
        /// Warning: if the point itself is not visible, the results may not be meaningful.
        /// </summary>
        /// <param name="point">A point in document space</param>
        Rect GetDocumentViewport(Point point);
    }

    [Imported, Serializable]
    public sealed class VisibleRoom
    {
        public double RoomAbove;
        public double RoomBelow;
        public double RoomLeft;
        public double RoomRight;

        [ObjectLiteral]
        public VisibleRoom(double roomAbove, double roomBelow, double roomLeft, double roomRight)
        {
            this.RoomAbove = roomAbove;
            this.RoomBelow = roomBelow;
            this.RoomLeft = roomLeft;
            this.RoomRight = roomRight;
        }
    }
}
