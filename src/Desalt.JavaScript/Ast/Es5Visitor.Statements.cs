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
        public virtual void VisitBlockStatement(Es5BlockStatement node) => Visit(node);

        /// <summary>
        /// Visits a variable declaration statement of the form 'var x' or 'var x = y, z'.
        /// </summary>
        public virtual void VisitVariableStatement(Es5VariableStatement node) => Visit(node);

        /// <summary>
        /// Visits an empty statement.
        /// </summary>
        public virtual void VisitEmptyStatement(Es5EmptyStatement node) => Visit(node);

        /// <summary>
        /// Visits an expression that is represented as a statement.
        /// </summary>
        public virtual void VisitExpressionStatement(Es5ExpressionStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'continue' or 'continue Identifier' statement.
        /// </summary>
        public virtual void VisitContinueStatement(Es5ContinueStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'break' or 'break Identifier' statement.
        /// </summary>
        public virtual void VisitBreakStatement(Es5BreakStatement node) => Visit(node);

        /// <summary>
        /// Visits an 'if (x) statement; else statement;' statement.
        /// </summary>
        public virtual void VisitIfStatement(Es5IfStatement node) => Visit(node);

        /// <summary>
        /// Visits a return statement of the form 'return expression;'.
        /// </summary>
        public virtual void VisitReturnStatement(Es5ReturnStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'with (Expression) Statement' statement.
        /// </summary>
        public virtual void VisitWithStatement(Es5WithStatement node) => Visit(node);

        /// <summary>
        /// Visits a labelled statement of the form 'Identifier: Statement'.
        /// </summary>
        public virtual void VisitLabelledStatement(Es5LabelledStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'switch' statement.
        /// </summary>
        public virtual void VisitSwitchStatement(Es5SwitchStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'case' clause.
        /// </summary>
        public virtual void VisitCaseClause(Es5CaseClause node) => Visit(node);

        /// <summary>
        /// Visits a 'throw' statement.
        /// </summary>
        public virtual void VisitThrowStatement(Es5ThrowStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'try/catch/finally' statement.
        /// </summary>
        public virtual void VisitTryStatement(Es5TryStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'debugger' statement.
        /// </summary>
        public virtual void VisitDebuggerStatement(Es5DebuggerStatement node) => Visit(node);
    }
}
