﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    using Desalt.Core.Ast;

    /// <summary>
    /// Represents an <see cref="ITsCodeModel"/> visitor that visits only the single model passed
    /// into its Visit method.
    /// </summary>
    public abstract partial class TypeScriptVisitor : AstVisitor<ITsCodeModel>
    {
        public override void Visit(ITsCodeModel model) => model?.Accept(this);

        /// <summary>
        /// Visits a TypeScript implementation (.ts) source file.
        /// </summary>
        public virtual void VisitImplementationSourceFile(ImplementationSourceFile model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript identifier.
        /// </summary>
        public virtual void VisitIdentifier(ITsIdentifier model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript qualified name, which is a full name with dots separating components.
        /// </summary>
        public virtual void VisitQualifiedName(ITsQualifiedName model) => DefaultVisit(model);
    }

    /// <summary>
    /// Represents an <see cref="ITsCodeModel"/> visitor that visits only the single model passed
    /// into its Visit method and produces a value of the type specified by the <typeparamref
    /// name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value this visitor's Visit method.</typeparam>
    public abstract partial class TypeScriptVisitor<TResult> : AstVisitor<ITsCodeModel, TResult>
    {
        public override TResult Visit(ITsCodeModel model)
        {
            return model != null ? model.Accept(this) : default(TResult);
        }

        /// <summary>
        /// Visits a TypeScript implementation (.ts) source file.
        /// </summary>
        public virtual TResult VisitImplementationSourceFile(ImplementationSourceFile model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript identifier.
        /// </summary>
        public virtual TResult VisitIdentifier(ITsIdentifier model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript qualified name, which is a full name with dots separating components.
        /// </summary>
        public virtual TResult VisitQualifiedName(ITsQualifiedName model) => DefaultVisit(model);
    }
}
