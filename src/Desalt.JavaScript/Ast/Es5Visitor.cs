// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Visitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast
{
    using Desalt.Core.Ast;

    /// <summary>
    /// Represents an <see cref="Es5AstNode"/> visitor that visits only the single node passed
    /// into its Visit method.
    /// </summary>
    public abstract partial class Es5Visitor : AstVisitor<IEs5AstNode>
    {
        public override void Visit(IEs5AstNode node)
        {
            node?.Accept(this);
        }

        /// <summary>
        /// Visits a function declaration of the form 'function name?(parameters) { body }'.
        /// </summary>
        public virtual void VisitFunctionDeclaration(Es5FunctionDeclaration node) => DefaultVisit(node);

        /// <summary>
        /// Visits a JavaScript program.
        /// </summary>
        public virtual void VisitProgram(Es5Program node) => DefaultVisit(node);

        /// <summary>
        /// Visits a JavaScript identifier.
        /// </summary>
        public virtual void VisitIdentifier(Es5Identifier node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents an <see cref="Es5AstNode"/> visitor that visits only the single node passed
    /// into its Visit method and produces a value of the type specified by the <typeparamref
    /// name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value this visitor's Visit method.</typeparam>
    public abstract partial class Es5Visitor<TResult> : AstVisitor<IEs5AstNode, TResult>
    {
        public override TResult Visit(IEs5AstNode node)
        {
            return node != null ? node.Accept(this) : default(TResult);
        }

        /// <summary>
        /// Visits a function declaration of the form 'function name?(parameters) { body }'.
        /// </summary>
        public virtual TResult VisitFunctionDeclaration(Es5FunctionDeclaration node) => DefaultVisit(node);

        /// <summary>
        /// Visits a JavaScript program.
        /// </summary>
        public virtual TResult VisitProgram(Es5Program node) => DefaultVisit(node);

        /// <summary>
        /// Visits a JavaScript identifier.
        /// </summary>
        public virtual TResult VisitIdentifier(Es5Identifier node) => DefaultVisit(node);
    }
}
