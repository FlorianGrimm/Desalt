// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="StatementTranslator.StatementVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    internal sealed partial class StatementVisitor
        : BaseTranslationVisitor<StatementSyntax, IEnumerable<ITsStatementListItem>>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public StatementVisitor(TranslationContext context)
            : base(context)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public ITsBlockStatement TranslateBlock(BlockSyntax node)
        {
            var statements = node.Statements.SelectMany(Visit);
            return Factory.Block(statements.ToArray());
        }

        /// <summary>
        /// Translates the specified statement. If multiple statements are returned, then they are wrapped inside of
        /// a <see cref="ITsBlockStatement"/>. If a single statement is translated, it is preserved.
        /// </summary>
        private ITsStatement TranslateAsSingleStatement(StatementSyntax node)
        {
            var statements = Visit(node).ToArray();
            var statement = statements.Length > 1 ? Factory.Block(statements) : (ITsStatement)statements[0];
            return statement;
        }

        //// ===========================================================================================================
        //// Simple Statements
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a EmptyStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsEmptyStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitEmptyStatement(EmptyStatementSyntax node)
        {
            yield return Factory.EmptyStatement;
        }

        /// <summary>
        /// Called when the visitor visits a ExpressionStatementSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsExpressionStatement"/> or a series of <see cref="ITsExpressionStatement"/> if the original
        /// expression expands to multiple statements.
        /// </returns>
        public override IEnumerable<ITsStatementListItem> VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var expressionTranslation = ExpressionTranslator.Translate(Context, node.Expression);
            ITsExpressionStatement translated = Factory.ExpressionStatement(expressionTranslation.Expression);
            return expressionTranslation.AdditionalStatementsRequiredBeforeExpression.Add(translated);
        }

        /// <summary>
        /// Called when the visitor visits a BlockSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsBlockStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitBlock(BlockSyntax node)
        {
            yield return TranslateBlock(node);
        }

        /// <summary>
        /// Called when the visitor visits a ReturnStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsReturnStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitReturnStatement(ReturnStatementSyntax node)
        {
            ITsExpression? expression = null;
            if (node.Expression != null)
            {
                var expressionTranslation = ExpressionTranslator.Translate(Context, node.Expression);
                expression = expressionTranslation.Expression;
                foreach (var additionalStatement in expressionTranslation.AdditionalStatementsRequiredBeforeExpression)
                {
                    yield return additionalStatement;
                }
            }

            ITsReturnStatement translated = Factory.Return(expression);
            yield return translated;
        }

        //// ===========================================================================================================
        //// Conditional Statements
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a IfStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsIfStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitIfStatement(IfStatementSyntax node)
        {
            IExpressionTranslation ifCondition = ExpressionTranslator.Translate(Context, node.Condition);
            var ifStatement = TranslateAsSingleStatement(node.Statement);

            var elseStatements = node.Else == null ? null : Visit(node.Else.Statement).ToArray();
            ITsStatement? elseStatement = elseStatements?.Length > 1
                ? Factory.Block(elseStatements)
                : elseStatements?.First() as ITsStatement;

            ITsIfStatement translated = Factory.IfStatement(ifCondition.Expression, ifStatement, elseStatement);
            return ifCondition.AdditionalStatementsRequiredBeforeExpression.Add(translated);
        }

        //// ===========================================================================================================
        //// Switch Statement
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a SwitchStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsSwitchStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitSwitchStatement(SwitchStatementSyntax node)
        {
            var conditionTranslation = ExpressionTranslator.Translate(Context, node.Expression);
            var clauses = node.Sections.SelectMany(TranslateSwitchSection);

            ITsSwitchStatement translated = Factory.Switch(conditionTranslation.Expression, clauses.ToArray());
            return conditionTranslation.AdditionalStatementsRequiredBeforeExpression.Add(translated);
        }

        /// <summary>
        /// Called when the visitor visits a SwitchSectionSyntax node.
        /// </summary>
        /// <returns>An enumerable of <see cref="ITsCaseOrDefaultClause"/>.</returns>
        private IEnumerable<ITsCaseOrDefaultClause> TranslateSwitchSection(SwitchSectionSyntax node)
        {
            ITsCaseOrDefaultClause[] labels = node.Labels.Select(TranslateSwitchLabel).ToArray();
            var statements = node.Statements.SelectMany(Visit).ToImmutableArray();

            // Attach the statements to the last label.
            if (labels[^1] is ITsCaseClause caseClause)
            {
                labels[^1] = caseClause.WithStatements(statements);
            }
            else
            {
                labels[^1] = ((ITsDefaultClause)labels[^1]).WithStatements(statements);
            }

            return labels;
        }

        private ITsCaseOrDefaultClause TranslateSwitchLabel(SwitchLabelSyntax node)
        {
            return node switch
            {
                CaseSwitchLabelSyntax caseLabel => TranslateCaseSwitchLabel(caseLabel),
                DefaultSwitchLabelSyntax _ => Factory.DefaultClause(),
                _ => throw new InvalidOperationException(
                    DiagnosticFactory.InternalError("Unknown SwitchLabelSyntax type", node.GetLocation()).ToString())
            };
        }

        private ITsCaseClause TranslateCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            IExpressionTranslation expressionTranslation = ExpressionTranslator.Translate(Context, node.Value);
            if (expressionTranslation.AdditionalStatementsRequiredBeforeExpression.Any())
            {
                Context.ReportInternalError(
                    "A switch case label cannot have a non-constant expression, which means that it can't have " +
                    "any additional statements associated with it",
                    node);
            }

            ITsCaseClause translated = Factory.CaseClause(expressionTranslation.Expression);
            return translated;
        }
    }
}
