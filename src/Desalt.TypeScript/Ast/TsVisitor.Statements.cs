// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsVisitor.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    public abstract partial class TsVisitor
    {
        /// <summary>
        /// Visits a debugger statement.
        /// </summary>
        public virtual void VisitDebuggerStatement(ITsDebuggerStatement node) => Visit(node);

        /// <summary>
        /// Visits a block of statements.
        /// </summary>
        public virtual void VisitBlockStatement(ITsBlockStatement node) => Visit(node);

        /// <summary>
        /// Visits an empty statement.
        /// </summary>
        public virtual void VisitEmptyStatement(ITsEmptyStatement node) => Visit(node);

        /// <summary>
        /// Visits a variable declaration statement of the form 'var x: type = y;'.
        /// </summary>
        public virtual void VisitVariableStatement(ITsVariableStatement node) => Visit(node);

        /// <summary>
        /// Visits a simple variable declaration of the form 'x: type = y'.
        /// </summary>
        public virtual void VisitSimpleVariableDeclaration(ITsSimpleVariableDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a destructuring variable declaration of the form '{x, y} = foo' or '[x, y] = foo'.
        /// </summary>
        public virtual void VisitDestructuringVariableDeclaration(ITsDestructuringVariableDeclaration node) => Visit(node);

        /// <summary>
        /// Visits an object binding pattern of the form '{propName = defaultValue, propName: otherPropName}'.
        /// </summary>
        public virtual void VisitObjectBindingPattern(ITsObjectBindingPattern node) => Visit(node);

        /// <summary>
        /// Visits an array binding pattern of the form '[x = y, z, ...p]'.
        /// </summary>
        public virtual void VisitArrayBindingPattern(ITsArrayBindingPattern node) => Visit(node);

        /// <summary>
        /// Visits a single name binding within an object or array pattern binding, of the form 'name
        /// = defaultValue'.
        /// </summary>
        public virtual void VisitSingleNameBinding(ITsSingleNameBinding node) => Visit(node);

        /// <summary>
        /// Visits a recursive pattern binding in an object or array binding.
        /// </summary>
        public virtual void VisitPatternBinding(ITsPatternBinding node) => Visit(node);

        /// <summary>
        /// Visits an expression in statement form.
        /// </summary>
        public virtual void VisitExpressionStatement(ITsExpressionStatement node) => Visit(node);

        /// <summary>
        /// Visits an 'if' statement of the form 'if (expression) statement else statement'.
        /// </summary>
        public virtual void VisitIfStatement(ITsIfStatement node) => Visit(node);

        /// <summary>
        /// Visits a try/catch/finally statement.
        /// </summary>
        public virtual void VisitTryStatement(ITsTryStatement node) => Visit(node);

        /// <summary>
        /// Visits a do/while statement.
        /// </summary>
        public virtual void VisitDoWhileStatement(ITsDoWhileStatement node) => Visit(node);

        /// <summary>
        /// Visits a while loop statement.
        /// </summary>
        public virtual void VisitWhileStatement(ITsWhileStatement node) => Visit(node);

        /// <summary>
        /// Visits a simple variable declaration of the form 'x: type = y'.
        /// </summary>
        public virtual void VisitSimpleLexicalBinding(ITsSimpleLexicalBinding node) => Visit(node);

        /// <summary>
        /// Visits a destructuring lexical binding of the form '{x, y}: type = foo' or '[x, y]: type = foo'.
        /// </summary>
        public virtual void VisitDestructuringLexicalBinding(ITsDestructuringLexicalBinding node) => Visit(node);

        /// <summary>
        /// Visits a lexical declaration of the form 'const|let x: type, y: type = z;'.
        /// </summary>
        public virtual void VisitLexicalDeclaration(ITsLexicalDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a 'for' loop statement.
        /// </summary>
        public virtual void VisitForStatement(ITsForStatement node) => Visit(node);

        /// <summary>
        /// Visits a for-in loop of the form 'for (const x in expression) statement'.
        /// </summary>
        public virtual void VisitForInStatement(ITsForInStatement node) => Visit(node);

        /// <summary>
        /// Visits a for-of loop of the form 'for (const x of expression) statement'.
        /// </summary>
        public virtual void VisitForOfStatement(ITsForOfStatement node) => Visit(node);

        /// <summary>
        /// Visits a case clause in a switch statement.
        /// </summary>
        public virtual void VisitCaseClause(ITsCaseClause node) => Visit(node);

        /// <summary>
        /// Visits a default clause in a switch statement of the form 'default: statements'.
        /// </summary>
        public virtual void VisitDefaultClause(ITsDefaultClause node) => Visit(node);

        /// <summary>
        /// Visits a switch statement of the form 'switch (condition) { case x: statement; default: statement; }'.
        /// </summary>
        public virtual void VisitSwitchStatement(ITsSwitchStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'continue' statement.
        /// </summary>
        public virtual void VisitContinueStatement(ITsContinueStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'break' statement.
        /// </summary>
        public virtual void VisitBreakStatement(ITsBreakStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'return' statement
        /// </summary>
        public virtual void VisitReturnStatement(ITsReturnStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'with' statement of the form, 'with (expression) statement'.
        /// </summary>
        public virtual void VisitWithStatement(ITsWithStatement node) => Visit(node);

        /// <summary>
        /// Visits a labelled statement.
        /// </summary>
        public virtual void VisitLabelledStatement(ITsLabelledStatement node) => Visit(node);

        /// <summary>
        /// Visits a 'throw' statement.
        /// </summary>
        public virtual void VisitThrowStatement(ITsThrowStatement node) => Visit(node);
    }
}
