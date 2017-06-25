// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5BlockStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a block, containing other statements.
    /// </summary>
    public sealed class Es5BlockStatement : Es5AstNode, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5BlockStatement(IEnumerable<IEs5Statement> statements)
        {
            Statements = statements?.ToImmutableArray() ?? ImmutableArray<IEs5Statement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<IEs5Statement> Statements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitBlockStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitBlockStatement(this);
        }

        public override string ToCodeDisplay() => $"Block, Statements.Length = {{ {Statements.Length} }}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            WriteBlock(writer, Statements);
        }
    }
}
