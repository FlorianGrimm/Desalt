// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBlockStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a block statement of the form '{ statements }'.
    /// </summary>
    internal class TsBlockStatement : AstNode, ITsBlockStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsBlockStatement(IEnumerable<ITsStatementListItem> statements)
        {
            Statements = statements?.ToImmutableArray() ?? ImmutableArray<ITsStatementListItem>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsStatementListItem> Statements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitBlockStatement(this);

        public override string CodeDisplay => $"{{ {Statements.ToElidedList(Environment.NewLine)} }}";

        protected override void EmitInternal(Emitter emitter) => emitter.WriteBlock(Statements, skipNewlines: true);
    }
}
