// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArrayLiteralExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Expressions
{
    using System.Collections.Immutable;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents an array literal of the form '[element...]'.
    /// </summary>
    internal class TsArrayLiteralExpression : CodeModel, ITsArrayLiteralExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal TsArrayLiteralExpression(params ITsExpression[] elements)
        {
            Elements = elements?.ToImmutableArray() ?? ImmutableArray<ITsExpression>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsExpression> Elements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor)
        {
            visitor.VisitArrayLiteralExpression(this);
        }

        public T Accept<T>(TypeScriptVisitor<T> visitor)
        {
            return visitor.VisitArrayLiteralExpression(this);
        }

        public override string ToCodeDisplay() => $"[{Elements.ToElidedList()}]";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            WriteItems(writer, Elements, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ");
        }
    }
}
