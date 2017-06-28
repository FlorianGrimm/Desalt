// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArrayLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents an array literal of the form '[element...]'.
    /// </summary>
    internal class TsArrayLiteral : AstNode, ITsArrayLiteral
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal TsArrayLiteral(params ITsArrayElement[] elements)
        {
            Elements = elements?.ToImmutableArray() ?? ImmutableArray<ITsArrayElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsArrayElement> Elements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitArrayLiteral(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitArrayLiteral(this);

        public override string CodeDisplay => $"[{Elements.ToElidedList()}]";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            WriteItems(writer, Elements, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ");
        }
    }
}
