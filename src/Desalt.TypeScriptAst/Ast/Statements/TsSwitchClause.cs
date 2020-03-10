// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSwitchClause.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a case clause in a switch statement.
    /// </summary>
    internal class TsSwitchClause : TsAstNode, ITsCaseClause, ITsDefaultClause
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsSwitchClause(ITsExpression expression, IEnumerable<ITsStatementListItem> statements)
        {
            Expression = expression;
            Statements = statements?.ToImmutableArray() ?? ImmutableArray<ITsStatementListItem>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Expression { get; }
        public ImmutableArray<ITsStatementListItem> Statements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static TsSwitchClause Case(ITsExpression expression, IEnumerable<ITsStatementListItem> statements)
        {
            return new TsSwitchClause(expression ?? throw new ArgumentNullException(nameof(expression)), statements);
        }

        public static TsSwitchClause Default(IEnumerable<ITsStatementListItem> statements)
        {
            return new TsSwitchClause(expression: null, statements: statements);
        }

        public override void Accept(TsVisitor visitor)
        {
            if (Expression != null)
            {
                visitor.VisitCaseClause(this);
            }
            else
            {
                visitor.VisitDefaultClause(this);
            }
        }

        public override string CodeDisplay =>
            Expression == null ? "default: " : "case {Expression}: " + $"{Statements.ToElidedList()}";

        protected override void EmitInternal(Emitter emitter)
        {
            if (Expression != null)
            {
                emitter.Write("case ");
                Expression.Emit(emitter);
            }
            else
            {
                emitter.Write("default");
            }
            emitter.WriteLine(":");

            emitter.IndentLevel++;
            foreach (ITsStatementListItem statement in Statements)
            {
                statement.Emit(emitter);
            }
            emitter.IndentLevel--;
        }
    }

    public static class TsSwitchClauseExtensions
    {
        public static ITsCaseOrDefaultClause WithStatements(
            this ITsCaseOrDefaultClause instance,
            IEnumerable<ITsStatementListItem> value)
        {
            var valueAsImmutableArray = value?.ToImmutableArray() ?? ImmutableArray<ITsStatementListItem>.Empty;
            if (instance.Statements.Equals(valueAsImmutableArray))
            {
                return instance;
            }

            switch (instance)
            {
                case TsSwitchClause switchClause when switchClause.Expression != null:
                    return TsSwitchClause.Case(switchClause.Expression, valueAsImmutableArray);

                case TsSwitchClause switchClause when switchClause.Expression == null:
                    return TsSwitchClause.Default(valueAsImmutableArray);

                case ITsCaseClause caseClause:
                    return TsSwitchClause.Case(caseClause.Expression, valueAsImmutableArray);

                default:
                    return TsSwitchClause.Default(valueAsImmutableArray);
            }
        }
    }
}
