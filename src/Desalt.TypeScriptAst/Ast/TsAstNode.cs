// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Abstract base class for all abstract syntax tree (AST) nodes.
    /// </summary>
    internal abstract class TsAstNode : TsNode, ITsAstNode, IEquatable<ITsAstNode>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        protected TsAstNode(
            ImmutableArray<ITsAstTriviaNode>? leadingTrivia = null,
            ImmutableArray<ITsAstTriviaNode>? trailingTrivia = null)
            : base(leadingTrivia, trailingTrivia)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Accepts the visitor by calling into a specific method on the visitor for this type of AST node.
        /// </summary>
        /// <param name="visitor">The visitor to visit.</param>
        public abstract void Accept(TsVisitor visitor);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        bool IEquatable<ITsAstNode>.Equals(ITsAstNode other)
        {
            return Equals(other as TsAstNode);
        }
    }
}
