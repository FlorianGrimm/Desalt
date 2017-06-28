// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IAstVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Ast
{
    /// <summary>
    /// Service contract for a class that visits nodes in an abstract syntax tree (AST).
    /// </summary>
    /// <typeparam name="TNode">The type of nodes in the tree.</typeparam>
    public interface IAstVisitor<in TNode> where TNode : IAstNode
    {
        void DefaultVisit(TNode node);

        void Visit(TNode node);
    }
}
