// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast
{
    /// <summary>
    /// Represents an <see cref="ITsAstNode"/> visitor that visits only the single node passed
    /// into its Visit method.
    /// </summary>
    public abstract partial class TsVisitor
    {
        public virtual void Visit(ITsAstNode node) => node?.Accept(this);

        /// <summary>
        /// Visits a TypeScript identifier.
        /// </summary>
        public virtual void VisitIdentifier(ITsIdentifier node) => Visit(node);

        /// <summary>
        /// Visits a TypeScript qualified name, which is a full name with dots separating components.
        /// </summary>
        public virtual void VisitQualifiedName(ITsQualifiedName node) => Visit(node);

        /// <summary>
        /// Visits a qualified name with type arguments. For example, 'ns.type.method&lt;T1, T2&gt;'.
        /// </summary>
        public virtual void VisitGenericTypeName(ITsGenericTypeName node) => Visit(node);
    }
}
