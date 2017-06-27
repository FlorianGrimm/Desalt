// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsVisitor.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    public abstract partial class TsVisitor
    {
        /// <summary>
        /// Visits an expression representing the literal 'this'.
        /// </summary>
        public virtual void VisitThis(ITsThis node) => DefaultVisit(node);

        /// <summary>
        /// Visits a null literal.
        /// </summary>
        public virtual void VisitNullLiteral(ITsNullLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits a boolean literal.
        /// </summary>
        public virtual void VisitBooleanLiteral(ITsBooleanLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits a numeric literal.
        /// </summary>
        public virtual void VisitNumericLiteral(ITsNumericLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits a string literal.
        /// </summary>
        public virtual void VisitStringLiteral(ITsStringLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits a regular expression literal.
        /// </summary>
        public virtual void VisitRegularExpressionLiteral(ITsRegularExpressionLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits an array literal of the form '[element, element...]'.
        /// </summary>
        public virtual void VisitArrayLiteral(ITsArrayLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits an array element.
        /// </summary>
        public virtual void VisitArrayElement(ITsArrayElement node) => DefaultVisit(node);

        /// <summary>
        /// Visits an object literal of the form '{ PropertDefinition, ... }'.
        /// </summary>
        public virtual void VisitObjectLiteral(ITsObjectLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits an element in an object initializer of the form 'identifer = expression'.
        /// </summary>
        /// <param name="node"></param>
        public virtual void VisitCoverInitializedName(ITsCoverInitializedName node) => DefaultVisit(node);

        /// <summary>
        /// Visits a property value assignment within an object literal of the form 'property: value'.
        /// </summary>
        public virtual void VisitPropertyAssignment(ITsPropertyAssignment node) => DefaultVisit(node);

        /// <summary>
        /// Visits a computed property name within an object literal of the form '[expression]'.
        /// </summary>
        public virtual void VisitComputedPropertyName(ITsComputedPropertyName node) => DefaultVisit(node);

        /// <summary>
        /// Visits an object literal property function.
        /// </summary>
        public virtual void VisitPropertyFunction(ITsPropertyFunction node) => DefaultVisit(node);

        /// <summary>
        /// Visits a property get accessor of the form 'get name (): type { body }'.
        /// </summary>
        public virtual void VisitGetAccessor(ITsGetAccessor node) => DefaultVisit(node);

        /// <summary>
        /// Visits a property set accessor of the form 'set name(value: type) { body }'.
        /// </summary>
        public virtual void VisitSetAccessor(ITsSetAccessor node) => DefaultVisit(node);

        /// <summary>
        /// Visits a function declaration acting as an expression.
        /// </summary>
        public virtual void VisitFunctionExpression(ITsFunctionExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits an element within a class.
        /// </summary>
        public virtual void VisitClassElement(ITsClassElement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a class declaration acting as an expression.
        /// </summary>
        public virtual void VisitClassExpression(ITsClassExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits a template literal of the form `string${Expression}`.
        /// </summary>
        public virtual void VisitTemplateLiteral(ITsTemplateLiteral node) => DefaultVisit(node);
    }

    public abstract partial class TsVisitor<TResult>
    {
        /// <summary>
        /// Visits an expression representing the literal 'this'.
        /// </summary>
        public virtual TResult VisitThis(ITsThis node) => DefaultVisit(node);

        /// <summary>
        /// Visits a null literal.
        /// </summary>
        public virtual TResult VisitNullLiteral(ITsNullLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits a boolean literal.
        /// </summary>
        public virtual TResult VisitBooleanLiteral(ITsBooleanLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits a numeric literal.
        /// </summary>
        public virtual TResult VisitNumericLiteral(ITsNumericLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits a string literal.
        /// </summary>
        public virtual TResult VisitStringLiteral(ITsStringLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits a regular expression literal.
        /// </summary>
        public virtual TResult VisitRegularExpressionLiteral(ITsRegularExpressionLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits an array literal of the form '[element, element...]'.
        /// </summary>
        public virtual TResult VisitArrayLiteral(ITsArrayLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits an array element.
        /// </summary>
        public virtual TResult VisitArrayElement(ITsArrayElement node) => DefaultVisit(node);

        /// <summary>
        /// Visits an object literal of the form '{ PropertDefinition, ... }'.
        /// </summary>
        public virtual TResult VisitObjectLiteral(ITsObjectLiteral node) => DefaultVisit(node);

        /// <summary>
        /// Visits an element in an object initializer of the form 'identifer = expression'.
        /// </summary>
        /// <param name="node"></param>
        public virtual TResult VisitCoverInitializedName(ITsCoverInitializedName node) => DefaultVisit(node);

        /// <summary>
        /// Visits a property value assignment within an object literal of the form 'property: value'.
        /// </summary>
        public virtual TResult VisitPropertyAssignment(ITsPropertyAssignment node) => DefaultVisit(node);

        /// <summary>
        /// Visits a computed property name within an object literal of the form '[expression]'.
        /// </summary>
        public virtual TResult VisitComputedPropertyName(ITsComputedPropertyName node) => DefaultVisit(node);

        /// <summary>
        /// Visits an object literal property function.
        /// </summary>
        public virtual TResult VisitPropertyFunction(ITsPropertyFunction node) => DefaultVisit(node);

        /// <summary>
        /// Visits a property get accessor of the form 'get name (): type { body }'.
        /// </summary>
        public virtual TResult VisitGetAccessor(ITsGetAccessor node) => DefaultVisit(node);

        /// <summary>
        /// Visits a property set accessor of the form 'set name(value: type) { body }'.
        /// </summary>
        public virtual TResult VisitSetAccessor(ITsSetAccessor node) => DefaultVisit(node);

        /// <summary>
        /// Visits a function declaration acting as an expression.
        /// </summary>
        public virtual TResult VisitFunctionExpression(ITsFunctionExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits an element within a class.
        /// </summary>
        public virtual TResult VisitClassElement(ITsClassElement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a class declaration acting as an expression.
        /// </summary>
        public virtual TResult VisitClassExpression(ITsClassExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits a template literal of the form `string${Expression}`.
        /// </summary>
        public virtual TResult VisitTemplateLiteral(ITsTemplateLiteral node) => DefaultVisit(node);
    }
}
