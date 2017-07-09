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
        public virtual void VisitThis(ITsThis node) => Visit(node);

        /// <summary>
        /// Visits a null literal.
        /// </summary>
        public virtual void VisitNullLiteral(ITsNullLiteral node) => Visit(node);

        /// <summary>
        /// Visits a boolean literal.
        /// </summary>
        public virtual void VisitBooleanLiteral(ITsBooleanLiteral node) => Visit(node);

        /// <summary>
        /// Visits a numeric literal.
        /// </summary>
        public virtual void VisitNumericLiteral(ITsNumericLiteral node) => Visit(node);

        /// <summary>
        /// Visits a string literal.
        /// </summary>
        public virtual void VisitStringLiteral(ITsStringLiteral node) => Visit(node);

        /// <summary>
        /// Visits a regular expression literal.
        /// </summary>
        public virtual void VisitRegularExpressionLiteral(ITsRegularExpressionLiteral node) => Visit(node);

        /// <summary>
        /// Visits an array literal of the form '[element, element...]'.
        /// </summary>
        public virtual void VisitArrayLiteral(ITsArrayLiteral node) => Visit(node);

        /// <summary>
        /// Visits an array element.
        /// </summary>
        public virtual void VisitArrayElement(ITsArrayElement node) => Visit(node);

        /// <summary>
        /// Visits an object literal of the form '{ PropertDefinition, ... }'.
        /// </summary>
        public virtual void VisitObjectLiteral(ITsObjectLiteral node) => Visit(node);

        /// <summary>
        /// Visits an element in an object initializer of the form 'identifer = expression'.
        /// </summary>
        /// <param name="node"></param>
        public virtual void VisitCoverInitializedName(ITsCoverInitializedName node) => Visit(node);

        /// <summary>
        /// Visits a property value assignment within an object literal of the form 'property: value'.
        /// </summary>
        public virtual void VisitPropertyAssignment(ITsPropertyAssignment node) => Visit(node);

        /// <summary>
        /// Visits a computed property name within an object literal of the form '[expression]'.
        /// </summary>
        public virtual void VisitComputedPropertyName(ITsComputedPropertyName node) => Visit(node);

        /// <summary>
        /// Visits an object literal property function.
        /// </summary>
        public virtual void VisitPropertyFunction(ITsPropertyFunction node) => Visit(node);

        /// <summary>
        /// Visits a property get accessor of the form 'get name (): type { body }'.
        /// </summary>
        public virtual void VisitGetAccessor(ITsGetAccessor node) => Visit(node);

        /// <summary>
        /// Visits a property set accessor of the form 'set name(value: type) { body }'.
        /// </summary>
        public virtual void VisitSetAccessor(ITsSetAccessor node) => Visit(node);

        /// <summary>
        /// Visits an arrow function expression of the form '() => body'.
        /// </summary>
        public virtual void VisitArrowFunction(ITsArrowFunction node) => Visit(node);

        /// <summary>
        /// Visits a function declaration acting as an expression.
        /// </summary>
        public virtual void VisitFunctionExpression(ITsFunctionExpression node) => Visit(node);

        /// <summary>
        /// Visits an element within a class.
        /// </summary>
        public virtual void VisitClassElement(ITsClassElement node) => Visit(node);

        /// <summary>
        /// Visits a class declaration acting as an expression.
        /// </summary>
        public virtual void VisitClassExpression(ITsClassExpression node) => Visit(node);

        /// <summary>
        /// Visits a template literal of the form `string${Expression}`.
        /// </summary>
        public virtual void VisitTemplateLiteral(ITsTemplateLiteral node) => Visit(node);

        /// <summary>
        /// Visits a part of a template literal.
        /// </summary>
        public virtual void VisitTemplatePart(ITsTemplatePart node) => Visit(node);

        /// <summary>
        /// Visits a unary expression
        /// </summary>
        public virtual void VisitUnaryExpression(ITsUnaryExpression node) => Visit(node);

        /// <summary>
        /// Visits a unary cast expression of the form, '&lt;Type&gt;.
        /// </summary>
        public virtual void VisitCastExpression(ITsCastExpression node) => Visit(node);

        /// <summary>
        /// Visits a binary expression
        /// </summary>
        public virtual void VisitBinaryExpression(ITsBinaryExpression node) => Visit(node);

        /// <summary>
        /// Visits a conditional expression of the form 'x ? y : z'.
        /// </summary>
        public virtual void VisitConditionalExpression(ITsConditionalExpression node) => Visit(node);

        /// <summary>
        /// Visits an expression that assigns one value to another.
        /// </summary>
        public virtual void VisitAssignmentExpression(ITsAssignmentExpression node) => Visit(node);

        /// <summary>
        /// Visits a member expression of the form 'expression[expression]'.
        /// </summary>
        public virtual void VisitMemberBracketExpression(ITsMemberBracketExpression node) => Visit(node);

        /// <summary>
        /// Visits a member expression of the form 'super[expression]'.
        /// </summary>
        public virtual void VisitSuperBracketExpression(ITsSuperBracketExpression node) => Visit(node);

        /// <summary>
        /// Visits a member expression of the form 'expression.name'.
        /// </summary>
        public virtual void VisitMemberDotExpression(ITsMemberDotExpression node) => Visit(node);

        /// <summary>
        /// Visits a member expression of the form 'super.name'.
        /// </summary>
        public virtual void VisitSuperDotExpression(ITsSuperDotExpression node) => Visit(node);

        /// <summary>
        /// Visits a call expression of the form 'expression(arguments)'.
        /// </summary>
        public virtual void VisitCallExpression(ITsCallExpression node) => Visit(node);

        /// <summary>
        /// Visits a new call expression of the form 'new expression(arguments)'.
        /// </summary>
        public virtual void VisitNewCallExpression(ITsNewCallExpression node) => Visit(node);

        /// <summary>
        /// Visits a super call expression of the form 'super(arguments)'.
        /// </summary>
        public virtual void VisitSuperCallExpression(ITsSuperCallExpression node) => Visit(node);

        /// <summary>
        /// Visits an argument in a call expression.
        /// </summary>
        public virtual void VisitArgument(ITsArgument node) => Visit(node);

        /// <summary>
        /// Visits an argument list of the form '&lt;T&gt;(x: type, y: type).
        /// </summary>
        public virtual void VisitArgumentList(ITsArgumentList node) => Visit(node);

        /// <summary>
        /// Visits an expression of the form 'new.target'.
        /// </summary>
        public virtual void VisitNewTargetExpression(ITsNewTargetExpression node) => Visit(node);
    }
}
