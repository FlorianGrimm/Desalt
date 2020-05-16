// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="StatementTranslator.StatementVisitor.TryCatchAndUsing.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------
namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Ast.Declarations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed partial class StatementVisitor
    {
        //// ===========================================================================================================
        //// Throw, Try/Catch, and Using Statements
        //// ===========================================================================================================

        private ITsIdentifier? _lastCatchIdentifier;

        /// <summary>
        /// Called when the visitor visits a ThrowStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsThrowStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitThrowStatement(ThrowStatementSyntax node)
        {
            ITsExpression expression;

            if (node.Expression == null)
            {
                if (_lastCatchIdentifier == null)
                {
                    Context.ReportInternalError("_lastCatchIdentifier should have been set", node);
                    expression = TsAstFactory.Identifier("FIXME");
                }
                else
                {
                    expression = _lastCatchIdentifier;
                }
            }
            else
            {
                var expressionTranslation = ExpressionTranslator.Translate(Context, node.Expression);
                expression = expressionTranslation.Expression;
                foreach (var statement in expressionTranslation.AdditionalStatementsRequiredBeforeExpression)
                {
                    yield return statement;
                }
            }

            ITsThrowStatement translated = TsAstFactory.Throw(expression);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a TryStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsTryStatement"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitTryStatement(TryStatementSyntax node)
        {
            ITsBlockStatement tryBlock = TranslateBlock(node.Block);

            // Translate only the first catch clause.
            if (node.Catches.Count > 1)
            {
                Diagnostics.Add(DiagnosticFactory.CatchClausesWithMoreThanOneParameterNotYetSupported(node));
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
                    catchParameter = TsAstFactory.Identifier("e");
                }
                else if (declaration != null)
                {
                    catchParameter = TsAstFactory.Identifier(declaration.Identifier.Text);
                }

                // C# can have plain 'throw;' statements, but TypeScript cannot, so check for this
                // case and generate a placeholder if necessary
                else if (catchClause.Block.Statements.OfType<ThrowStatementSyntax>()
                    .Any(throwSyntax => throwSyntax.Expression == null))
                {
                    catchParameter = TsAstFactory.Identifier("e");
                }

                // Cache this identifier temporarily since VisitThrowStatement will need it.
                _lastCatchIdentifier = catchParameter;
            }

            ITsBlockStatement? catchBlock = catchClause != null ? TranslateBlock(catchClause.Block) : null;

            // Translate the finally block if present.
            ITsBlockStatement? finallyBlock = node.Finally != null ? TranslateBlock(node.Finally.Block) : null;

            // Translate the try/catch/finally statement.
            ITsTryStatement translated;
            if (catchBlock != null && finallyBlock != null)
            {
                translated = TsAstFactory.TryCatchFinally(tryBlock, catchParameter, catchBlock, finallyBlock);
            }
            else if (catchBlock != null)
            {
                translated = TsAstFactory.TryCatch(tryBlock, catchParameter, catchBlock);
            }
            else if (finallyBlock != null)
            {
                translated = TsAstFactory.TryFinally(tryBlock, finallyBlock);
            }
            else
            {
                translated = TsAstFactory.Try(tryBlock);
            }

            // Reset this temporary variable.
            _lastCatchIdentifier = null;

            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a UsingStatementSyntax node.
        /// </summary>
        /// <returns>A <see cref="ITsBlockStatement"/> representing a wrapped try/finally block.</returns>
        public override IEnumerable<ITsStatementListItem> VisitUsingStatement(UsingStatementSyntax node)
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
                var declaration = TranslateVariableDeclaration(
                    node.Declaration,
                    out ImmutableArray<ITsStatementListItem> additionalDeclarationStatements);

                declaration = declaration.WithIsConst(true);

                // Get the type of the declaration.
                ITypeSymbol? typeSymbol = SemanticModel.GetTypeInfo(node.Declaration.Type).Type;
                ITsType? declarationType = typeSymbol == null
                    ? null
                    : TypeTranslator.TranslateTypeSymbol(Context, typeSymbol, node.Declaration.Type.GetLocation);

                // Fix up all of the declarations to add the type.
                if (declarationType != null)
                {
                    declaration = declaration.WithDeclarations(
                        declaration.Declarations.Cast<ITsSimpleLexicalBinding>()
                            .Select(binding => binding.WithVariableType(declarationType))
                            .Cast<ITsLexicalBinding>()
                            .ToImmutableArray());
                }

                // Add any additional statements before this statement.
                statements.AddRange(additionalDeclarationStatements);
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
                if (node.Expression == null)
                {
                    Context.ReportInternalError("Inside of a using statement the expression should be defined", node);
                }

                IExpressionTranslation expressionTranslation = ExpressionTranslator.Translate(Context, node.Expression);

                // Try to find the type of the expression.
                ITypeSymbol? expressionTypeSymbol = SemanticModel.GetTypeInfo(node.Expression).ConvertedType;

                ITsType? variableType = expressionTypeSymbol == null
                    ? null
                    : TypeTranslator.TranslateTypeSymbol(
                        Context,
                        expressionTypeSymbol,
                        () => node.Expression.GetLocation());

                // Create a temporary variable name to hold the expression.
                reservedTemporaryVariable = Context.TemporaryVariableAllocator.Reserve("$using");
                variableNameIdentifier = TsAstFactory.Identifier(reservedTemporaryVariable);

                // Assign the expression to the temporary variable.
                ITsLexicalDeclaration declaration = TsAstFactory.LexicalDeclaration(
                    isConst: true,
                    declarations: new ITsLexicalBinding[]
                    {
                        TsAstFactory.SimpleLexicalBinding(
                            variableNameIdentifier,
                            variableType,
                            expressionTranslation.Expression)
                    });

                // Add any additional statements before this statement.
                statements.AddRange(expressionTranslation.AdditionalStatementsRequiredBeforeExpression);
                statements.Add(declaration);
            }

            // Create the try block, which is the using block.
            var usingBlock = TranslateAsSingleStatement(node.Statement);
            if (usingBlock is ITsBlockStatement tryBlock)
            {
                // Remove any trailing newlines.
                tryBlock = tryBlock.WithTrailingTrivia();
            }
            else
            {
                tryBlock = TsAstFactory.Block(usingBlock);
            }

            // Create the finally block, which disposes the object.
            ITsBlockStatement finallyBlock = TsAstFactory.Block(
                TsAstFactory.IfStatement(
                    variableNameIdentifier,
                    TsAstFactory.Block(
                        TsAstFactory.Call(TsAstFactory.MemberDot(variableNameIdentifier, "dispose")).ToStatement())));

            // Create the try/finally statement.
            ITsTryStatement tryFinally = TsAstFactory.TryFinally(tryBlock, finallyBlock);
            statements.Add(tryFinally);

            // Wrap the declaration inside of a block so that scoping will be correct.
            ITsBlockStatement translated =
                TsAstFactory.Block(statements.ToArray()).WithTrailingTrivia(TsAstFactory.Newline);

            // Return the temporary variable to the allocator.
            if (reservedTemporaryVariable != null)
            {
                Context.TemporaryVariableAllocator.Return(reservedTemporaryVariable);
            }

            yield return translated;
        }
    }
}
