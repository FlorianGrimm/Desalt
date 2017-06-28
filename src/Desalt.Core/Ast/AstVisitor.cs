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
        public abstract void Visit(TNode node);

        public virtual void DefaultVisit(TNode node)
        {
            throw new InvalidOperationException($"{GetType().Name}: Node not supported: {node.GetType().Name}");
        }
    }
}
