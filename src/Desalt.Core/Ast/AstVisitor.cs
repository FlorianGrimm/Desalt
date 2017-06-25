// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AstVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Ast
{
    using System;

    /// <summary>
    /// Abstract base class for an <see cref="IAstVisitor{TNode}"/> visitor that visits only the
    /// single node passed into its Visit method.
    /// </summary>
    /// <typeparam name="TNode">The type of the node to visit.</typeparam>
    public abstract class AstVisitor<TNode> : IAstVisitor<TNode>
        where TNode : IAstNode
    {
        public abstract void Visit(TNode model);

        public virtual void DefaultVisit(TNode node)
        {
            throw new InvalidOperationException($"{GetType().Name}: Node not supported: {node.GetType().Name}");
        }
    }

    /// <summary>
    /// Abstract base class for an <see cref="IAstVisitor{TNode}"/> visitor that visits only the
    /// single node passed into its Visit method and produces a value of the type specified by the
    /// <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TNode">The type of the node to visit.</typeparam>
    /// <typeparam name="TResult">The type of the return value this visitor's Visit method.</typeparam>
    public abstract class AstVisitor<TNode, TResult> : IAstVisitor<TNode, TResult>
        where TNode : IAstNode
    {
        public abstract TResult Visit(TNode model);

        public virtual TResult DefaultVisit(TNode node)
        {
            throw new InvalidOperationException($"{GetType().Name}: Node not supported: {node.GetType().Name}");
        }
    }
}
