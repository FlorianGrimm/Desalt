// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Visitor.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast
{
    using Desalt.JavaScript.Ast.Expressions;

    public abstract partial class Es5Visitor
    {
        public virtual void VisitThisExpresssion(Es5ThisExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits a literal expression, where a literal can be an identifier, keyword, string,
        /// number, or regular expression.
        /// </summary>
        public virtual void VisitLiteralExpression(Es5LiteralExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits an assignment expression of the form 'x = y', where '=' can be any valid
        /// assignment expression operator.
        /// </summary>
        public virtual void VisitAssignmentExpression(Es5AssignmentExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits an array literal of the form '[element, element...]'.
        /// </summary>
        public virtual void VisitArrayLiteralExpression(Es5ArrayLiteralExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits an object literal of the form '{ propertyAssignment, propertyAssignment... }'.
        /// </summary>
        public virtual void VisitObjectLiteralExpression(Es5ObjectLiteralExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits a property get assignment within an object literal of the form 'get property() {}'.
        /// </summary>
        public virtual void VisitPropertyGetAssignment(Es5PropertyGetAssignment node) => DefaultVisit(node);

        /// <summary>
        /// Visits a property set assignment within an object literal of the form 'set property(value) {}'.
        /// </summary>
        public virtual void VisitPropertySetAssignment(Es5PropertySetAssignment node) => DefaultVisit(node);

        /// <summary>
        /// Visits a property value assignment within an object literal of the form 'property: value'.
        /// </summary>
        public virtual void VisitPropertyValueAssignment(Es5PropertyValueAssignment node) => DefaultVisit(node);

        /// <summary>
        /// Visits an expression surrounded by parentheses.
        /// </summary>
        public virtual void VisitParenthesizedExpression(Es5ParenthesizedExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits an expression that is a function declaration.
        /// </summary>
        public virtual void VisitFunctionExpression(Es5FunctionExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits a member expression of the form 'expression[member]', 'expression.member', or 'new expression(args)'.
        /// </summary>
        public virtual void VisitMemberExpression(Es5MemberExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits a call expression of the form 'expression(args)'.
        /// </summary>
        public virtual void VisitCallExpression(Es5CallExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits a unary expression.
        /// </summary>
        public virtual void VisitUnaryExpression(Es5UnaryExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits a binary expression of the form 'x ? y' where ? represents any of the enum values
        /// from <see cref="Es5BinaryOperator"/>.
        /// </summary>
        public virtual void VisitBinaryExpression(Es5BinaryExpression node) => DefaultVisit(node);

        /// <summary>
        /// Visits a conditional expression of the form 'x ? y : z'.
        /// </summary>
        public virtual void VisitConditionalExpression(Es5ConditionalExpression node) => DefaultVisit(node);
    }
}
