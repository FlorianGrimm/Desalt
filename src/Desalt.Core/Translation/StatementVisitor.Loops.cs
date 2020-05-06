// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="StatementTranslator.StatementVisitor.Loops.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    internal sealed partial class StatementVisitor
    {
        //// ===========================================================================================================
        //// Loops
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a BreakStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsBreakStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitBreakStatement(BreakStatementSyntax node)
        {
            ITsBreakStatement translated = Factory.Break();
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ContinueStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsContinueStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitContinueStatement(ContinueStatementSyntax node)
        {
            ITsContinueStatement translated = Factory.Continue();
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ForEachStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsForOfStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitForEachStatement(ForEachStatementSyntax node)
        {
            // Translate the variable declaration - the 'x' in 'for (const x of )'.
            // NOTE: in TypeScript you can't actually have a type annotation on the left hand side of
            //       a for/of loop, so we just translate the variable name.
            ITsIdentifier declaration = Factory.Identifier(node.Identifier.Text);
            IExpressionTranslation rightSide = ExpressionTranslator.Translate(Context, node.Expression);

            var statement = TranslateAsSingleStatement(node.Statement);

            ITsForOfStatement translated = Factory.ForOf(
                VariableDeclarationKind.Const,
                declaration,
                rightSide.Expression,
                statement);

            return rightSide.AdditionalStatementsRequiredBeforeExpression.Add(translated);
        }

        /// <summary>
        /// Called when the visitor visits a ForStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsForStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitForStatement(ForStatementSyntax node)
        {
            TranslateForLoopInitializers(
                node,
                out ITsLexicalDeclaration? initializerWithLexicalDeclaration,
                out ITsExpression? initializer,
                out ImmutableArray<ITsStatementListItem> additionalInitializerStatements);
            var preLoopInitializers = new List<ITsStatementListItem>(additionalInitializerStatements);

            IExpressionTranslation? condition =
                node.Condition == null ? null : ExpressionTranslator.Translate(Context, node.Condition);
            if (condition != null)
            {
                preLoopInitializers.AddRange(condition.AdditionalStatementsRequiredBeforeExpression);
            }

            ITsStatement statement = TranslateAsSingleStatement(node.Statement);
            ITsExpression? incrementor = TranslateForLoopIncrementors(node.Incrementors, ref statement);

            ITsForStatement translated = initializerWithLexicalDeclaration != null
                ? Factory.For(initializerWithLexicalDeclaration, condition?.Expression, incrementor, statement)
                : Factory.For(initializer!, condition?.Expression, incrementor, statement);

            return preLoopInitializers.ToImmutableArray().Add(translated);
        }

        /// <summary>
        /// Translates the initializer part of a for loop `for(initializers, condition, incrementors)`, taking into
        /// account user-defined operators.
        /// </summary>
        /// <param name="node">The node to translate.</param>
        /// <param name="initializerWithLexicalDeclaration">
        /// The translated initializer if contains a declaration ( <c><![CDATA[for (int i = 0;...)]]></c>; null if
        /// the initializer doesn't contain a declaration, in which case <paramref name="initializer"/> will be defined.
        /// </param>
        /// <param name="initializer">
        /// The translated initializer if doesn't contain a declaration ( <c><![CDATA[for (i = 0;...)]]></c>; null
        /// if the initializer contains a declaration, in which case <paramref
        /// name="initializerWithLexicalDeclaration"/> will be defined.
        /// </param>
        /// <param name="additionalStatements">
        /// Any additional statements that need to be prepended to the translated `if` statement.
        /// </param>
        private void TranslateForLoopInitializers(
            ForStatementSyntax node,
            out ITsLexicalDeclaration? initializerWithLexicalDeclaration,
            out ITsExpression? initializer,
            out ImmutableArray<ITsStatementListItem> additionalStatements)
        {
            if (node.Declaration != null)
            {
                initializerWithLexicalDeclaration = TranslateVariableDeclaration(
                    node.Declaration,
                    out additionalStatements);
                initializer = null;
                return;
            }

            var preLoopStatements = new List<ITsStatementListItem>();

            // Translate all of the initializers and create a comma expression from them.
            var translatedInitializers = new List<ITsExpression>();
            foreach (ExpressionSyntax initializerNode in node.Initializers)
            {
                var initializerTranslation = ExpressionTranslator.Translate(Context, initializerNode);

                // If we have additional statements that need to be output before the initializer, then we have to move this
                // initializer and all preceding initializers outside of the for loop.
                if (initializerTranslation.AdditionalStatementsRequiredBeforeExpression.Any())
                {
                    // Add the already-translated initializers first.
                    preLoopStatements.AddRange(translatedInitializers.Select(x => x.ToStatement()));

                    // Then add the additional statements (temporary variable declarations).
                    preLoopStatements.AddRange(initializerTranslation.AdditionalStatementsRequiredBeforeExpression);

                    // And clear the already-translated initializers.
                    translatedInitializers.Clear();
                }

                translatedInitializers.Add(initializerTranslation.Expression);
            }

            initializerWithLexicalDeclaration = null;
            initializer = translatedInitializers.Count switch
            {
                0 => null,
                1 => translatedInitializers[0],
                _ => Factory.CommaExpression(translatedInitializers.ToArray()),
            };

            additionalStatements = preLoopStatements.ToImmutableArray();
        }

        /// <summary>
        /// Translates the incrementor part of a for loop `for(initializers, condition, incrementors)`, taking into
        /// account user-defined operators.
        /// </summary>
        /// <param name="incrementors">The incrementor list to translate.</param>
        /// <param name="forLoopStatement">
        /// A reference to the already-translated for loop statement, which could be changed in this method.
        /// </param>
        /// <returns>A <see cref="ITsExpression"/> representing the incrementor.</returns>
        private ITsExpression? TranslateForLoopIncrementors(
            SeparatedSyntaxList<ExpressionSyntax> incrementors,
            ref ITsStatement forLoopStatement)
        {
            var translatedIncrementors = new List<ITsExpression>();
            var blockStatements = new List<ITsStatementListItem>();

            foreach (ExpressionSyntax incrementorNode in incrementors)
            {
                var incrementorExpression = ExpressionTranslator.Translate(Context, incrementorNode);
                var translatedIncrementor = incrementorExpression.Expression;

                // If we have additional statements that need to be output before the incrementor, then we have to move the
                // incrementor inside the for loop right before the end.
                if (incrementorExpression.AdditionalStatementsRequiredBeforeExpression.Any())
                {
                    // Add the already-translated incrementors first.
                    blockStatements.AddRange(translatedIncrementors.Select(x => x.ToStatement()));

                    // Then add the additional statements (temporary variable declarations).
                    blockStatements.AddRange(incrementorExpression.AdditionalStatementsRequiredBeforeExpression);

                    // Then add the just-translated incrementor.
                    blockStatements.Add(translatedIncrementor.ToStatement());

                    // And clear the already-translated incrementor list.
                    translatedIncrementors.Clear();
                }
                else if (blockStatements.Any())
                {
                    // If we've already had to move the incrementors to the body, add it there.
                    blockStatements.Add(translatedIncrementor.ToStatement());
                }
                else
                {
                    // Incrementors (so far) can appear as part of the for loop incrementor list.
                    translatedIncrementors.Add(translatedIncrementor);
                }
            }

            ITsExpression? incrementor = translatedIncrementors.Count switch
            {
                0 => null,
                1 => translatedIncrementors[0],
                _ => Factory.CommaExpression(translatedIncrementors.ToArray())
            };

            // If we had to move the incrementors to the body, then convert the statement to a block statement if
            // needed, and then add the incrementors at the end of the block.
            forLoopStatement = AddAdditionalStatementsToExistingStatement(
                forLoopStatement,
                blockStatements,
                prependAdditionalStatements: false);

            return incrementor;
        }

        /// <summary>
        /// Called when the visitor visits a WhileStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsWhileStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitWhileStatement(WhileStatementSyntax node)
        {
            yield return TranslateWhileAndDoWhileLoops(node.Condition, node.Statement, isWhileLoop: true);
        }

        /// <summary>
        /// Called when the visitor visits a DoStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsDoWhileStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitDoStatement(DoStatementSyntax node)
        {
            yield return TranslateWhileAndDoWhileLoops(node.Condition, node.Statement, isWhileLoop: false);
        }

        private ITsStatementListItem TranslateWhileAndDoWhileLoops(
            ExpressionSyntax condition,
            StatementSyntax statement,
            bool isWhileLoop)
        {
            IExpressionTranslation whileConditionTranslation = ExpressionTranslator.Translate(Context, condition);
            ITsExpression whileCondition = whileConditionTranslation.Expression;

            // If there are any additional statements from translating the condition, we need to add those to the block.
            var blockStatements = new List<ITsStatementListItem>(
                whileConditionTranslation.AdditionalStatementsRequiredBeforeExpression);

            // Translate the statement.
            ITsStatement whileStatement = TranslateAsSingleStatement(statement);

            // If there were additional statements when translating the condition, then we need to pull them out of the
            // 'while' condition and into the loop before any translated statements.
            if (whileConditionTranslation.AdditionalStatementsRequiredBeforeExpression.Any())
            {
                // Add an explicit 'if' check here to exit the loop.
                blockStatements.Add(Factory.IfStatement(whileCondition, Factory.Break().ToBlock()));
                whileCondition = Factory.True;
            }

            whileStatement = AddAdditionalStatementsToExistingStatement(
                whileStatement,
                blockStatements,
                prependAdditionalStatements: isWhileLoop);

            ITsStatementListItem translated = isWhileLoop
                ? (ITsStatementListItem)Factory.While(whileCondition, whileStatement)
                : Factory.DoWhile(whileStatement, whileCondition);

            return translated;
        }

        /// <summary>
        /// Inserts the specified additional statements before or after the specified existing statement by creating a
        /// block statement if needed.
        /// </summary>
        /// <param name="existingStatement">The existing statement (or statements if this is a <see cref="ITsBlockStatement"/>).</param>
        /// <param name="additionalStatements">
        /// A list of additional statements to insert before or after the existing statement.
        /// </param>
        /// <param name="prependAdditionalStatements">
        /// If true, the additional statements will appear before the existing statement. If false, the additional
        /// statements will be appended after the existing statement.
        /// </param>
        /// <returns>
        /// If there are no additional statements, <paramref name="existingStatement"/>; otherwise a new <see
        /// cref="ITsBlockStatement"/> with the additional statements prepended (or appended) to the existing statement.
        /// </returns>
        private static ITsStatement AddAdditionalStatementsToExistingStatement(
            ITsStatement existingStatement,
            ICollection<ITsStatementListItem> additionalStatements,
            bool prependAdditionalStatements)
        {
            // Just return the original statement if there aren't any additional statements to add.
            if (additionalStatements.Count == 0)
            {
                return existingStatement;
            }

            var newStatements = new List<ITsStatementListItem>();

            // Add the existing statements to the block.
            if (existingStatement is ITsBlockStatement existingBlock)
            {
                newStatements.AddRange(existingBlock.Statements);
            }
            else
            {
                newStatements.Add(existingStatement);
            }

            // Add the additional statements to the block.
            if (prependAdditionalStatements)
            {
                newStatements.InsertRange(0, additionalStatements);
            }
            else
            {
                newStatements.AddRange(additionalStatements);
            }

            var blockStatement = Factory.Block(newStatements.ToArray());
            return blockStatement;
        }
    }
}
