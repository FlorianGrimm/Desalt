// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
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
            return new TsCommaExpression(expressions);
        }

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

        public static ITsNumericLiteral Number(double value)
        {
            return new TsNumericLiteral(TsNumericLiteralKind.Decimal, value);
        }

        public static ITsNumericLiteral BinaryInteger(long value)
        {
            return new TsNumericLiteral(TsNumericLiteralKind.BinaryInteger, value);
        }

        public static ITsNumericLiteral OctalInteger(long value)
        {
            return new TsNumericLiteral(TsNumericLiteralKind.OctalInteger, value);
        }

        public static ITsNumericLiteral HexInteger(long value)
        {
            return new TsNumericLiteral(TsNumericLiteralKind.HexInteger, value);
        }

        public static ITsRegularExpressionLiteral RegularExpression(string body, string flags)
        {
            return new TsRegularExpressionLiteral(body, flags);
        }

        public static ITsArrayLiteral Array()
        {
            return new TsArrayLiteral(null);
        }

        public static ITsArrayLiteral Array(params ITsArrayElement[] elements)
        {
            return new TsArrayLiteral(elements);
        }

        public static ITsArrayLiteral Array(params ITsExpression[] elements)
        {
            return new TsArrayLiteral(elements?.Select(e => ArrayElement(e)));
        }

        public static ITsArrayElement ArrayElement(ITsExpression element, bool isSpreadElement = false)
        {
            return new TsArrayElement(element, isSpreadElement);
        }

        public static ITsTemplateLiteral TemplateString(params ITsTemplatePart[] parts)
        {
            return new TsTemplateLiteral(parts);
        }

        public static ITsTemplatePart TemplatePart(string template = null, ITsExpression expression = null)
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
            return new TsObjectLiteral(propertyDefinitions);
        }

        public static ITsObjectLiteral Object(params ITsPropertyDefinition[] propertyDefinitions)
        {
            return new TsObjectLiteral(propertyDefinitions);
        }

        /// <summary>
        /// Creates an element in an object initializer of the form 'identifer = expression'.
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
            ITsType propertyType = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsGetAccessor(propertyName, propertyType, functionBody);
        }

        public static ITsGetAccessor GetAccessor(
            ITsPropertyName propertyName,
            ITsType propertyType,
            params ITsStatementListItem[] functionBody)
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

        public static ITsSetAccessor SetAccessor(
            ITsPropertyName propertyName,
            ITsBindingIdentifierOrPattern parameterName,
            ITsType parameterType,
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
            return TsMemberBracketExpression.Create(leftSide, bracketContents);
        }

        public static ITsMemberDotExpression MemberDot(ITsExpression leftSide, string dotName)
        {
            return TsMemberDotExpression.Create(leftSide, dotName);
        }

        public static ITsSuperBracketExpression SuperBracket(ITsExpression bracketContents)
        {
            return TsMemberBracketExpression.CreateSuper(bracketContents);
        }

        public static ITsSuperDotExpression SuperDot(string dotName)
        {
            return TsMemberDotExpression.CreateSuper(dotName);
        }

        public static ITsCallExpression Call(ITsExpression leftSide, ITsArgumentList arguments = null)
        {
            return TsCallExpression.Create(leftSide, arguments);
        }

        public static ITsNewCallExpression NewCall(ITsExpression leftSide, ITsArgumentList arguments = null)
        {
            return TsCallExpression.CreateNew(leftSide, arguments);
        }

        public static ITsSuperCallExpression SuperCall(ITsArgumentList arguments)
        {
            return TsCallExpression.CreateSuper(arguments);
        }

        public static ITsArgument Argument(ITsExpression argument, bool isSpreadArgument = false)
        {
            return new TsArgument(argument, isSpreadArgument);
        }

        public static ITsArgumentList ArgumentList()
        {
            return new TsArgumentList();
        }

        /// <summary>
        /// Represents an argument list of the form '&lt;T&gt;(x: type, y: type).
        /// </summary>
        public static ITsArgumentList ArgumentList(IEnumerable<ITsType> typeArguments, params ITsArgument[] arguments)
        {
            return new TsArgumentList(typeArguments, arguments);
        }

        /// <summary>
        /// Represents an argument list of the form '&lt;T&gt;(x: type, y: type).
        /// </summary>
        public static ITsArgumentList ArgumentList(params ITsArgument[] arguments)
        {
            return new TsArgumentList(typeArguments: null, arguments: arguments);
        }

        /// <summary>
        /// Represents an argument list of the form '&lt;T&gt;(x: type, y: type).
        /// </summary>
        public static ITsArgumentList ArgumentList(params ITsIdentifier[] arguments)
        {
            return new TsArgumentList(typeArguments: null, arguments: arguments.Select(id => Argument(id)));
        }

        public static ITsNewTargetExpression NewTarget => TsNewTargetExpression.Instance;

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
            ITsIdentifier functionName = null,
            params ITsStatementListItem[] functionBody)
        {
            return new TsFunctionExpression(callSignature, functionName, functionBody);
        }

        public static ITsClassExpression ClassExpression(
            ITsIdentifier className = null,
            ITsClassHeritage heritage = null,
            params ITsClassElement[] classBody)
        {
            return new TsClassExpression(className, heritage, classBody);
        }
    }
}
