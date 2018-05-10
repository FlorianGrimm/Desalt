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
            ITsType type = _typeTranslator.TranslateSymbol(typeSymbol, _typesToImport, _diagnostics);

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
            ITsIdentifier catchParameter = hasCatch && catchClause.Declaration != null
                ? Factory.Identifier(catchClause.Declaration.Identifier.Text)
                : null;
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

        //// ===========================================================================================================
        //// Loops
        //// ===========================================================================================================

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
