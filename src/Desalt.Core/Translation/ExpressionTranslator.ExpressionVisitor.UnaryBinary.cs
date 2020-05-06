// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionTranslator.ExpressionVisitor.UnaryBinary.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Diagnostics.CodeAnalysis;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Options;
    using Desalt.Core.SymbolTables;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Ast.Expressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal partial class ExpressionTranslator
    {
        private sealed partial class ExpressionVisitor
        {
            //// =======================================================================================================
            //// Unary Expressions
            //// =======================================================================================================

            /// <summary>
            /// Called when the visitor visits a PrefixUnaryExpressionSyntax node.
            /// </summary>
            /// <returns>
            /// An <see cref="ITsUnaryExpression"/> or an <see cref="ITsCallExpression"/> if the unary
            /// expression is backed by an overloaded operator function.
            /// </returns>
            public override ITsExpression VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
            {
                var translated = TranslateUnaryExpression(
                    node,
                    node.Operand,
                    TranslateUnaryOperator(node.OperatorToken, asPrefix: true));
                return translated;
            }

            /// <summary>
            /// Called when the visitor visits a PostfixUnaryExpressionSyntax node.
            /// </summary>
            /// <returns>An <see cref="ITsUnaryExpression"/>.</returns>
            public override ITsExpression VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
            {
                var translated = TranslateUnaryExpression(
                    node,
                    node.Operand,
                    TranslateUnaryOperator(node.OperatorToken, asPrefix: false));
                return translated;
            }

            private ITsExpression TranslateUnaryExpression(
                ExpressionSyntax prefixOrPostfixExpressionNode,
                ExpressionSyntax operandNode,
                TsUnaryOperator operatorKind)
            {
                // See if we need to translate the expression to a user-defined operator method call.
                if (IsTranslatableUserDefinedOperator(prefixOrPostfixExpressionNode, out IMethodSymbol? methodSymbol))
                {
                    return TranslateUserDefinedUnaryOperator(prefixOrPostfixExpressionNode, operandNode, methodSymbol);
                }

                // It's very important not to visit the sub expression until after the user-defined operator check has
                // happened since it depends on the _isVisitingSubExpression state variable.
                ITsExpression operand = VisitSubExpression(operandNode);
                var translated = TsAstFactory.UnaryExpression(operand, operatorKind);
                return translated;
            }

            private ITsExpression TranslateUserDefinedUnaryOperator(
                ExpressionSyntax prefixOrPostfixExpressionNode,
                ExpressionSyntax operandNode,
                IMethodSymbol methodSymbol)
            {
                // Capture the state of the sub-expression tracker before visiting any sub expressions.
                bool isTopLevelExpression = !_isVisitingSubExpression;
                ITsExpression operand = VisitSubExpression(operandNode);

                ITsExpression invocationTarget = GetUserDefinedOperatorInvocationTarget(
                    methodSymbol,
                    prefixOrPostfixExpressionNode);

                // There should only be one argument.
                ITsArgumentList arguments = TsAstFactory.ArgumentList(TsAstFactory.Argument(operand));
                ITsCallExpression callExpression = TsAstFactory.Call(invocationTarget, arguments);

                bool isPrefixIncrementOrDecrement = prefixOrPostfixExpressionNode.Kind()
                    .IsOneOf(SyntaxKind.PreIncrementExpression, SyntaxKind.PreDecrementExpression);
                bool isPostfixIncrementOrDecrement = prefixOrPostfixExpressionNode.Kind()
                    .IsOneOf(SyntaxKind.PostIncrementExpression, SyntaxKind.PostDecrementExpression);
                bool isIncrementOrDecrement = isPrefixIncrementOrDecrement || isPostfixIncrementOrDecrement;

                if (!isIncrementOrDecrement)
                {
                    return callExpression;
                }

                // "Plain" increment and decrement expressions are also fairly simple, but the operand needs to be
                // assigned. (e.g. `x++` => `x = op_Increment(x)`)
                if (isTopLevelExpression || isPrefixIncrementOrDecrement)
                {
                    ITsAssignmentExpression assignmentExpression = TsAstFactory.Assignment(
                        operand,
                        TsAssignmentOperator.SimpleAssign,
                        callExpression);

                    return assignmentExpression;
                }
                else
                {
                    // const $t1 = x;
                    ITsIdentifier temporaryVariableName =
                        TsAstFactory.Identifier(Context.TemporaryVariableAllocator.Reserve("$t"));
                    ITsLexicalDeclaration variableDeclaration = TsAstFactory.LexicalDeclaration(
                        isConst: true,
                        TsAstFactory.SimpleLexicalBinding(temporaryVariableName, initializer: operand));

                    AdditionalStatements.Add(variableDeclaration);

                    // x = op_Increment($t1);
                    ITsAssignmentExpression assignmentExpression = TsAstFactory.Assignment(
                        operand,
                        TsAssignmentOperator.SimpleAssign,
                        TsAstFactory.Call(
                            invocationTarget,
                            TsAstFactory.ArgumentList(TsAstFactory.Argument(temporaryVariableName))));

                    AdditionalStatements.Add(assignmentExpression.ToStatement());

                    // The returned translated node is just the temporary variable now.
                    return temporaryVariableName;
                }
            }

            private TsUnaryOperator TranslateUnaryOperator(SyntaxToken operatorToken, bool asPrefix)
            {
                TsUnaryOperator? op = operatorToken.Kind() switch
                {
                    SyntaxKind.PlusPlusToken => asPrefix
                        ? TsUnaryOperator.PrefixIncrement
                        : TsUnaryOperator.PostfixIncrement,
                    SyntaxKind.MinusMinusToken => asPrefix
                        ? TsUnaryOperator.PrefixDecrement
                        : TsUnaryOperator.PostfixDecrement,
                    SyntaxKind.PlusToken => TsUnaryOperator.Plus,
                    SyntaxKind.MinusToken => TsUnaryOperator.Minus,
                    SyntaxKind.TildeToken => TsUnaryOperator.BitwiseNot,
                    SyntaxKind.ExclamationToken => TsUnaryOperator.LogicalNot,
                    _ => null,
                };

                if (op != null)
                {
                    return op.Value;
                }

                Diagnostics.Add(DiagnosticFactory.OperatorKindNotSupported(operatorToken));
                return TsUnaryOperator.Plus;
            }

            //// =======================================================================================================
            //// Binary Expressions
            //// =======================================================================================================

            /// <summary>
            /// Called when the visitor visits a BinaryExpressionSyntax node.
            /// </summary>
            /// <returns>An <see cref="ITsBinaryExpression"/>.</returns>
            public override ITsExpression VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                var leftSide = VisitSubExpression(node.Left);
                var rightSide = VisitSubExpression(node.Right);

                // See if we need to translate the expression to a user-defined operator method call.
                if (IsTranslatableUserDefinedOperator(node, out IMethodSymbol? methodSymbol))
                {
                    ITsExpression invocationTarget = GetUserDefinedOperatorInvocationTarget(methodSymbol, node);
                    ITsCallExpression callExpression = TsAstFactory.Call(
                        invocationTarget,
                        TsAstFactory.ArgumentList(TsAstFactory.Argument(leftSide), TsAstFactory.Argument(rightSide)));
                    return callExpression;
                }

                ITsBinaryExpression translated = TsAstFactory.BinaryExpression(
                    leftSide,
                    TranslateBinaryOperator(node.OperatorToken),
                    rightSide);
                return translated;
            }

            private TsBinaryOperator TranslateBinaryOperator(SyntaxToken operatorToken)
            {
                TsBinaryOperator? op = operatorToken.Kind() switch
                {
                    SyntaxKind.AsteriskToken => TsBinaryOperator.Multiply,
                    SyntaxKind.SlashToken => TsBinaryOperator.Divide,
                    SyntaxKind.PercentToken => TsBinaryOperator.Modulo,
                    SyntaxKind.PlusToken => TsBinaryOperator.Add,
                    SyntaxKind.MinusToken => TsBinaryOperator.Subtract,
                    SyntaxKind.LessThanLessThanToken => TsBinaryOperator.LeftShift,
                    SyntaxKind.GreaterThanGreaterThanToken => TsBinaryOperator.SignedRightShift,
                    SyntaxKind.LessThanToken => TsBinaryOperator.LessThan,
                    SyntaxKind.GreaterThanToken => TsBinaryOperator.GreaterThan,
                    SyntaxKind.LessThanEqualsToken => TsBinaryOperator.LessThanEqual,
                    SyntaxKind.GreaterThanEqualsToken => TsBinaryOperator.GreaterThanEqual,
                    SyntaxKind.EqualsEqualsToken => TsBinaryOperator.StrictEquals,
                    SyntaxKind.ExclamationEqualsToken => TsBinaryOperator.StrictNotEquals,
                    SyntaxKind.AmpersandToken => TsBinaryOperator.BitwiseAnd,
                    SyntaxKind.CaretToken => TsBinaryOperator.BitwiseXor,
                    SyntaxKind.BarToken => TsBinaryOperator.BitwiseOr,
                    SyntaxKind.AmpersandAmpersandToken => TsBinaryOperator.LogicalAnd,
                    SyntaxKind.BarBarToken => TsBinaryOperator.LogicalOr,
                    SyntaxKind.QuestionQuestionToken => TsBinaryOperator.LogicalOr,
                    _ => null,
                };

                if (op != null)
                {
                    return op.Value;
                }

                Diagnostics.Add(DiagnosticFactory.OperatorKindNotSupported(operatorToken));
                return TsBinaryOperator.Add;
            }

            /// <summary>
            /// Returns a value indicating whether the specified node represents a user-defined operator method overload
            /// and if it should be translated as such. If the user-defined operator is decorated with
            /// [IntrinsicOperator] it should be treated like the built-in operator.
            /// </summary>
            /// <param name="node">The syntax node to test.</param>
            /// <param name="methodSymbol">
            /// If true, the resulting <see cref="IMethodSymbol"/> representing the user-defined operator method.
            /// </param>
            /// <returns>True if the symbol is a user-defined operator method; otherwise, false.</returns>
            private bool IsTranslatableUserDefinedOperator(
                ExpressionSyntax node,
                [NotNullWhen(true)] out IMethodSymbol? methodSymbol)
            {
                methodSymbol = SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                return methodSymbol?.MethodKind == MethodKind.UserDefinedOperator &&
                    !Context.GetExpectedScriptSymbol<IScriptMethodSymbol>(methodSymbol, node).IntrinsicOperator;
            }

            /// <summary>
            /// Gets the target of the user defined operator function call (the static class name).
            /// </summary>
            private ITsExpression GetUserDefinedOperatorInvocationTarget(
                IMethodSymbol methodSymbol,
                ExpressionSyntax node)
            {
                // Get the translated name of the overload function.
                UserDefinedOperatorKind kind = MethodNameToOperatorOverloadKind(methodSymbol.Name, node);
                string functionName = Context.RenameRules.UserDefinedOperatorMethodNames[kind];

                // Get the target of the invocation (the static class name).
                ITsExpression invocationTarget = Context.TranslateIdentifierName(methodSymbol, node, functionName);

                return invocationTarget;
            }

            private UserDefinedOperatorKind MethodNameToOperatorOverloadKind(string methodName, SyntaxNode node)
            {
                UserDefinedOperatorKind? kind = methodName switch
                {
                    // Unary Operators
                    "op_UnaryPlus" => UserDefinedOperatorKind.UnaryPlus,
                    "op_UnaryNegation" => UserDefinedOperatorKind.UnaryNegation,
                    "op_LogicalNot" => UserDefinedOperatorKind.LogicalNot,
                    "op_OnesComplement" => UserDefinedOperatorKind.OnesComplement,
                    "op_Increment" => UserDefinedOperatorKind.Increment,
                    "op_Decrement" => UserDefinedOperatorKind.Decrement,

                    // Binary Operators
                    "op_Addition" => UserDefinedOperatorKind.Addition,
                    "op_Subtraction" => UserDefinedOperatorKind.Subtraction,
                    "op_Multiply" => UserDefinedOperatorKind.Multiplication,
                    "op_Division" => UserDefinedOperatorKind.Division,
                    "op_Modulus" => UserDefinedOperatorKind.Modulus,
                    "op_LeftShift" => UserDefinedOperatorKind.LeftShift,
                    "op_RightShift" => UserDefinedOperatorKind.RightShift,
                    "op_LessThan" => UserDefinedOperatorKind.LessThan,
                    "op_LessThanOrEqual" => UserDefinedOperatorKind.LessThanEquals,
                    "op_GreaterThan" => UserDefinedOperatorKind.GreaterThan,
                    "op_GreaterThanOrEqual" => UserDefinedOperatorKind.GreaterThanEquals,
                    "op_Equality" => UserDefinedOperatorKind.Equality,
                    "op_Inequality" => UserDefinedOperatorKind.Inequality,
                    "op_BitwiseAnd" => UserDefinedOperatorKind.BitwiseAnd,
                    "op_ExclusiveOr" => UserDefinedOperatorKind.ExclusiveOr,
                    "op_BitwiseOr" => UserDefinedOperatorKind.BitwiseOr,
                    _ => null
                };

                if (kind == null)
                {
                    Context.ReportInternalError(
                        $"Unsupported or unknown operator overload method '{methodName}'.",
                        node);
                }

                return kind.GetValueOrDefault();
            }
        }
    }
}
