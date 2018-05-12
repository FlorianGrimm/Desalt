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
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Extensions;
    using Desalt.Core.TypeScript.Ast;
    using Desalt.Core.TypeScript.Ast.Declarations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    internal sealed partial class TranslationVisitor
    {
        /// <summary>
        /// Called when the visitor visits a ExpressionStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsExpressionStatement"/>.</returns>
        public override IEnumerable<IAstNode> VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var expression = (ITsExpression)Visit(node.Expression).Single();
            ITsExpressionStatement translated = Factory.ExpressionStatement(expression);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a BlockSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsBlockStatement"/>.</returns>
        public override IEnumerable<IAstNode> VisitBlock(BlockSyntax node)
        {
            ITsStatementListItem[] statements =
                node.Statements.SelectMany(Visit).Cast<ITsStatementListItem>().ToArray();
            return Factory.Block(statements).ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a BracketedArgumentListSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsExpression"/>.</returns>
        public override IEnumerable<IAstNode> VisitBracketedArgumentList(BracketedArgumentListSyntax node)
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
        public override IEnumerable<IAstNode> VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
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
        public override IEnumerable<IAstNode> VisitVariableDeclarator(VariableDeclaratorSyntax node)
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
        public override IEnumerable<IAstNode> VisitVariableDeclaration(VariableDeclarationSyntax node)
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
        public override IEnumerable<IAstNode> VisitIfStatement(IfStatementSyntax node)
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

        /// <summary>
        /// Called when the visitor visits a ThrowStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsThrowStatement"/>.</returns>
        public override IEnumerable<IAstNode> VisitThrowStatement(ThrowStatementSyntax node)
        {
            var expression = (ITsExpression)Visit(node.Expression).Single();
            ITsThrowStatement translated = Factory.Throw(expression);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a TryStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsTryStatement"/>.</returns>
        public override IEnumerable<IAstNode> VisitTryStatement(TryStatementSyntax node)
        {
            var tryBlock = (ITsBlockStatement)Visit(node.Block).Single();

            // translate only the first catch clause
            if (node.Catches.Count > 1)
            {
                _diagnostics.Add(DiagnosticFactory.CatchClausesWithMoreThanOneParameterNotYetSupported(node));
            }

            bool hasCatch = node.Catches.Count > 0;
            CatchClauseSyntax catchClause = node.Catches[0];
            ITsIdentifier catchParameter = null;
            if (hasCatch && catchClause.Declaration != null)
            {
                // C# can have `catch (Exception)` without an identifier, but we need one in
                // TypeScript, so generate a placeholder if necessary
                if (catchClause.Declaration.Identifier.IsKind(SyntaxKind.None))
                {
                    catchParameter = Factory.Identifier("e");
                }
                else
                {
                    catchParameter = Factory.Identifier(catchClause.Declaration.Identifier.Text);
                }
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

            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a UsingStatementSyntax node.
        /// </summary>
        /// <returns>A <see cref="ITsBlockStatement"/> representing a wrapped try/finally block.</returns>
        public override IEnumerable<IAstNode> VisitUsingStatement(UsingStatementSyntax node)
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
            // using (c.GetDipose()) {}
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
                tryBlock = tryBlock.WithTrailingTrivia(new IAstTriviaNode[0]);
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
        public override IEnumerable<IAstNode> VisitBreakStatement(BreakStatementSyntax node)
        {
            ITsBreakStatement translated = Factory.Break();
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ForEachStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsForOfStatement"/>.</returns>
        public override IEnumerable<IAstNode> VisitForEachStatement(ForEachStatementSyntax node)
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
        public override IEnumerable<IAstNode> VisitForStatement(ForStatementSyntax node)
        {
            var initializer = (ITsLexicalDeclaration)Visit(node.Declaration).Single();
            var condition = (ITsExpression)Visit(node.Condition).Single();
            var statement = (ITsStatement)Visit(node.Statement).Single();

            // translate all of the incrementors and create a comma expression from them
            var incrementors = node.Incrementors.SelectMany(Visit).Cast<ITsExpression>().ToArray();
            ITsExpression incrementor =
                incrementors.Length == 1 ? incrementors[0] : Factory.CommaExpression(incrementors);

            ITsForStatement translated = Factory.For(initializer, condition, incrementor, statement);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a WhileStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsWhileStatement"/>.</returns>
        public override IEnumerable<IAstNode> VisitWhileStatement(WhileStatementSyntax node)
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
        public override IEnumerable<IAstNode> VisitDoStatement(DoStatementSyntax node)
        {
            var doStatement = (ITsStatement)Visit(node.Statement).Single();
            var whileCondition = (ITsExpression)Visit(node.Condition).Single();

            ITsDoWhileStatement translated = Factory.DoWhile(doStatement, whileCondition);
            yield return translated;
        }

        //// ===========================================================================================================
        //// Functions and Methods
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a AnonymousMethodExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArrowFunction"/>.</returns>
        public override IEnumerable<IAstNode> VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            ITsCallSignature callSignature = TranslateCallSignature(node.ParameterList);
            var body = (ITsBlockStatement)Visit(node.Block).Single();
            ITsArrowFunction translated = Factory.ArrowFunction(callSignature, body.Statements.ToArray());
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ParenthesizedLambdaExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArrowFunction"/>.</returns>
        public override IEnumerable<IAstNode> VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            ITsCallSignature callSignature = TranslateCallSignature(node.ParameterList);
            var body = (ITsExpression)Visit(node.Body).Single();
            ITsArrowFunction translated = Factory.ArrowFunction(callSignature, body);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ReturnStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsReturnStatement"/>.</returns>
        public override IEnumerable<IAstNode> VisitReturnStatement(ReturnStatementSyntax node)
        {
            ITsExpression expression = null;
            if (node.Expression != null)
            {
                expression = (ITsExpression)Visit(node.Expression).Single();
            }

            ITsReturnStatement translated = Factory.Return(expression);
            yield return translated;
        }
    }
}
