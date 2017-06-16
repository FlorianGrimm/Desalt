// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptVisitor.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    public abstract partial class TypeScriptVisitor
    {
        /// <summary>
        /// Visits an expression representing the literal 'this'.
        /// </summary>
        public virtual void VisitThis(ITsThis model) => DefaultVisit(model);

        /// <summary>
        /// Visits a null literal.
        /// </summary>
        public virtual void VisitNullLiteral(ITsNullLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a boolean literal.
        /// </summary>
        public virtual void VisitBooleanLiteral(ITsBooleanLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a numeric literal.
        /// </summary>
        public virtual void VisitNumericLiteral(ITsNumericLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a string literal.
        /// </summary>
        public virtual void VisitStringLiteral(ITsStringLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a regular expression literal.
        /// </summary>
        public virtual void VisitRegularExpressionLiteral(ITsRegularExpressionLiteral model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits an assignment expression of the form 'x = y', where '=' can be any valid
        ///// assignment expression operator.
        ///// </summary>
        //public virtual void VisitAssignmentExpression(TypeScriptAssignmentExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits an array literal of the form '[element, element...]'.
        /// </summary>
        public virtual void VisitArrayLiteral(ITsArrayLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits an array element.
        /// </summary>
        public virtual void VisitArrayElement(ITsArrayElement model) => DefaultVisit(model);

        /// <summary>
        /// Visits an object literal of the form '{ PropertDefinition, ... }'.
        /// </summary>
        public virtual void VisitObjectLiteral(ITsObjectLiteral model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a property get assignment within an object literal of the form 'get property() {}'.
        ///// </summary>
        //public virtual void VisitPropertyGetAssignment(TypeScriptPropertyGetAssignment model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a property set assignment within an object literal of the form 'set property(value) {}'.
        ///// </summary>
        //public virtual void VisitPropertySetAssignment(TypeScriptPropertySetAssignment model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a property value assignment within an object literal of the form 'property: value'.
        ///// </summary>
        //public virtual void VisitPropertyValueAssignment(TypeScriptPropertyValueAssignment model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits an expression surrounded by parentheses.
        ///// </summary>
        //public virtual void VisitParenthesizedExpression(TypeScriptParenthesizedExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits an expression that is a function declaration.
        ///// </summary>
        //public virtual void VisitFunctionExpression(TypeScriptFunctionExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a member expression of the form 'expression[member]', 'expression.member', or 'new expression(args)'.
        ///// </summary>
        //public virtual void VisitMemberExpression(TypeScriptMemberExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a call expression of the form 'expression(args)'.
        ///// </summary>
        //public virtual void VisitCallExpression(TypeScriptCallExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a unary expression.
        ///// </summary>
        //public virtual void VisitUnaryExpression(TypeScriptUnaryExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a binary expression of the form 'x ? y' where ? represents any of the enum values
        ///// from <see cref="TypeScriptBinaryOperator"/>.
        ///// </summary>
        //public virtual void VisitBinaryExpression(TypeScriptBinaryExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a conditional expression of the form 'x ? y : z'.
        ///// </summary>
        //public virtual void VisitConditionalExpression(TypeScriptConditionalExpression model) => DefaultVisit(model);
    }

    public abstract partial class TypeScriptVisitor<TResult>
    {
        /// <summary>
        /// Visits an expression representing the literal 'this'.
        /// </summary>
        public virtual TResult VisitThis(ITsThis model) => DefaultVisit(model);

        /// <summary>
        /// Visits a null literal.
        /// </summary>
        public virtual TResult VisitNullLiteral(ITsNullLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a boolean literal.
        /// </summary>
        public virtual TResult VisitBooleanLiteral(ITsBooleanLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a numeric literal.
        /// </summary>
        public virtual TResult VisitNumericLiteral(ITsNumericLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a string literal.
        /// </summary>
        public virtual TResult VisitStringLiteral(ITsStringLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a regular expression literal.
        /// </summary>
        public virtual TResult VisitRegularExpressionLiteral(ITsRegularExpressionLiteral model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits an assignment expression of the form 'x = y', where '=' can be any valid
        ///// assignment expression operator.
        ///// </summary>
        //public virtual TResult VisitAssignmentExpression(TypeScriptAssignmentExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits an array literal of the form '[element, element...]'.
        /// </summary>
        public virtual TResult VisitArrayLiteral(ITsArrayLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits an array element.
        /// </summary>
        public virtual TResult VisitArrayElement(ITsArrayElement model) => DefaultVisit(model);

        /// <summary>
        /// Visits an object literal of the form '{ PropertDefinition, ... }'.
        /// </summary>
        public virtual TResult VisitObjectLiteral(ITsObjectLiteral model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a property get assignment within an object literal of the form 'get property() {}'.
        ///// </summary>
        //public virtual TResult VisitPropertyGetAssignment(TypeScriptPropertyGetAssignment model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a property set assignment within an object literal of the form 'set property(value) {}'.
        ///// </summary>
        //public virtual TResult VisitPropertySetAssignment(TypeScriptPropertySetAssignment model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a property value assignment within an object literal of the form 'property: value'.
        ///// </summary>
        //public virtual TResult VisitPropertyValueAssignment(TypeScriptPropertyValueAssignment model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits an expression surrounded by parentheses.
        ///// </summary>
        //public virtual TResult VisitParenthesizedExpression(TypeScriptParenthesizedExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits an expression that is a function declaration.
        ///// </summary>
        //public virtual TResult VisitFunctionExpression(TypeScriptFunctionExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a member expression of the form 'expression[member]', 'expression.member', or 'new expression(args)'.
        ///// </summary>
        //public virtual TResult VisitMemberExpression(TypeScriptMemberExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a call expression of the form 'expression(args)'.
        ///// </summary>
        //public virtual TResult VisitCallExpression(TypeScriptCallExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a unary expression.
        ///// </summary>
        //public virtual TResult VisitUnaryExpression(TypeScriptUnaryExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a binary expression of the form 'x ? y' where ? represents any of the enum values
        ///// from <see cref="TypeScriptBinaryOperator"/>.
        ///// </summary>
        //public virtual TResult VisitBinaryExpression(TypeScriptBinaryExpression model) => DefaultVisit(model);

        ///// <summary>
        ///// Visits a conditional expression of the form 'x ? y : z'.
        ///// </summary>
        //public virtual TResult VisitConditionalExpression(TypeScriptConditionalExpression model) => DefaultVisit(model);
    }
}
