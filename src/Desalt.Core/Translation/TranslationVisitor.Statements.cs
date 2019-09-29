// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using TypeScriptAst.Ast;
    using TypeScriptAst.Ast.Declarations;
    using TypeScriptAst.Ast.Statements;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    internal sealed partial class TranslationVisitor
    {
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
        /// <returns>An <see cref="ITsExpressionStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var expression = (ITsExpression)Visit(node.Expression).Single();
            ITsExpressionStatement translated = Factory.ExpressionStatement(expression);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a BlockSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsBlockStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitBlock(BlockSyntax node)
        {
            ITsStatementListItem[] statements =
                node.Statements.SelectMany(Visit).Cast<ITsStatementListItem>().ToArray();
            return Factory.Block(statements).ToSingleEnumerable();
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

            return node.Arguments.Count == 0
                ? null
                : ((ITsArgument)Visit(node.Arguments[0]).Single()).Argument.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a LocalDeclarationStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsLexicalDeclaration"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            // TODO: figure out if the variable ever changes to determine if it's const vs. let
            bool isConst = node.IsConst;

            // get the type of all of the declarations
            var typeSymbol = node.Declaration.Type.GetTypeSymbol(_semanticModel);
            ITsType type = _typeTranslator.TranslateSymbol(
                typeSymbol,
                _typesToImport,
                _diagnostics,
                node.Declaration.Type.GetLocation);

            ITsSimpleLexicalBinding[] declarations = node.Declaration.Variables.SelectMany(Visit)
                .Cast<ITsSimpleLexicalBinding>()
                .Select(binding => binding.WithVariableType(type))
                .ToArray();

            // ReSharper disable once CoVariantArrayConversion
            ITsLexicalDeclaration translated = Factory.LexicalDeclaration(isConst, declarations);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a VariableDeclaratorSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsSimpleLexicalBinding"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var variableName = Factory.Identifier(node.Identifier.Text);

            ITsExpression initializer = null;
            if (node.Initializer != null)
            {
                initializer = (ITsExpression)Visit(node.Initializer).First();
            }

            // Note that we don't return the type here since in C# the type is declared first and
            // then it can have multiple variable declarators. The type will get bound in the parent
            // Visit callers.
            ITsSimpleLexicalBinding translated = Factory.SimpleLexicalBinding(variableName, initializer: initializer);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a VariableDeclarationSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsLexicalDeclaration"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            // TODO: Determine whether this should be a const or let declaration
            const bool isConst = false;

            // iterate over all of the variables and translate them
            var lexicalBindings = node.Variables.SelectMany(Visit).Cast<ITsLexicalBinding>();

            ITsLexicalDeclaration translated = Factory.LexicalDeclaration(isConst, lexicalBindings.ToArray());
            yield return translated;
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
            var ifCondition = (ITsExpression)Visit(node.Condition).Single();
            var ifStatement = (ITsStatement)Visit(node.Statement).Single();
            var elseStatement = node.Else == null ? null : (ITsStatement)Visit(node.Else.Statement).Single();

            ITsIfStatement translated = Factory.IfStatement(ifCondition, ifStatement, elseStatement);
            yield return translated;
        }

        //// ===========================================================================================================
        //// Throw, Try/Catch, and Using Statements
        //// ===========================================================================================================

        private ITsIdentifier _lastCatchIdentifier;

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
                    _diagnostics.Add(
                        DiagnosticFactory.InternalError(
                            "_lastCatchIdentifier should have been set",
                            node.GetLocation()));
                    expression = Factory.Identifier("FIXME");
                }
                else
                {
                    expression = _lastCatchIdentifier;
                }
            }
            else
            {
                expression = (ITsExpression)Visit(node.Expression).Single();
            }

            ITsThrowStatement translated = Factory.Throw(expression);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a TryStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsTryStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitTryStatement(TryStatementSyntax node)
        {
            var tryBlock = (ITsBlockStatement)Visit(node.Block).Single();

            // translate only the first catch clause
            if (node.Catches.Count > 1)
            {
                _diagnostics.Add(DiagnosticFactory.CatchClausesWithMoreThanOneParameterNotYetSupported(node));
            }

            bool hasCatch = node.Catches.Count > 0;
            CatchClauseSyntax catchClause = hasCatch ? node.Catches[0] : null;
            ITsIdentifier catchParameter = null;
            if (hasCatch)
            {
                CatchDeclarationSyntax declaration = catchClause.Declaration;

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

            var catchBlock = hasCatch ? (ITsBlockStatement)Visit(catchClause.Block).Single() : null;

            // translate the finally block if present
            bool hasFinally = node.Finally != null;
            var finallyBlock = hasFinally ? (ITsBlockStatement)Visit(node.Finally.Block).Single() : null;

            // translate the try/catch/finally statement
            ITsTryStatement translated;
            if (hasCatch && hasFinally)
            {
                translated = Factory.TryCatchFinally(tryBlock, catchParameter, catchBlock, finallyBlock);
            }
            else if (hasCatch)
            {
                translated = Factory.TryCatch(tryBlock, catchParameter, catchBlock);
            }
            else if (hasFinally)
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
            string reservedTemporaryVariable = null;
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
                // translate the declaration
                var declaration = (ITsLexicalDeclaration)Visit(node.Declaration).Single();
                declaration = declaration.WithIsConst(true);

                // get the type of the declaration
                ITypeSymbol typeSymbol = _semanticModel.GetTypeInfo(node.Declaration.Type).Type;
                ITsType declarationType = typeSymbol == null
                    ? null
                    : _typeTranslator.TranslateSymbol(
                        typeSymbol,
                        _typesToImport,
                        _diagnostics,
                        node.Declaration.Type.GetLocation);

                // fixup all of the declarations to add the type
                if (declarationType != null)
                {
                    declaration = declaration.WithDeclarations(
                        declaration.Declarations.Cast<ITsSimpleLexicalBinding>()
                            .Select(binding => binding.WithVariableType(declarationType)));
                }

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
                // translate the expression
                var expression = (ITsExpression)Visit(node.Expression).Single();

                // try to find the type of the expression
                ITypeSymbol expressionTypeSymbol =
                    _semanticModel.GetTypeInfo(node.Expression, _cancellationToken).ConvertedType;

                ITsType variableType = expressionTypeSymbol == null
                    ? null
                    : _typeTranslator.TranslateSymbol(
                        expressionTypeSymbol,
                        _typesToImport,
                        _diagnostics,
                        node.Expression.GetLocation);

                // create a temporary variable name to hold the expression
                reservedTemporaryVariable = _temporaryVariableAllocator.Reserve("$using");
                variableNameIdentifier = Factory.Identifier(reservedTemporaryVariable);

                // assign the expression to the temporary variable
                ITsLexicalDeclaration declaration = Factory.LexicalDeclaration(
                    isConst: true,
                    declarations: new ITsLexicalBinding[]
                    {
                        Factory.SimpleLexicalBinding(variableNameIdentifier, variableType, expression)
                    });

                statements.Add(declaration);
            }

            // create the try block, which is the using block
            var usingBlock = (ITsStatement)Visit(node.Statement).Single();
            if (usingBlock is ITsBlockStatement tryBlock)
            {
                // remove any trailing newlines
                tryBlock = tryBlock.WithTrailingTrivia(new ITsAstTriviaNode[0]);
            }
            else
            {
                tryBlock = Factory.Block(usingBlock);
            }

            // create the finally block, which disposes the object
            ITsBlockStatement finallyBlock = Factory.Block(
                Factory.IfStatement(
                    variableNameIdentifier,
                    Factory.Block(Factory.Call(Factory.MemberDot(variableNameIdentifier, "dispose")).ToStatement())));

            // create the try/finally statement
            var tryFinally = Factory.TryFinally(tryBlock, finallyBlock);
            statements.Add(tryFinally);

            // wrap the declaration inside of a block so that scoping will be correct
            ITsBlockStatement translated = Factory.Block(statements.ToArray()).WithTrailingTrivia(Factory.Newline);

            // return the temporary variable to the allocator
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
            // translate the variable declaration - the 'x' in 'for (const x of )'
            // NOTE: in TypeScript you can't actually have a type annotation on the left hand side of
            //       a for/of loop, so we just translate the variable name.
            ITsIdentifier declaration = Factory.Identifier(node.Identifier.Text);
            var rightSide = (ITsExpression)Visit(node.Expression).Single();
            var statement = (ITsStatement)Visit(node.Statement).Single();

            ITsForOfStatement translated = Factory.ForOf(
                VariableDeclarationKind.Const,
                declaration,
                rightSide,
                statement);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ForStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsForStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitForStatement(ForStatementSyntax node)
        {
            ITsLexicalDeclaration initializerWithLexicalDeclaration = null;
            ITsExpression initializer = null;
            if (node.Declaration != null)
            {
                initializerWithLexicalDeclaration = (ITsLexicalDeclaration)Visit(node.Declaration).Single();
            }
            else
            {
                // translate all of the initializers and create a comma expression from them
                var initializers = node.Initializers.SelectMany(Visit).Cast<ITsExpression>().ToArray();
                initializer = initializers.Length == 1 ? initializers[0] : Factory.CommaExpression(initializers);
            }

            var condition = (ITsExpression)Visit(node.Condition).Single();
            var statement = (ITsStatement)Visit(node.Statement).Single();

            // translate all of the incrementors and create a comma expression from them
            var incrementors = node.Incrementors.SelectMany(Visit).Cast<ITsExpression>().ToArray();
            ITsExpression incrementor =
                incrementors.Length == 1 ? incrementors[0] : Factory.CommaExpression(incrementors);

            ITsForStatement translated = initializerWithLexicalDeclaration != null
                ? Factory.For(initializerWithLexicalDeclaration, condition, incrementor, statement)
                : Factory.For(initializer, condition, incrementor, statement);

            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a WhileStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsWhileStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitWhileStatement(WhileStatementSyntax node)
        {
            var whileCondition = (ITsExpression)Visit(node.Condition).Single();
            var whileStatement = (ITsStatement)Visit(node.Statement).Single();

            ITsWhileStatement translated = Factory.While(whileCondition, whileStatement);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a DoStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsDoWhileStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitDoStatement(DoStatementSyntax node)
        {
            var doStatement = (ITsStatement)Visit(node.Statement).Single();
            var whileCondition = (ITsExpression)Visit(node.Condition).Single();

            ITsDoWhileStatement translated = Factory.DoWhile(doStatement, whileCondition);
            yield return translated;
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
            var condition = (ITsExpression)Visit(node.Expression).Single();
            var clauses = node.Sections.SelectMany(Visit).Cast<ITsCaseOrDefaultClause>();

            ITsSwitchStatement translated = Factory.Switch(condition, clauses.ToArray());
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a SwitchSectionSyntax node.
        /// </summary>
        /// <returns>An enumerable of <see cref="ITsCaseOrDefaultClause"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitSwitchSection(SwitchSectionSyntax node)
        {
            var labels = node.Labels.SelectMany(Visit).Cast<ITsCaseOrDefaultClause>().ToArray();
            var statements = node.Statements.SelectMany(Visit).Cast<ITsStatementListItem>().ToArray();

            // attach the statements to the last label
            labels[labels.Length - 1] = labels[labels.Length - 1].WithStatements(statements);

            return labels;
        }

        /// <summary>
        /// Called when the visitor visits a CaseSwitchLabelSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsCaseClause"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            var expression = (ITsExpression)Visit(node.Value).Single();
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
