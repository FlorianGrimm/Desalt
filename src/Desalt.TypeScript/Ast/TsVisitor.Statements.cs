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
        /// Visits a block of statements.
        /// </summary>
        public virtual void VisitBlockStatement(ITsBlockStatement node) => Visit(node);

        /// <summary>
        /// Visits an empty statement.
        /// </summary>
        public virtual void VisitEmptyStatement(ITsEmptyStatement node) => Visit(node);

        /// <summary>
        /// Visits a simple variable declaration of the form 'x = y'.
        /// </summary>
        public virtual void VisitSimpleVariableDeclaration(ITsSimpleVariableDeclaration node) => Visit(node);

        /// <summary>
        /// Visits an object binding pattern of the form '{propName = defaultValue, propName: otherPropName}'.
        /// </summary>
        public virtual void VisitObjectBindingPattern(ITsObjectBindingPattern node) => Visit(node);

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
        /// Visits a debugger statement.
        /// </summary>
        public virtual void VisitDebuggerStatement(ITsDebuggerStatement node) => Visit(node);
    }
}
