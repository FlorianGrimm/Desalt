// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITsAstNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Root interface for all abstract syntax tree (AST) node types.
    /// </summary>
    public interface ITsAstNode : ITsNode, IEquatable<ITsAstNode>
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Accepts the visitor by calling into a specific method on the visitor for this type of AST node.
        /// </summary>
        /// <param name="visitor">The visitor to visit.</param>
        void Accept(TsVisitor visitor);

        /// <summary>
        /// Creates a shallow copy of this node with the leading and trailing trivia replaced with the specified values.
        /// </summary>
        /// <param name="leadingTrivia">The new leading trivia for the node.</param>
        /// <param name="trailingTrivia">The new trailing trivia for the node.</param>
        /// <returns>A copy of this node with the trivia replaced.</returns>
        ITsAstNode ShallowCopy(
            ImmutableArray<ITsAstTriviaNode> leadingTrivia,
            ImmutableArray<ITsAstTriviaNode> trailingTrivia);
    }
}
