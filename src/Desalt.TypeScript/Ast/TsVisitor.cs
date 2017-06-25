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
    public abstract partial class TsVisitor : AstVisitor<ITsAstNode>
    {
        public override void Visit(ITsAstNode node) => node?.Accept(this);

        /// <summary>
        /// Visits a TypeScript implementation (.ts) source file.
        /// </summary>
        public virtual void VisitImplementationSourceFile(ImplementationSourceFile node) => DefaultVisit(node);

        /// <summary>
        /// Visits a TypeScript identifier.
        /// </summary>
        public virtual void VisitIdentifier(ITsIdentifier node) => DefaultVisit(node);

        /// <summary>
        /// Visits a TypeScript qualified name, which is a full name with dots separating components.
        /// </summary>
        public virtual void VisitQualifiedName(ITsQualifiedName node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents an <see cref="ITsAstNode"/> visitor that visits only the single node passed
    /// into its Visit method and produces a value of the type specified by the <typeparamref
    /// name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value this visitor's Visit method.</typeparam>
    public abstract partial class TsVisitor<TResult> : AstVisitor<ITsAstNode, TResult>
    {
        public override TResult Visit(ITsAstNode node)
        {
            return node != null ? node.Accept(this) : default(TResult);
        }

        /// <summary>
        /// Visits a TypeScript implementation (.ts) source file.
        /// </summary>
        public virtual TResult VisitImplementationSourceFile(ImplementationSourceFile node) => DefaultVisit(node);

        /// <summary>
        /// Visits a TypeScript identifier.
        /// </summary>
        public virtual TResult VisitIdentifier(ITsIdentifier node) => DefaultVisit(node);

        /// <summary>
        /// Visits a TypeScript qualified name, which is a full name with dots separating components.
        /// </summary>
        public virtual TResult VisitQualifiedName(ITsQualifiedName node) => DefaultVisit(node);
    }
}
