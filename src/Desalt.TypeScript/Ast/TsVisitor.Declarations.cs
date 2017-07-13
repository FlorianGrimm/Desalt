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

        /// <summary>
        /// Visits a member variable declaration in a class.
        /// </summary>
        public virtual void VisitVariableMemberDeclaration(ITsVariableMemberDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a member function declaration in a class.
        /// </summary>
        public virtual void VisitFunctionMemberDeclaration(ITsFunctionMemberDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a 'get' member accessor declaration in a class.
        /// </summary>
        public virtual void VisitGetAccessorMemberDeclaration(ITsGetAccessorMemberDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a 'set' member accessor declaration in a class.
        /// </summary>
        public virtual void VisitSetAccessorMemberDeclaration(ITsSetAccessorMemberDeclaration node) => Visit(node);

        /// <summary>
        /// Visits an index member declaration in a class.
        /// </summary>
        public virtual void VisitIndexMemberDeclaration(ITsIndexMemberDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a class heritage of the form ' extends type implements type, type'.
        /// </summary>
        public virtual void VisitClassHeritage(ITsClassHeritage node) => Visit(node);

        /// <summary>
        /// Visits a class declaration.
        /// </summary>
        public virtual void VisitClassDeclaration(ITsClassDeclaration node) => Visit(node);

        /// <summary>
        /// Visits an interface declaration
        /// </summary>
        public virtual void VisitInterfaceDeclaration(ITsInterfaceDeclaration node) => Visit(node);

        /// <summary>
        /// Visits an enum member of the form, 'name = value'.
        /// </summary>
        public virtual void VisitEnumMember(ITsEnumMember node) => Visit(node);

        /// <summary>
        /// Visits an enum declaration.
        /// </summary>
        public virtual void VisitEnumDeclaration(ITsEnumDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a namespace declaration.
        /// </summary>
        public virtual void VisitNamespaceDeclaration(ITsNamespaceDeclaration node) => Visit(node);

        /// <summary>
        /// Visits an exported declaration.
        /// </summary>
        public virtual void VisitExportedDeclaration(ITsExportedDeclaration node) => Visit(node);

        /// <summary>
        /// Visits an exported variable declaration statement
        /// </summary>
        public virtual void VisitExportedVariableStatement(ITsExportedVariableStatement node) => Visit(node);

        /// <summary>
        /// Visits an import alias declaration of the form, 'import alias = dotted.name'.
        /// </summary>
        public virtual void VisitImportAliasDeclaration(ITsImportAliasDeclaration node) => Visit(node);

        /// <summary>
        /// Visits an ambient variable binding of the form 'name: type'.
        /// </summary>
        public virtual void VisitAmbientBinding(ITsAmbientBinding node) => Visit(node);

        /// <summary>
        /// Visits an ambient variable declaration of the form, 'var|let|const x, y: type;'.
        /// </summary>
        public virtual void VisitAmbientVariableDeclaration(ITsAmbientVariableDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a function declaration of the form 'function name signature'.
        /// </summary>
        public virtual void VisitAmbientFunctionDeclaration(ITsAmbientFunctionDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a constructor declaration in an ambient class declaration.
        /// </summary>
        public virtual void VisitAmbientConstructorDeclaration(ITsAmbientConstructorDeclaration node) => Visit(node);

        /// <summary>
        /// Visits a member variable declaration in an ambient class declaration.
        /// </summary>
        public virtual void VisitAmbientVariableMemberDeclaration(ITsAmbientVariableMemberDeclaration node) => Visit(node);
    }
}
