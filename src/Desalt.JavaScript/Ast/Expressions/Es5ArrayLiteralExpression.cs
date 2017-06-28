// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ArrayLiteralExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an array literal of the form '[element...]'.
    /// </summary>
    public class Es5ArrayLiteralExpression : AstNode<Es5Visitor>, IEs5Expression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ArrayLiteralExpression(params IEs5Expression[] elements)
        {
            Elements = elements?.ToImmutableArray() ?? ImmutableArray<IEs5Expression>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<IEs5Expression> Elements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitArrayLiteralExpression(this);
        }

        public override string CodeDisplay => $"[{Elements.ToElidedList()}]";

        public override void Emit(Emitter emitter)
        {
            WriteItems(emitter, Elements, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ");
        }
    }
}
