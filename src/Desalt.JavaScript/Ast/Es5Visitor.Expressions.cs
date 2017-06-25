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
        public virtual void VisitThisExpresssion(Es5ThisExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a literal expression, where a literal can be an identifier, keyword, string,
        /// number, or regular expression.
        /// </summary>
        public virtual void VisitLiteralExpression(Es5LiteralExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits an assignment expression of the form 'x = y', where '=' can be any valid
        /// assignment expression operator.
        /// </summary>
        public virtual void VisitAssignmentExpression(Es5AssignmentExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits an array literal of the form '[element, element...]'.
        /// </summary>
        public virtual void VisitArrayLiteralExpression(Es5ArrayLiteralExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits an object literal of the form '{ propertyAssignment, propertyAssignment... }'.
        /// </summary>
        public virtual void VisitObjectLiteralExpression(Es5ObjectLiteralExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a property get assignment within an object literal of the form 'get property() {}'.
        /// </summary>
        public virtual void VisitPropertyGetAssignment(Es5PropertyGetAssignment model) => DefaultVisit(model);

        /// <summary>
        /// Visits a property set assignment within an object literal of the form 'set property(value) {}'.
        /// </summary>
        public virtual void VisitPropertySetAssignment(Es5PropertySetAssignment model) => DefaultVisit(model);

        /// <summary>
        /// Visits a property value assignment within an object literal of the form 'property: value'.
        /// </summary>
        public virtual void VisitPropertyValueAssignment(Es5PropertyValueAssignment model) => DefaultVisit(model);

        /// <summary>
        /// Visits an expression surrounded by parentheses.
        /// </summary>
        public virtual void VisitParenthesizedExpression(Es5ParenthesizedExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits an expression that is a function declaration.
        /// </summary>
        public virtual void VisitFunctionExpression(Es5FunctionExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a member expression of the form 'expression[member]', 'expression.member', or 'new expression(args)'.
        /// </summary>
        public virtual void VisitMemberExpression(Es5MemberExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a call expression of the form 'expression(args)'.
        /// </summary>
        public virtual void VisitCallExpression(Es5CallExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a unary expression.
        /// </summary>
        public virtual void VisitUnaryExpression(Es5UnaryExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a binary expression of the form 'x ? y' where ? represents any of the enum values
        /// from <see cref="Es5BinaryOperator"/>.
        /// </summary>
        public virtual void VisitBinaryExpression(Es5BinaryExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a conditional expression of the form 'x ? y : z'.
        /// </summary>
        public virtual void VisitConditionalExpression(Es5ConditionalExpression model) => DefaultVisit(model);
    }

    public abstract partial class Es5Visitor<TResult>
    {
        public virtual TResult VisitThisExpresssion(Es5ThisExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a literal expression, where a literal can be an identifier, keyword, string,
        /// number, or regular expression.
        /// </summary>
        public virtual TResult VisitLiteralExpression(Es5LiteralExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits an assignment expression of the form 'x = y', where '=' can be any valid
        /// assignment expression operator.
        /// </summary>
        public virtual TResult VisitAssignmentExpression(Es5AssignmentExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits an array literal of the form '[element, element...]'.
        /// </summary>
        public virtual TResult VisitArrayLiteralExpression(Es5ArrayLiteralExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits an object literal of the form '{ propertyAssignment, propertyAssignment... }'.
        /// </summary>
        public virtual TResult VisitObjectLiteralExpression(Es5ObjectLiteralExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a property get assignment within an object literal of the form 'get property() {}'.
        /// </summary>
        public virtual TResult VisitPropertyGetAssignment(Es5PropertyGetAssignment model) => DefaultVisit(model);

        /// <summary>
        /// Visits a property set assignment within an object literal of the form 'set property(value) {}'.
        /// </summary>
        public virtual TResult VisitPropertySetAssignment(Es5PropertySetAssignment model) => DefaultVisit(model);

        /// <summary>
        /// Visits a property value assignment within an object literal of the form 'property: value'.
        /// </summary>
        public virtual TResult VisitPropertyValueAssignment(Es5PropertyValueAssignment model) => DefaultVisit(model);

        /// <summary>
        /// Visits an expression surrounded by parentheses.
        /// </summary>
        public virtual TResult VisitParenthesizedExpression(Es5ParenthesizedExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits an expression that is a function declaration.
        /// </summary>
        public virtual TResult VisitFunctionExpression(Es5FunctionExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a member expression of the form 'expression[member]', 'expression.member', or 'new expression(args)'.
        /// </summary>
        public virtual TResult VisitMemberExpression(Es5MemberExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a call expression of the form 'expression(args)'.
        /// </summary>
        public virtual TResult VisitCallExpression(Es5CallExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a unary expression.
        /// </summary>
        public virtual TResult VisitUnaryExpression(Es5UnaryExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a binary expression of the form 'x ? y' where ? represents any of the enum values
        /// from <see cref="Es5BinaryOperator"/>.
        /// </summary>
        public virtual TResult VisitBinaryExpression(Es5BinaryExpression model) => DefaultVisit(model);

        /// <summary>
        /// Visits a conditional expression of the form 'x ? y : z'.
        /// </summary>
        public virtual TResult VisitConditionalExpression(Es5ConditionalExpression model) => DefaultVisit(model);
    }
}
