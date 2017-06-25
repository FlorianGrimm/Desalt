// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using System.Collections.Generic;
    using Desalt.TypeScript.Ast.Expressions;

    public static partial class TsAstFactory
    {
        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        public static ITsThis This => TsThis.Instance;
        public static readonly ITsObjectLiteral EmptyObjectLiteral = new TsObjectLiteral(null);

        public static ITsNullLiteral NullLiteral => TsNullLiteral.Instance;
        public static ITsBooleanLiteral TrueLiteral => TsBooleanLiteral.True;
        public static ITsBooleanLiteral FalseLiteral => TsBooleanLiteral.False;

        public static ITsStringLiteral StringLiteral(string value, StringLiteralQuoteKind quoteKind) =>
            new TsStringLiteral(value, quoteKind);

        public static ITsNumericLiteral DecimalLiteral(double value) =>
            new TsNumericLiteral(TsNumericLiteralKind.Decimal, value);

        public static ITsNumericLiteral BinaryIntegerLiteral(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.BinaryInteger, value);

        public static ITsNumericLiteral OctalIntegerLiteral(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.OctalInteger, value);

        public static ITsNumericLiteral HexIntegerLiteral(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.HexInteger, value);

        public static ITsRegularExpressionLiteral RegularExpressionLiteral(string body, string flags) =>
            new TsRegularExpressionLiteral(body, flags);

        public static ITsArrayLiteral ArrayLiteral(params ITsArrayElement[] elements) => new TsArrayLiteral(elements);

        public static ITsArrayElement ArrayElement(ITsAssignmentExpression element, bool isSpreadElement = false) =>
            new TsArrayElement(element, isSpreadElement);

        public static ITsTemplateLiteral TemplateLiteral(params TsTemplatePart[] parts) => new TsTemplateLiteral(parts);

        //// ===========================================================================================================
        //// Object Literal Expressions
        //// ===========================================================================================================

        public static ITsObjectLiteral ObjectLiteral(IEnumerable<ITsPropertyDefinition> propertyDefinitions) =>
            new TsObjectLiteral(propertyDefinitions);

        public static ITsObjectLiteral ObjectLiteral(params ITsPropertyDefinition[] propertyDefinitions) =>
            new TsObjectLiteral(propertyDefinitions);

        public static ITsCoverInitializedName CoverInitializedName(
            ITsIdentifier identifier,
            ITsAssignmentExpression initializer)
        {
            return new TsCoverInitializedName(identifier, initializer);
        }

        public static ITsPropertyAssignment PropertyAssignment(
            ITsPropertyName propertyName,
            ITsAssignmentExpression initializer)
        {
            return new TsPropertyAssignment(propertyName, initializer);
        }

        public static ITsComputedPropertyName ComputedPropertyName(ITsAssignmentExpression expression) =>
            new TsComputedPropertyName(expression);

        public static ITsPropertyFunction PropertyFunction(
            ITsPropertyName propertyName,
            ITsCallSignature callSignature,
            params ITsStatementListItem[] functionBody)
        {
            return new TsPropertyFunction(propertyName, callSignature, functionBody);
        }

        public static ITsGetAccessor GetAccessor(
            ITsPropertyName propertyName,
            ITsType propertyType = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsGetAccessor(propertyName, propertyType, functionBody);
        }

        public static ITsSetAccessor SetAccessor(
            ITsPropertyName propertyName,
            ITsBindingIdentifierOrPattern parameterName,
            ITsType parameterType = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsSetAccessor(propertyName, parameterName, parameterType, functionBody);
        }

        //// ===========================================================================================================
        //// Unary and Binary Expressions
        //// ===========================================================================================================

        public static ITsUnaryExpression UnaryExpression(ITsExpression operand, TsUnaryOperator @operator) =>
            new TsUnaryExpression(operand, @operator);

        public static ITsBinaryExpression BinaryExpression(
            ITsExpression leftSide,
            TsBinaryOperator @operator,
            ITsExpression rightSide)
        {
            return new TsBinaryExpression(leftSide, @operator, rightSide);
        }

        public static ITsConditionalExpression ConditionalExpression(
            ITsExpression condition,
            ITsExpression whenTrue,
            ITsExpression whenFalse)
        {
            return new TsConditionalExpression(condition, whenTrue, whenFalse);
        }

        //// ===========================================================================================================
        //// Function and Class Expressions
        //// ===========================================================================================================

        public static ITsFunctionExpression FunctionExpression(
            ITsCallSignature callSignature,
            ITsIdentifier functionName = null,
            params ITsStatementListItem[] functionBody)
        {
            return new TsFunctionExpression(callSignature, functionName, functionBody);
        }

        public static ITsClassExpression ClassExpression(
            ITsIdentifier className = null,
            ITsLeftHandSideExpression heritage = null,
            params ITsClassElement[] classBody)
        {
            return new TsClassExpression(className, heritage, classBody);
        }
    }
}
