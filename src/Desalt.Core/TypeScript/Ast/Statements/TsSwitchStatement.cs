// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSwitchStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a switch statement of the form 'switch (condition) { case x: statement; default: statement; }'.
    /// </summary>
    internal class TsSwitchStatement : AstNode<TsVisitor>, ITsSwitchStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsSwitchStatement(ITsExpression condition, IEnumerable<ITsCaseOrDefaultClause> clauses)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Clauses = clauses?.ToImmutableArray() ?? ImmutableArray<ITsCaseOrDefaultClause>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Condition { get; }
        public ImmutableArray<ITsCaseOrDefaultClause> Clauses { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitSwitchStatement(this);

        public override string CodeDisplay => $"switch ({Condition}) {{ {Clauses.ToElidedList()} }}";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("switch (");
            Condition.Emit(emitter);
            emitter.WriteLine(") {");

            emitter.IndentLevel++;
            for (int i = 0; i < Clauses.Length; i++)
            {
                ITsCaseOrDefaultClause clause = Clauses[i];
                clause.Emit(emitter);

                // don't write newlines between empty clauses or after the last item
                if (!clause.Statements.IsEmpty && i < Clauses.Length - 1)
                {
                    emitter.WriteLineWithoutIndentation();
                }
            }
            emitter.IndentLevel--;
            emitter.WriteLine("}");
        }
    }
}
