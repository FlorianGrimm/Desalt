// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.TypeScriptAst.Ast.Expressions;

    public static partial class TsAstFactory
    {
        /// <summary>
        /// Creates an expression list separated by commas. Useful in for loops for the initializer
        /// or incrementor, for example.
        /// </summary>
        public static ITsCommaExpression CommaExpression(params ITsExpression[] expressions)
        {
            return new TsCommaExpression(expressions.ToImmutableArray());
        }

        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        public static readonly ITsThis This = new TsThis();

        public static readonly ITsObjectLiteral EmptyObject =
            new TsObjectLiteral(ImmutableArray<ITsPropertyDefinition>.Empty);

        public static readonly ITsNullLiteral Null = new TsNullLiteral();
        public static readonly ITsBooleanLiteral True = new TsBooleanLiteral(true);
        public static readonly ITsBooleanLiteral False = new TsBooleanLiteral(false);

        public static readonly ITsNumericLiteral Zero = new TsNumericLiteral(0, TsNumericLiteralKind.Decimal);

        public static ITsStringLiteral String(
            string value,
            StringLiteralQuoteKind quoteKind = StringLiteralQuoteKind.SingleQuote)
        {
            return new TsStringLiteral(value, quoteKind);
        }

        public static ITsNumericLiteral Number(double value)
        {
            return new TsNumericLiteral(value, TsNumericLiteralKind.Decimal);
        }

        public static ITsNumericLiteral BinaryInteger(long value)
        {
            return new TsNumericLiteral(value, TsNumericLiteralKind.BinaryInteger);
        }

        public static ITsNumericLiteral OctalInteger(long value)
        {
            return new TsNumericLiteral(value, TsNumericLiteralKind.OctalInteger);
        }

        public static ITsNumericLiteral HexInteger(long value)
        {
            return new TsNumericLiteral(value, TsNumericLiteralKind.HexInteger);
        }

        public static ITsRegularExpressionLiteral RegularExpression(string body, string? flags)
        {
            return new TsRegularExpressionLiteral(body, flags);
        }

        public static ITsArrayLiteral Array()
        {
            return new TsArrayLiteral(ImmutableArray<ITsArrayElement?>.Empty);
        }

        public static ITsArrayLiteral Array(params ITsArrayElement?[] elements)
        {
            return new TsArrayLiteral(elements.ToImmutableArray());
        }

        public static ITsArrayLiteral Array(params ITsExpression?[] elements)
        {
            return new TsArrayLiteral(elements.Select(e => e == null ? null : ArrayElement(e)).ToImmutableArray());
        }

        public static ITsArrayElement ArrayElement(ITsExpression expression, bool isSpreadElement = false)
        {
            return new TsArrayElement(expression, isSpreadElement);
        }

        public static ITsTemplateLiteral TemplateString(params ITsTemplatePart[] parts)
        {
            return new TsTemplateLiteral(parts.ToImmutableArray());
        }

        public static ITsTemplatePart TemplatePart(string template, ITsExpression? expression = null)
        {
            return new TsTemplatePart(template, expression);
        }

        public static ITsParenthesizedExpression ParenthesizedExpression(ITsExpression expression)
        {
            return new TsParenthesizedExpression(expression);
        }

        //// ===========================================================================================================
        //// Object Literal Expressions
        //// ===========================================================================================================

        public static ITsObjectLiteral Object(IEnumerable<ITsPropertyDefinition> propertyDefinitions)
        {
            return new TsObjectLiteral(propertyDefinitions.ToImmutableArray());
        }

        public static ITsObjectLiteral Object(params ITsPropertyDefinition[] propertyDefinitions)
        {
            return new TsObjectLiteral(propertyDefinitions.ToImmutableArray());
        }

        /// <summary>
        /// Creates an element in an object initializer of the form 'identifier = expression'.
        /// </summary>
        public static ITsCoverInitializedName CoverInitializedName(ITsIdentifier identifier, ITsExpression initializer)
        {
            return new TsCoverInitializedName(identifier, initializer);
        }

        /// <summary>
        /// Creates a property assignment in the following form: 'propertyName: value'.
        /// </summary>
        public static ITsPropertyAssignment PropertyAssignment(ITsPropertyName propertyName, ITsExpression initializer)
        {
            return new TsPropertyAssignment(propertyName, initializer);
        }

        /// <summary>
        /// Creates a computed property name in the following form: '[expression]'.
        /// </summary>
        public static ITsComputedPropertyName ComputedPropertyName(ITsExpression expression)
        {
            return new TsComputedPropertyName(expression);
        }

        public static ITsPropertyFunction PropertyFunction(
            ITsPropertyName propertyName,
            ITsCallSignature callSignature,
            params ITsStatementListItem[] functionBody)
        {
            return new TsPropertyFunction(propertyName, callSignature, functionBody);
        }

        public static ITsGetAccessor GetAccessor(
            ITsPropertyName propertyName,
            ITsType? propertyType = null,
            IEnumerable<ITsStatementListItem>? functionBody = null)
        {
            return new TsGetAccessor(propertyName, propertyType, functionBody);
        }

        public static ITsGetAccessor GetAccessor(
            ITsPropertyName propertyName,
            ITsType? propertyType,
            params ITsStatementListItem[] functionBody)
        {
            return new TsGetAccessor(propertyName, propertyType, functionBody);
        }

        public static ITsSetAccessor SetAccessor(
            ITsPropertyName propertyName,
            ITsBindingIdentifierOrPattern parameterName,
            ITsType? parameterType = null,
            IEnumerable<ITsStatementListItem>? functionBody = null)
        {
            return new TsSetAccessor(propertyName, parameterName, parameterType, functionBody);
        }

        public static ITsSetAccessor SetAccessor(
            ITsPropertyName propertyName,
            ITsBindingIdentifierOrPattern parameterName,
            ITsType? parameterType,
            params ITsStatementListItem[] functionBody)
        {
            return new TsSetAccessor(propertyName, parameterName, parameterType, functionBody);
        }

        //// ===========================================================================================================
        //// Unary and Binary Expressions
        //// ===========================================================================================================

        public static ITsUnaryExpression UnaryExpression(ITsExpression operand, TsUnaryOperator @operator)
        {
            return new TsUnaryExpression(operand, @operator);
        }

        /// <summary>
        /// Create a unary cast expression of the form, '&lt;Type&gt;.
        /// </summary>
        public static ITsCastExpression Cast(ITsType castType, ITsExpression expression)
        {
            return new TsCastExpression(castType, expression);
        }

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

        public static ITsAssignmentExpression Assignment(
            ITsExpression leftSide,
            TsAssignmentOperator @operator,
            ITsExpression rightSide)
        {
            return new TsAssignmentExpression(leftSide, @operator, rightSide);
        }

        //// ===========================================================================================================
        //// Left-hand Side Expressions
        //// ===========================================================================================================

        public static ITsMemberBracketExpression MemberBracket(ITsExpression leftSide, ITsExpression bracketContents)
        {
            return new TsMemberBracketExpression(leftSide, bracketContents);
        }

        public static ITsMemberDotExpression MemberDot(ITsExpression leftSide, string dotName)
        {
            return new TsMemberDotExpression(leftSide, dotName);
        }

        public static ITsMemberDotExpression MemberDot(ITsExpression leftSide, ITsIdentifier dotName)
        {
            return new TsMemberDotExpression(leftSide, dotName.Text);
        }

        public static ITsSuperBracketExpression SuperBracket(ITsExpression bracketContents)
        {
            return new TsSuperBracketExpression(bracketContents);
        }

        public static ITsSuperDotExpression SuperDot(string dotName)
        {
            return new TsSuperDotExpression(dotName);
        }

        public static ITsCallExpression Call(ITsExpression leftSide, ITsArgumentList? argumentList = null)
        {
            return new TsCallExpression(leftSide, argumentList ?? ArgumentList());
        }

        public static ITsNewCallExpression NewCall(ITsExpression leftSide, ITsArgumentList? argumentList = null)
        {
            return new TsNewCallExpression(leftSide, argumentList ?? ArgumentList());
        }

        public static ITsSuperCallExpression SuperCall(ITsArgumentList argumentList)
        {
            return new TsSuperCallExpression(argumentList);
        }

        public static ITsArgument Argument(ITsExpression expression, bool isSpreadArgument = false)
        {
            return new TsArgument(expression, isSpreadArgument);
        }

        public static ITsArgumentList ArgumentList()
        {
            return new TsArgumentList(ImmutableArray<ITsType>.Empty, ImmutableArray<ITsArgument>.Empty);
        }

        /// <summary>
        /// Represents an argument list of the form '&lt;T&gt;(x: type, y: type).
        /// </summary>
        public static ITsArgumentList ArgumentList(IEnumerable<ITsType>? typeArguments, params ITsArgument[] arguments)
        {
            return new TsArgumentList(
                typeArguments?.ToImmutableArray() ?? ImmutableArray<ITsType>.Empty,
                arguments.ToImmutableArray());
        }

        /// <summary>
        /// Represents an argument list of the form '&lt;T&gt;(x: type, y: type).
        /// </summary>
        public static ITsArgumentList ArgumentList(params ITsArgument[] arguments)
        {
            return new TsArgumentList(
                typeArguments: ImmutableArray<ITsType>.Empty,
                arguments: arguments.ToImmutableArray());
        }

        /// <summary>
        /// Represents an argument list of the form '&lt;T&gt;(x: type, y: type).
        /// </summary>
        public static ITsArgumentList ArgumentList(params ITsIdentifier[] arguments)
        {
            return new TsArgumentList(
                typeArguments: ImmutableArray<ITsType>.Empty,
                arguments: arguments.Select(id => Argument(id)).ToImmutableArray());
        }

        public static readonly ITsNewTargetExpression NewTarget = new TsNewTargetExpression();

        //// ===========================================================================================================
        //// Function and Class Expressions
        //// ===========================================================================================================

        public static ITsArrowFunction ArrowFunction(ITsIdentifier singleParameterName, ITsExpression bodyExpression)
        {
            return new TsArrowFunction(singleParameterName, bodyExpression);
        }

        public static ITsArrowFunction ArrowFunction(
            ITsIdentifier singleParameterName,
            params ITsStatementListItem[] body)
        {
            return new TsArrowFunction(singleParameterName, body);
        }

        public static ITsArrowFunction ArrowFunction(ITsCallSignature callSignature, ITsExpression bodyExpression)
        {
            return new TsArrowFunction(callSignature, bodyExpression);
        }

        public static ITsArrowFunction ArrowFunction(
            ITsCallSignature callSignature,
            params ITsStatementListItem[] body)
        {
            return new TsArrowFunction(callSignature, body);
        }

        /// <summary>
        /// Represents a function declaration acting as an expression.
        /// </summary>
        public static ITsFunctionExpression FunctionExpression(
            ITsCallSignature callSignature,
            ITsIdentifier? functionName = null,
            params ITsStatementListItem[] functionBody)
        {
            return new TsFunctionExpression(callSignature, functionName, functionBody);
        }

        public static ITsClassExpression ClassExpression(
            ITsIdentifier? className = null,
            ITsClassHeritage? heritage = null,
            params ITsClassElement[] classBody)
        {
            return new TsClassExpression(className, heritage, classBody);
        }
    }
}
