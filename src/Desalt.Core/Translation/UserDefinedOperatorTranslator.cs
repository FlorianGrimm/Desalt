// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="UserDefinedOperatorTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Options;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Ast.Expressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    internal class UserDefinedOperatorTranslator
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly SemanticModel _semanticModel;
        private readonly RenameRules _renameRules;
        private readonly TranslateIdentifierFunc _translateIdentifierFunc;
        private readonly TranslationVisitFunc<ITsExpression> _visitFunc;
        private readonly TemporaryVariableAllocator _temporaryVariableAllocator;
        private readonly ICollection<Diagnostic> _diagnostics;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public UserDefinedOperatorTranslator(
            SemanticModel semanticModel,
            RenameRules renameRules,
            TranslateIdentifierFunc translateIdentifierFunc,
            TranslationVisitFunc<ITsExpression> visitFunc,
            TemporaryVariableAllocator temporaryVariableAllocator,
            ICollection<Diagnostic>? diagnostics = null)
        {
            _semanticModel = semanticModel;
            _renameRules = renameRules;
            _translateIdentifierFunc = translateIdentifierFunc;
            _visitFunc = visitFunc;
            _temporaryVariableAllocator = temporaryVariableAllocator;
            _diagnostics = diagnostics ?? new List<Diagnostic>();
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Attempts to translate a unary expression as a call to a user defined operator overload method call.
        /// </summary>
        /// <param name="expressionNode">The expression to try to translate.</param>
        /// <param name="isTopLevelExpressionInStatement">
        /// Indicates whether this is the top-level expression inside of a statement. This matters for translating post
        /// increment/decrement operators since if they're part of an inner expression, then a temporary variable will
        /// need to be allocated.
        /// </param>
        /// <param name="translatedExpression">
        /// The translated operator overload method call if the method returns true; otherwise, null.
        /// </param>
        /// <param name="additionalStatementsRequiredBeforeTranslatedExpression">
        /// If the method returns true, this contains additional statements that will need to be inserted before the
        /// expression is evaluated. For some operations, a temporary variable needs to be declared and used.
        /// </param>
        /// <returns>True if the expression was translated as a user-defined operator; false, otherwise.</returns>
        public bool TryTranslate(
            ExpressionSyntax expressionNode,
            bool isTopLevelExpressionInStatement,
            [NotNullWhen(true)] out ITsExpression? translatedExpression,
            out IEnumerable<ITsStatementListItem> additionalStatementsRequiredBeforeTranslatedExpression)
        {
            additionalStatementsRequiredBeforeTranslatedExpression = Enumerable.Empty<ITsStatementListItem>();

            // See if this is actually a user-defined operator overload method call.
            if (!IsUserDefinedOperator(expressionNode, out IMethodSymbol? methodSymbol))
            {
                translatedExpression = null;
                return false;
            }

            translatedExpression = expressionNode switch
            {
                BinaryExpressionSyntax binary => TranslateBinaryExpression(binary, methodSymbol),
                PostfixUnaryExpressionSyntax postfix => TranslateUnaryExpression(
                    expressionNode,
                    postfix.Operand,
                    methodSymbol,
                    isTopLevelExpressionInStatement,
                    out additionalStatementsRequiredBeforeTranslatedExpression),
                PrefixUnaryExpressionSyntax prefix => TranslateUnaryExpression(
                    expressionNode,
                    prefix.Operand,
                    methodSymbol,
                    isTopLevelExpressionInStatement,
                    out additionalStatementsRequiredBeforeTranslatedExpression),
                _ => null,
            };

            return translatedExpression != null;
        }

        private static bool IsIncrementOrDecrement(ExpressionSyntax expressionNode)
        {
            return IsPrefixIncrementOrDecrement(expressionNode) || IsPostfixIncrementOrDecrement(expressionNode);
        }

        private static bool IsPrefixIncrementOrDecrement(ExpressionSyntax expressionNode)
        {
            return expressionNode.Kind().IsOneOf(SyntaxKind.PreIncrementExpression, SyntaxKind.PreDecrementExpression);
        }

        private static bool IsPostfixIncrementOrDecrement(ExpressionSyntax expressionNode)
        {
            return expressionNode.Kind()
                .IsOneOf(SyntaxKind.PostIncrementExpression, SyntaxKind.PostDecrementExpression);
        }

        /// <summary>
        /// Translates a binary expression that is a user-defined operator as a function call to the overloaded operator.
        /// </summary>
        private ITsCallExpression TranslateBinaryExpression(BinaryExpressionSyntax node, IMethodSymbol methodSymbol)
        {
            ITsExpression leftSide = _visitFunc(node.Left);
            ITsExpression rightSide = _visitFunc(node.Right);

            ITsExpression invocationTarget = GetInvocationTarget(node, methodSymbol);
            ITsCallExpression callExpression = Factory.Call(
                invocationTarget,
                Factory.ArgumentList(Factory.Argument(leftSide), Factory.Argument(rightSide)));

            return callExpression;
        }

        private ITsExpression TranslateUnaryExpression(
            ExpressionSyntax expressionNode,
            ExpressionSyntax operandNode,
            IMethodSymbol methodSymbol,
            bool isTopLevelExpressionInStatement,
            out IEnumerable<ITsStatementListItem> additionalStatementsRequiredBeforeTranslatedExpression)
        {
            ITsExpression translatedOperand = _visitFunc(operandNode);
            ITsExpression invocationTarget = GetInvocationTarget(expressionNode, methodSymbol);
            var additionalStatements = new List<ITsStatementListItem>();
            additionalStatementsRequiredBeforeTranslatedExpression = additionalStatements;

            // There should only be one argument.
            ITsArgumentList arguments = Factory.ArgumentList(Factory.Argument(translatedOperand));
            ITsCallExpression callExpression = Factory.Call(invocationTarget, arguments);

            if (!IsIncrementOrDecrement(expressionNode))
            {
                return callExpression;
            }

            // "Plain" increment and decrement expressions are also fairly simple, but the operand needs to be assigned.
            // (e.g. `x++` => `x = op_Increment(x)`)
            if (isTopLevelExpressionInStatement || IsPrefixIncrementOrDecrement(expressionNode))
            {
                ITsAssignmentExpression assignmentExpression = Factory.Assignment(
                    translatedOperand,
                    TsAssignmentOperator.SimpleAssign,
                    callExpression);

                return assignmentExpression;
            }
            else
            {
                // const $t1 = x;
                ITsIdentifier temporaryVariableName = Factory.Identifier(_temporaryVariableAllocator.Reserve("$t"));
                ITsLexicalDeclaration variableDeclaration = Factory.LexicalDeclaration(
                    isConst: true,
                    Factory.SimpleLexicalBinding(temporaryVariableName, initializer: translatedOperand));

                additionalStatements.Add(variableDeclaration);

                // x = op_Increment($t1);
                ITsAssignmentExpression assignmentExpression = Factory.Assignment(
                    translatedOperand,
                    TsAssignmentOperator.SimpleAssign,
                    Factory.Call(invocationTarget, Factory.ArgumentList(Factory.Argument(temporaryVariableName))));

                additionalStatements.Add(assignmentExpression.ToStatement());

                // The returned translated node is just the temporary variable now.
                return temporaryVariableName;
            }
        }

        /// <summary>
        /// Gets the target of the user defined operator function call (the static class name).
        /// </summary>
        private ITsExpression GetInvocationTarget(ExpressionSyntax node, IMethodSymbol methodSymbol)
        {
            // Get the translated name of the overload function.
            UserDefinedOperatorKind kind = MethodNameToOperatorOverloadKind(methodSymbol.Name, node.GetLocation);
            string functionName = _renameRules.UserDefinedOperatorMethodNames[kind];

            // Get the target of the invocation (the static class name).
            ITsExpression invocationTarget = _translateIdentifierFunc(methodSymbol, node, functionName);

            return invocationTarget;
        }

        /// <summary>
        /// Returns a value indicating whether the specified node represents a user-defined operator method overload.
        /// </summary>
        /// <param name="expressionNode">The syntax node to test.</param>
        /// <param name="methodSymbol">
        /// If true, the resulting <see cref="IMethodSymbol"/> representing the user-defined operator method.
        /// </param>
        /// <returns>True if the symbol is a user-defined operator method; otherwise, false.</returns>
        private bool IsUserDefinedOperator(
            ExpressionSyntax expressionNode,
            [NotNullWhen(true)] out IMethodSymbol? methodSymbol)
        {
            methodSymbol = _semanticModel.GetSymbolInfo(expressionNode).Symbol as IMethodSymbol;
            return methodSymbol?.MethodKind == MethodKind.UserDefinedOperator;
        }

        public UserDefinedOperatorKind MethodNameToOperatorOverloadKind(
            string methodName,
            Func<Location> getNodeLocationFunc)
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
                _diagnostics.Add(
                    DiagnosticFactory.InternalError(
                        $"Unsupported or unknown operator overload method '{methodName}'.",
                        getNodeLocationFunc()));
            }

            return kind.GetValueOrDefault();
        }
    }
}
