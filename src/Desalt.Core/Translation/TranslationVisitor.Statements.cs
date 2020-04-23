// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Ast.Declarations;
    using Desalt.TypeScriptAst.Ast.Statements;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    internal sealed partial class TranslationVisitor
    {
        /// <summary>
        /// Returns the specified translated statement after any additional statements and clears the <see
        /// cref="_additionalStatementsNeededBeforeCurrentStatement"/> list.
        /// </summary>
        private IEnumerable<ITsStatementListItem> ReturnVisitResultPlusAdditionalStatements(
            ITsStatementListItem translated)
        {
            // Copy the additional statements here in case they change while we're iterating (it shouldn't but to be safe...).
            var copy = _additionalStatementsNeededBeforeCurrentStatement.ToImmutableArray();
            _additionalStatementsNeededBeforeCurrentStatement.Clear();

            foreach (ITsStatementListItem statement in copy)
            {
                yield return statement;
            }

            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a EmptyStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsEmptyStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitEmptyStatement(EmptyStatementSyntax node)
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
        public override IEnumerable<ITsAstNode> VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            // Special case: "naked" post increment/decrement user-defined operators are usually translated using
            // temporary variables. However, if they are the only statement (`x++`), it only needs to get translated as
            // `x = op_Increment(x)`.
            if (!TryTranslateUserDefinedOperator(
                node.Expression,
                isTopLevelExpressionInStatement: true,
                out ITsExpression? translatedExpression))
            {
                translatedExpression = VisitExpression(node.Expression);
            }

            ITsExpressionStatement translated = Factory.ExpressionStatement(translatedExpression);
            return ReturnVisitResultPlusAdditionalStatements(translated);
        }

        /// <summary>
        /// Called when the visitor visits a BlockSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsBlockStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitBlock(BlockSyntax node)
        {
            List<ITsStatementListItem> statements = VisitMultipleOfType<ITsStatementListItem>(node.Statements);
            yield return Factory.Block(statements.ToArray());
        }

        /// <summary>
        /// Called when the visitor visits a BracketedArgumentListSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitBracketedArgumentList(BracketedArgumentListSyntax node)
        {
            if (node.Arguments.Count > 1)
            {
                _diagnostics.Add(DiagnosticFactory.ElementAccessWithMoreThanOneExpressionNotAllowed(node));
            }

            if (node.Arguments.Count > 0)
            {
                yield return VisitSingleOfType<ITsArgument>(node.Arguments[0]).Argument;
            }
        }

        /// <summary>
        /// Called when the visitor visits a LocalDeclarationStatementSyntax node of the form `var x = y;`. A <see
        /// cref="LocalDeclarationStatementSyntax"/> contains a series of <see cref="VariableDeclaratorSyntax"/> for
        /// each variable declaration.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsLexicalDeclaration"/> and <see cref="_additionalStatementsNeededBeforeCurrentStatement"/>
        /// should be empty.
        /// </returns>
        public override IEnumerable<ITsAstNode> VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            // TODO: figure out if the variable ever changes to determine if it's const vs. let
            bool isConst = node.IsConst;

            // Get the type of all of the declarations.
            ITsType type = _typeTranslator.TranslateSymbol(
                node.Declaration.Type.GetTypeSymbol(_semanticModel),
                _typesToImport,
                _diagnostics,
                node.Declaration.Type.GetLocation);

            // Translate all of the VariableDeclaratorSyntax nodes.
            ITsLexicalDeclaration translated = TranslateDeclarationVariables(node.Declaration.Variables, isConst, type);
            return ReturnVisitResultPlusAdditionalStatements(translated);
        }

        /// <summary>
        /// Called when the visitor visits a <see cref="VariableDeclarationSyntax"/> node, which is a variable
        /// declaration within a blocked statement like 'using' or 'for'. A <see cref="VariableDeclarationSyntax"/>
        /// contains a series of <see cref="VariableDeclaratorSyntax"/> for each variable declaration.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsLexicalDeclaration"/> and <see cref="_additionalStatementsNeededBeforeCurrentStatement"/>
        /// could be populated with temporary variable declarations.
        /// </returns>
        public override IEnumerable<ITsAstNode> VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            // TODO: Determine whether this should be a const or let declaration
            const bool isConst = false;

            // Iterate over all of the variables and translate them.
            ITsLexicalDeclaration translated = TranslateDeclarationVariables(node.Variables, isConst);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a VariableDeclaratorSyntax node of the form `x [= y]`, which is an identifier
        /// followed by an optional initializer expression.
        /// </summary>
        /// <returns>An <see cref="ITsSimpleLexicalBinding"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            ITsIdentifier variableName = Factory.Identifier(node.Identifier.Text);

            ITsExpression? initializer = null;
            if (node.Initializer != null)
            {
                initializer = VisitExpression(node.Initializer);
            }

            // Note that we don't return the type here since in C# the type is declared first and
            // then it can have multiple variable declarators. The type will get bound in the parent
            // Visit callers.
            ITsSimpleLexicalBinding translated = Factory.SimpleLexicalBinding(variableName, initializer: initializer);
            yield return translated;
        }

        /// <summary>
        /// Translates a list of <see cref="VariableDeclaratorSyntax"/> nodes, taking into account user-defined
        /// operators. Used in both <see cref="VisitVariableDeclaration"/> and <see
        /// cref="VisitLocalDeclarationStatement"/>, where the only difference is whether a type definition is added to
        /// the <see cref="ITsLexicalDeclaration"/>.
        /// </summary>
        /// <param name="variables">The list of <see cref="VariableDeclaratorSyntax"/> nodes to translate.</param>
        /// <param name="isConst">TODO: Determine whether this should be a const or let declaration</param>
        /// <param name="type">An optional type to add to the lexical declaration.</param>
        /// <returns>
        /// A <see cref="ITsLexicalDeclaration"/> containing all of the translated initializers. Also, <see
        /// cref="_additionalStatementsNeededBeforeCurrentStatement"/> will contain any temporary variable declarations
        /// that need to be prepended before the current statement.
        /// </returns>
        private ITsLexicalDeclaration TranslateDeclarationVariables(
            SeparatedSyntaxList<VariableDeclaratorSyntax> variables,
            bool isConst,
            ITsType? type = null)
        {
            var preDeclarationStatements = new List<ITsStatementListItem>();
            var declarations = new List<ITsSimpleLexicalBinding>();

            ITsLexicalDeclaration CreateDeclaration()
            {
                ITsLexicalBinding[] bindings =
                    (type != null ? declarations.Select(x => x.WithVariableType(type)) : declarations)
                    .Cast<ITsLexicalBinding>()
                    .ToArray();

                return Factory.LexicalDeclaration(isConst, bindings);
            }

            foreach (var translatedBinding in variables.Select(VisitSingleOfType<ITsSimpleLexicalBinding>))
            {
                if (_additionalStatementsNeededBeforeCurrentStatement.Any())
                {
                    // Add any already-translated bindings to a new lexical declaration that should appear before the
                    // additional statements (temporary variable declarations).
                    if (declarations.Any())
                    {
                        ITsLexicalDeclaration combinedDeclaration = CreateDeclaration();
                        preDeclarationStatements.Add(combinedDeclaration);

                        declarations.Clear();
                    }

                    // Add the additional statements (temporary variable declarations).
                    preDeclarationStatements.AddRange(_additionalStatementsNeededBeforeCurrentStatement);
                    _additionalStatementsNeededBeforeCurrentStatement.Clear();
                }

                declarations.Add(translatedBinding);
            }

            _additionalStatementsNeededBeforeCurrentStatement.AddRange(preDeclarationStatements);
            ITsLexicalDeclaration declaration = CreateDeclaration();
            return declaration;
        }

        //// ===========================================================================================================
        //// Conditional Statements
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a IfStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsIfStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitIfStatement(IfStatementSyntax node)
        {
            var ifCondition = VisitExpression(node.Condition);
            var ifStatements = VisitMultipleOfType<ITsStatementListItem>(node.Statement);
            ITsStatement ifStatement = ifStatements.Count > 1
                ? Factory.Block(ifStatements.ToArray())
                : (ITsStatement)ifStatements[0];

            ITsStatement? elseStatement = node.Else == null ? null : VisitStatement(node.Else.Statement);

            ITsIfStatement translated = Factory.IfStatement(ifCondition, ifStatement, elseStatement);
            return ReturnVisitResultPlusAdditionalStatements(translated);
        }

        //// ===========================================================================================================
        //// Throw, Try/Catch, and Using Statements
        //// ===========================================================================================================

        private ITsIdentifier? _lastCatchIdentifier;

        /// <summary>
        /// Called when the visitor visits a ThrowStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsThrowStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitThrowStatement(ThrowStatementSyntax node)
        {
            ITsExpression expression;

            if (node.Expression == null)
            {
                if (_lastCatchIdentifier == null)
                {
                    ReportInternalError("_lastCatchIdentifier should have been set", node);
                    expression = Factory.Identifier("FIXME");
                }
                else
                {
                    expression = _lastCatchIdentifier;
                }
            }
            else
            {
                expression = VisitExpression(node.Expression);
            }

            ITsThrowStatement translated = Factory.Throw(expression);
            return ReturnVisitResultPlusAdditionalStatements(translated);
        }

        /// <summary>
        /// Called when the visitor visits a TryStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsTryStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitTryStatement(TryStatementSyntax node)
        {
            var tryBlock = VisitSingleOfType<ITsBlockStatement>(node.Block);

            // Translate only the first catch clause.
            if (node.Catches.Count > 1)
            {
                _diagnostics.Add(DiagnosticFactory.CatchClausesWithMoreThanOneParameterNotYetSupported(node));
            }

            CatchClauseSyntax? catchClause = node.Catches.Count > 0 ? node.Catches[0] : null;
            ITsIdentifier? catchParameter = null;
            if (catchClause != null)
            {
                CatchDeclarationSyntax? declaration = catchClause.Declaration;

                // C# can have `catch (Exception)` without an identifier, but we need one in
                // TypeScript, so generate a placeholder if necessary
                if (declaration?.Identifier.IsKind(SyntaxKind.None) == true)
                {
                    catchParameter = Factory.Identifier("e");
                }
                else if (declaration != null)
                {
                    catchParameter = Factory.Identifier(declaration.Identifier.Text);
                }

                // C# can have plain 'throw;' statements, but TypeScript cannot, so check for this
                // case and generate a placeholder if necessary
                else if (catchClause.Block.Statements.OfType<ThrowStatementSyntax>()
                    .Any(throwSyntax => throwSyntax.Expression == null))
                {
                    catchParameter = Factory.Identifier("e");
                }

                // cache this identifier temporarily since VisitThrowStatement will need it
                _lastCatchIdentifier = catchParameter;
            }

            ITsBlockStatement? catchBlock =
                catchClause != null ? VisitSingleOfType<ITsBlockStatement>(catchClause.Block) : null;

            // translate the finally block if present
            ITsBlockStatement? finallyBlock =
                node.Finally != null ? VisitSingleOfType<ITsBlockStatement>(node.Finally.Block) : null;

            // translate the try/catch/finally statement
            ITsTryStatement translated;
            if (catchBlock != null && finallyBlock != null)
            {
                translated = Factory.TryCatchFinally(tryBlock, catchParameter, catchBlock, finallyBlock);
            }
            else if (catchBlock != null)
            {
                translated = Factory.TryCatch(tryBlock, catchParameter, catchBlock);
            }
            else if (finallyBlock != null)
            {
                translated = Factory.TryFinally(tryBlock, finallyBlock);
            }
            else
            {
                translated = Factory.Try(tryBlock);
            }

            // reset this temporary variable
            _lastCatchIdentifier = null;

            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a UsingStatementSyntax node.
        /// </summary>
        /// <returns>A <see cref="ITsBlockStatement"/> representing a wrapped try/finally block.</returns>
        public override IEnumerable<ITsAstNode> VisitUsingStatement(UsingStatementSyntax node)
        {
            var statements = new List<ITsStatementListItem>();
            string? reservedTemporaryVariable = null;
            ITsIdentifier variableNameIdentifier;

            // Case 1: when there's a declaration
            // ----------------------------------
            // C#:
            // using (var c = new C()) {}
            //
            // TypeScript:
            // {
            //   const c = new C();
            //   try {
            //   } finally {
            //     if (c) {
            //       c.dispose();
            //     }
            //   }
            // }
            if (node.Declaration != null)
            {
                // Translate the declaration.
                var declaration = VisitSingleOfType<ITsLexicalDeclaration>(node.Declaration);
                declaration = declaration.WithIsConst(true);

                // Get the type of the declaration.
                ITypeSymbol? typeSymbol = _semanticModel.GetTypeInfo(node.Declaration.Type).Type;
                ITsType? declarationType = typeSymbol == null
                    ? null
                    : _typeTranslator.TranslateSymbol(
                        typeSymbol,
                        _typesToImport,
                        _diagnostics,
                        node.Declaration.Type.GetLocation);

                // Fix up all of the declarations to add the type.
                if (declarationType != null)
                {
                    declaration = declaration.WithDeclarations(
                        declaration.Declarations.Cast<ITsSimpleLexicalBinding>()
                            .Select(binding => binding.WithVariableType(declarationType)));
                }

                // Add any additional statements before this statement.
                statements.AddRange(_additionalStatementsNeededBeforeCurrentStatement);
                _additionalStatementsNeededBeforeCurrentStatement.Clear();

                statements.Add(declaration);

                variableNameIdentifier = declaration.Declarations.Cast<ITsSimpleLexicalBinding>().First().VariableName;
            }

            // Case 2: when there's an expression
            // ----------------------------------
            // C#:
            // using (c.GetDispose()) {}
            //
            // TypeScript:
            // {
            //   const $using1 = c.getDispose();
            //   try {
            //   } finally {
            //     if ($using1) {
            //       $using1.dispose();
            //     }
            //   }
            // }
            else
            {
                // Translate the expression.
                if (!TryTranslateUserDefinedOperator(
                    node.Expression!,
                    isTopLevelExpressionInStatement: true,
                    out ITsExpression? expression))
                {
                    expression = VisitExpression(node.Expression!);
                }

                // Try to find the type of the expression.
                ITypeSymbol? expressionTypeSymbol =
                    _semanticModel.GetTypeInfo(node.Expression, _cancellationToken).ConvertedType;

                ITsType? variableType = expressionTypeSymbol == null
                    ? null
                    : _typeTranslator.TranslateSymbol(
                        expressionTypeSymbol,
                        _typesToImport,
                        _diagnostics,
                        node.Expression!.GetLocation);

                // Create a temporary variable name to hold the expression.
                reservedTemporaryVariable = _temporaryVariableAllocator.Reserve("$using");
                variableNameIdentifier = Factory.Identifier(reservedTemporaryVariable);

                // Assign the expression to the temporary variable.
                ITsLexicalDeclaration declaration = Factory.LexicalDeclaration(
                    isConst: true,
                    declarations: new ITsLexicalBinding[]
                    {
                        Factory.SimpleLexicalBinding(variableNameIdentifier, variableType, expression)
                    });

                // Add any additional statements before this statement.
                statements.AddRange(_additionalStatementsNeededBeforeCurrentStatement);
                _additionalStatementsNeededBeforeCurrentStatement.Clear();

                statements.Add(declaration);
            }

            // Create the try block, which is the using block.
            var usingBlock = VisitStatement(node.Statement);
            if (usingBlock is ITsBlockStatement tryBlock)
            {
                // Remove any trailing newlines.
                tryBlock = tryBlock.WithTrailingTrivia();
            }
            else
            {
                tryBlock = Factory.Block(usingBlock);
            }

            // Create the finally block, which disposes the object.
            ITsBlockStatement finallyBlock = Factory.Block(
                Factory.IfStatement(
                    variableNameIdentifier,
                    Factory.Block(Factory.Call(Factory.MemberDot(variableNameIdentifier, "dispose")).ToStatement())));

            // Create the try/finally statement.
            ITsTryStatement tryFinally = Factory.TryFinally(tryBlock, finallyBlock);
            statements.Add(tryFinally);

            // Wrap the declaration inside of a block so that scoping will be correct.
            ITsBlockStatement translated = Factory.Block(statements.ToArray()).WithTrailingTrivia(Factory.Newline);

            // Return the temporary variable to the allocator.
            if (reservedTemporaryVariable != null)
            {
                _temporaryVariableAllocator.Return(reservedTemporaryVariable);
            }

            yield return translated;
        }

        //// ===========================================================================================================
        //// Loops
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a BreakStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsBreakStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitBreakStatement(BreakStatementSyntax node)
        {
            ITsBreakStatement translated = Factory.Break();
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ContinueStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsContinueStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitContinueStatement(ContinueStatementSyntax node)
        {
            ITsContinueStatement translated = Factory.Continue();
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ForEachStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsForOfStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitForEachStatement(ForEachStatementSyntax node)
        {
            // Translate the variable declaration - the 'x' in 'for (const x of )'.
            // NOTE: in TypeScript you can't actually have a type annotation on the left hand side of
            //       a for/of loop, so we just translate the variable name.
            ITsIdentifier declaration = Factory.Identifier(node.Identifier.Text);

            if (!TryTranslateUserDefinedOperator(
                node.Expression,
                isTopLevelExpressionInStatement: true,
                out ITsExpression? rightSide))
            {
                rightSide = VisitExpression(node.Expression);
            }

            var statement = VisitStatement(node.Statement);

            ITsForOfStatement translated = Factory.ForOf(
                VariableDeclarationKind.Const,
                declaration,
                rightSide,
                statement);

            return ReturnVisitResultPlusAdditionalStatements(translated);
        }

        /// <summary>
        /// Called when the visitor visits a ForStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsForStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitForStatement(ForStatementSyntax node)
        {
            ITsLexicalDeclaration? initializerWithLexicalDeclaration = null;
            ITsExpression? initializer = null;
            TranslateForLoopInitializers(node, ref initializerWithLexicalDeclaration, ref initializer);
            var preLoopInitializers = new List<ITsStatementListItem>(_additionalStatementsNeededBeforeCurrentStatement);
            _additionalStatementsNeededBeforeCurrentStatement.Clear();

            ITsExpression? condition = node.Condition == null ? null : VisitExpression(node.Condition);
            preLoopInitializers.AddRange(_additionalStatementsNeededBeforeCurrentStatement);
            _additionalStatementsNeededBeforeCurrentStatement.Clear();

            ITsStatement statement = VisitStatement(node.Statement);
            ITsExpression? incrementor = TranslateForLoopIncrementors(node.Incrementors, ref statement);

            ITsForStatement translated = initializerWithLexicalDeclaration != null
                ? Factory.For(initializerWithLexicalDeclaration, condition, incrementor, statement)
                : Factory.For(initializer!, condition, incrementor, statement);

            _additionalStatementsNeededBeforeCurrentStatement.AddRange(preLoopInitializers);
            return ReturnVisitResultPlusAdditionalStatements(translated);
        }

        /// <summary>
        /// Translates the initializer part of a for loop `for(initializers, condition, incrementors)`, taking into
        /// account user-defined operators.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="initializerWithLexicalDeclaration"></param>
        /// <param name="initializer"></param>
        private void TranslateForLoopInitializers(
            ForStatementSyntax node,
            ref ITsLexicalDeclaration? initializerWithLexicalDeclaration,
            ref ITsExpression? initializer)
        {
            var preLoopStatements = new List<ITsStatementListItem>();

            if (node.Declaration != null)
            {
                initializerWithLexicalDeclaration = VisitSingleOfType<ITsLexicalDeclaration>(node.Declaration);
                return;
            }

            // Translate all of the initializers and create a comma expression from them.
            var translatedInitializers = new List<ITsExpression>();
            foreach (ExpressionSyntax initializerNode in node.Initializers)
            {
                // Translate the initializer either by converting it to a user-defined operator function call or a
                // standard expression.
                if (!TryTranslateUserDefinedOperator(
                    initializerNode,
                    isTopLevelExpressionInStatement: true,
                    out initializer))
                {
                    initializer = VisitExpression(initializerNode);
                }

                // If we have additional statements that need to be output before the initializer, then we have to move this
                // initializer and all preceding initializers outside of the for loop.
                if (_additionalStatementsNeededBeforeCurrentStatement.Any())
                {
                    // Add the already-translated initializers first.
                    preLoopStatements.AddRange(translatedInitializers.Select(x => x.ToStatement()));

                    // Then add the additional statements (temporary variable declarations).
                    preLoopStatements.AddRange(_additionalStatementsNeededBeforeCurrentStatement);
                    _additionalStatementsNeededBeforeCurrentStatement.Clear();

                    // And clear the already-translated initializers.
                    translatedInitializers.Clear();
                }

                translatedInitializers.Add(initializer);
            }

            initializer = translatedInitializers.Count switch
            {
                0 => null,
                1 => translatedInitializers[0],
                _ => Factory.CommaExpression(translatedInitializers.ToArray()),
            };

            _additionalStatementsNeededBeforeCurrentStatement.AddRange(preLoopStatements);
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
                // Translate the incrementor either by converting it to a user-defined operator function call or a
                // standard expression.
                if (!TryTranslateUserDefinedOperator(
                    incrementorNode,
                    isTopLevelExpressionInStatement: true,
                    out ITsExpression? translatedIncrementor))
                {
                    translatedIncrementor = VisitExpression(incrementorNode);
                }

                // If we have additional statements that need to be output before the incrementor, then we have to move the
                // incrementor inside the for loop right before the end.
                if (_additionalStatementsNeededBeforeCurrentStatement.Any())
                {
                    // Add the already-translated incrementors first.
                    blockStatements.AddRange(translatedIncrementors.Select(x => x.ToStatement()));

                    // Then add the additional statements (temporary variable declarations).
                    blockStatements.AddRange(_additionalStatementsNeededBeforeCurrentStatement);
                    _additionalStatementsNeededBeforeCurrentStatement.Clear();

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
        public override IEnumerable<ITsAstNode> VisitWhileStatement(WhileStatementSyntax node)
        {
            yield return TranslateWhileAndDoWhileLoops(node.Condition, node.Statement, isWhileLoop: true);
        }

        /// <summary>
        /// Called when the visitor visits a DoStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsDoWhileStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitDoStatement(DoStatementSyntax node)
        {
            yield return TranslateWhileAndDoWhileLoops(node.Condition, node.Statement, isWhileLoop: false);
        }

        private ITsStatementListItem TranslateWhileAndDoWhileLoops(
            ExpressionSyntax condition,
            StatementSyntax statement,
            bool isWhileLoop)
        {
            if (!TryTranslateUserDefinedOperator(
                condition,
                isTopLevelExpressionInStatement: true,
                out ITsExpression? whileCondition))
            {
                whileCondition = VisitExpression(condition);
            }

            // If there are any additional statements from translating the condition, we need to add those to the block.
            var blockStatements = new List<ITsStatementListItem>(_additionalStatementsNeededBeforeCurrentStatement);
            bool conditionHasAdditionalStatements = _additionalStatementsNeededBeforeCurrentStatement.Count > 0;
            _additionalStatementsNeededBeforeCurrentStatement.Clear();

            // Translate the statement.
            var whileStatement = VisitStatement(statement);

            // If there were additional statements when translating the condition, then we need to pull them out of the
            // 'while' condition and into the loop before any translated statements.
            if (conditionHasAdditionalStatements)
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

        //// ===========================================================================================================
        //// Switch Statement
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a SwitchStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsSwitchStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitSwitchStatement(SwitchStatementSyntax node)
        {
            if (!TryTranslateUserDefinedOperator(
                node.Expression,
                isTopLevelExpressionInStatement: true,
                out ITsExpression? condition))
            {
                condition = VisitExpression(node.Expression);
            }

            var preSwitchStatements = new List<ITsStatementListItem>(_additionalStatementsNeededBeforeCurrentStatement);
            _additionalStatementsNeededBeforeCurrentStatement.Clear();

            var clauses = VisitMultipleOfType<ITsCaseOrDefaultClause>(node.Sections);

            ITsSwitchStatement translated = Factory.Switch(condition, clauses.ToArray());

            _additionalStatementsNeededBeforeCurrentStatement.AddRange(preSwitchStatements);
            return ReturnVisitResultPlusAdditionalStatements(translated);
        }

        /// <summary>
        /// Called when the visitor visits a SwitchSectionSyntax node.
        /// </summary>
        /// <returns>An enumerable of <see cref="ITsCaseOrDefaultClause"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitSwitchSection(SwitchSectionSyntax node)
        {
            List<ITsCaseOrDefaultClause> labels = VisitMultipleOfType<ITsCaseOrDefaultClause>(node.Labels);
            List<ITsStatementListItem> statements = VisitMultipleOfType<ITsStatementListItem>(node.Statements);

            // Attach the statements to the last label
            labels[^1] = labels[^1].WithStatements(statements);

            return labels;
        }

        /// <summary>
        /// Called when the visitor visits a CaseSwitchLabelSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsCaseClause"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            var expression = VisitExpression(node.Value);
            ITsCaseClause translated = Factory.CaseClause(expression);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a DefaultSwitchLabelSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsDefaultClause"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
        {
            ITsDefaultClause translated = Factory.DefaultClause();
            yield return translated;
        }
    }
}
