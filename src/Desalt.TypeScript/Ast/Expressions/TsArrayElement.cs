// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArrayElement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents an element in an array.
    /// </summary>
    internal class TsArrayElement : AstNode, ITsArrayElement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArrayElement(ITsExpression element, bool isSpreadElement = false)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            IsSpreadElement = isSpreadElement;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Element { get; }

        /// <summary>
        /// Indicates whether the <see cref="ITsArrayElement.Element"/> is preceded by a spread operator '...'.
        /// </summary>
        public bool IsSpreadElement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitArrayElement(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitArrayElement(this);

        public override string ToCodeDisplay() => (IsSpreadElement ? "... " : "") + Element.ToCodeDisplay();

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            if (IsSpreadElement)
            {
                writer.Write("... ");
            }

            Element.WriteFullCodeDisplay(writer);
        }
    }
}
