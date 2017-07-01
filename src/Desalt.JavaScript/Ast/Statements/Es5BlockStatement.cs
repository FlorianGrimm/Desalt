// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5BlockStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a block, containing other statements.
    /// </summary>
    public sealed class Es5BlockStatement : AstNode<Es5Visitor>, IEs5Statement
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

        public override void Accept(Es5Visitor visitor) => visitor.VisitBlockStatement(this);

        public override string CodeDisplay => $"{{ {Statements.ToElidedList(Environment.NewLine)} }}";

        public override void Emit(Emitter emitter) => emitter.WriteBlock(Statements, skipNewlines: true);
    }
}
