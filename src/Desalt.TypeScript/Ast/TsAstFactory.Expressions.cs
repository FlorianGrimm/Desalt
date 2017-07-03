// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.TypeScript.Ast.Expressions;

    public static partial class TsAstFactory
    {
        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        public static ITsThis This => TsThis.Instance;
        public static readonly ITsObjectLiteral EmptyObject = new TsObjectLiteral(null);

        public static ITsNullLiteral Null => TsNullLiteral.Instance;
        public static ITsBooleanLiteral True => TsBooleanLiteral.True;
        public static ITsBooleanLiteral False => TsBooleanLiteral.False;

        public static ITsNumericLiteral Zero => TsNumericLiteral.Zero;

        public static ITsStringLiteral String(
            string value,
            StringLiteralQuoteKind quoteKind = StringLiteralQuoteKind.SingleQuote)
        {
            return new TsStringLiteral(value, quoteKind);
        }

        public static ITsNumericLiteral Number(double value) =>
            new TsNumericLiteral(TsNumericLiteralKind.Decimal, value);

        public static ITsNumericLiteral BinaryInteger(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.BinaryInteger, value);

        public static ITsNumericLiteral OctalInteger(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.OctalInteger, value);

        public static ITsNumericLiteral HexInteger(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.HexInteger, value);

        public static ITsRegularExpressionLiteral RegularExpression(string body, string flags) =>
            new TsRegularExpressionLiteral(body, flags);

        public static ITsArrayLiteral Array(params ITsArrayElement[] elements) => new TsArrayLiteral(elements);

        public static ITsArrayLiteral Array(params ITsExpression[] elements) =>
            new TsArrayLiteral(elements.Select(e => ArrayElement(e)));

        public static ITsArrayElement ArrayElement(ITsExpression element, bool isSpreadElement = false) =>
            new TsArrayElement(element, isSpreadElement);

        public static ITsTemplateLiteral TemplateString(params TsTemplatePart[] parts) => new TsTemplateLiteral(parts);

        //// ===========================================================================================================
        //// Object Literal Expressions
        //// ===========================================================================================================

        public static ITsObjectLiteral Object(IEnumerable<ITsPropertyDefinition> propertyDefinitions) =>
            new TsObjectLiteral(propertyDefinitions);

        public static ITsObjectLiteral Object(params ITsPropertyDefinition[] propertyDefinitions) =>
            new TsObjectLiteral(propertyDefinitions);

        public static ITsCoverInitializedName CoverInitializedName(ITsIdentifier identifier, ITsExpression initializer)
        {
            return new TsCoverInitializedName(identifier, initializer);
        }

        public static ITsPropertyAssignment PropertyAssignment(ITsPropertyName propertyName, ITsExpression initializer)
        {
            return new TsPropertyAssignment(propertyName, initializer);
        }

        public static ITsComputedPropertyName ComputedPropertyName(ITsExpression expression) =>
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

        public static ITsConditionalExpression Conditional(
            ITsExpression condition,
            ITsExpression whenTrue,
            ITsExpression whenFalse)
        {
            return new TsConditionalExpression(condition, whenTrue, whenFalse);
        }

        public static ITsAssignmentExpression AssignmentExpression(
            ITsExpression leftSide,
            TsAssignmentOperator @operator,
            ITsExpression rightSide)
        {
            return new TsAssignmentExpression(leftSide, @operator, rightSide);
        }

        //// ===========================================================================================================
        //// Left-hand Side Expressions
        //// ===========================================================================================================

        public static ITsMemberBracketExpression MemberBracket(ITsExpression leftSide, ITsExpression bracketContents) =>
            TsMemberBracketExpression.Create(leftSide, bracketContents);

        public static ITsMemberDotExpression MemberDot(ITsExpression leftSide, string dotName) =>
            TsMemberDotExpression.Create(leftSide, dotName);

        public static ITsSuperBracketExpression SuperBracket(ITsExpression bracketContents) =>
            TsMemberBracketExpression.CreateSuper(bracketContents);

        public static ITsSuperDotExpression SuperDot(string dotName) => TsMemberDotExpression.CreateSuper(dotName);

        public static ITsCallExpression Call(ITsExpression leftSide, params ITsArgument[] arguments) =>
            TsCallExpression.Create(leftSide, arguments);

        public static ITsNewCallExpression NewCall(ITsExpression leftSide, params ITsArgument[] arguments) =>
            TsCallExpression.CreateNew(leftSide, arguments);

        public static ITsSuperCallExpression SuperCall(params ITsArgument[] arguments) =>
            TsCallExpression.CreateSuper(arguments);

        public static ITsArgument Argument(ITsExpression argument, bool isSpreadArgument = false) =>
            new TsArgument(argument, isSpreadArgument);

        public static ITsNewTargetExpression NewTarget => TsNewTargetExpression.Instance;

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
            ITsExpression heritage = null,
            params ITsClassElement[] classBody)
        {
            return new TsClassExpression(className, heritage, classBody);
        }
    }
}
