// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsVisitor.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    public abstract partial class TsVisitor
    {
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
        /// Visits a function declaration of the form 'function [name] signature { body }'.
        /// </summary>
        public virtual void VisitFunctionDeclaration(ITsFunctionDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a type alias of the form 'type alias&lt;T&gt; = type'.
        /// </summary>
        public virtual void VisitTypeAliasDeclaration(ITsTypeAliasDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a constructor declaration in a class.
        /// </summary>
        public virtual void VisitConstructorDeclaration(ITsConstructorDeclaration node) => Visit(node);
    }
}
