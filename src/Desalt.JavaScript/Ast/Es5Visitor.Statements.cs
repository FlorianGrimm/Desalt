// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Visitor.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast
{
    using Desalt.JavaScript.Ast.Statements;

    public abstract partial class Es5Visitor
    {
        public virtual void VisitBlockStatement(Es5BlockStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a variable declaration statement of the form 'var x' or 'var x = y, z'.
        /// </summary>
        public virtual void VisitVariableStatement(Es5VariableStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits an empty statement.
        /// </summary>
        public virtual void VisitEmptyStatement(Es5EmptyStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits an expression that is represented as a statement.
        /// </summary>
        public virtual void VisitExpressionStatement(Es5ExpressionStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'continue' or 'continue Identifier' statement.
        /// </summary>
        public virtual void VisitContinueStatement(Es5ContinueStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'break' or 'break Identifier' statement.
        /// </summary>
        public virtual void VisitBreakStatement(Es5BreakStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits an 'if (x) statement; else statement;' statement.
        /// </summary>
        public virtual void VisitIfStatement(Es5IfStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a return statement of the form 'return expression;'.
        /// </summary>
        public virtual void VisitReturnStatement(Es5ReturnStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'with (Expression) Statement' statement.
        /// </summary>
        public virtual void VisitWithStatement(Es5WithStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a labelled statement of the form 'Identifier: Statement'.
        /// </summary>
        public virtual void VisitLabelledStatement(Es5LabelledStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'switch' statement.
        /// </summary>
        public virtual void VisitSwitchStatement(Es5SwitchStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'case' clause.
        /// </summary>
        public virtual void VisitCaseClause(Es5CaseClause node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'throw' statement.
        /// </summary>
        public virtual void VisitThrowStatement(Es5ThrowStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'try/catch/finally' statement.
        /// </summary>
        public virtual void VisitTryStatement(Es5TryStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'debugger' statement.
        /// </summary>
        public virtual void VisitDebuggerStatement(Es5DebuggerStatement node) => DefaultVisit(node);
    }

    public abstract partial class Es5Visitor<TResult>
    {
        public virtual TResult VisitBlockStatement(Es5BlockStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a variable declaration statement of the form 'var x' or 'var x = y, z'.
        /// </summary>
        public virtual TResult VisitVariableStatement(Es5VariableStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits an empty statement.
        /// </summary>
        public virtual TResult VisitEmptyStatement(Es5EmptyStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits an expression that is represented as a statement.
        /// </summary>
        public virtual TResult VisitExpressionStatement(Es5ExpressionStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'continue' or 'continue Identifier' statement.
        /// </summary>
        public virtual TResult VisitContinueStatement(Es5ContinueStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'break' or 'break Identifier' statement.
        /// </summary>
        public virtual TResult VisitBreakStatement(Es5BreakStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits an 'if (x) statement; else statement;' statement.
        /// </summary>
        public virtual TResult VisitIfStatement(Es5IfStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a return statement of the form 'return expression;'.
        /// </summary>
        public virtual TResult VisitReturnStatement(Es5ReturnStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'with (Expression) Statement' statement.
        /// </summary>
        public virtual TResult VisitWithStatement(Es5WithStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a labelled statement of the form 'Identifier: Statement'.
        /// </summary>
        public virtual TResult VisitLabelledStatement(Es5LabelledStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'switch' statement.
        /// </summary>
        public virtual TResult VisitSwitchStatement(Es5SwitchStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'case' clause.
        /// </summary>
        public virtual TResult VisitCaseClause(Es5CaseClause node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'throw' statement.
        /// </summary>
        public virtual TResult VisitThrowStatement(Es5ThrowStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'try/catch/finally' statement.
        /// </summary>
        public virtual TResult VisitTryStatement(Es5TryStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'debugger' statement.
        /// </summary>
        public virtual TResult VisitDebuggerStatement(Es5DebuggerStatement node) => DefaultVisit(node);
    }
}
