// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using Desalt.Core.Ast;

    /// <summary>
    /// Represents an <see cref="ITsAstNode"/> visitor that visits only the single node passed
    /// into its Visit method.
    /// </summary>
    public abstract partial class TsVisitor : AstVisitor
    {
        public override void Visit(IAstNode node) => (node as ITsAstNode)?.Accept(this);

        /// <summary>
        /// Visits a TypeScript implementation (.ts) source file.
        /// </summary>
        public virtual void VisitImplementationSourceFile(ImplementationSourceFile node) => Visit(node);

        /// <summary>
        /// Visits a TypeScript identifier.
        /// </summary>
        public virtual void VisitIdentifier(ITsIdentifier node) => Visit(node);

        /// <summary>
        /// Visits a TypeScript qualified name, which is a full name with dots separating components.
        /// </summary>
        public virtual void VisitQualifiedName(ITsQualifiedName node) => Visit(node);
    }
}
