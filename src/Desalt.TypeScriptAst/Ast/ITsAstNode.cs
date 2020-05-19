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
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Root interface for all abstract syntax tree (AST) node types.
    /// </summary>
    public interface ITsAstNode : IEquatable<ITsAstNode>
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets an array of trivia that appear before this node in the source code.
        /// </summary>
        ImmutableArray<ITsAstTriviaNode> LeadingTrivia { get; }

        /// <summary>
        /// Gets an array of trivia that appear after this node in the source code.
        /// </summary>
        ImmutableArray<ITsAstTriviaNode> TrailingTrivia { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Accepts the visitor by calling into a specific method on the visitor for this type of AST node.
        /// </summary>
        /// <param name="visitor">The visitor to visit.</param>
        void Accept(TsVisitor visitor);

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        void Emit(Emitter emitter);

        /// <summary>
        /// Emits a node using a string stream. Useful for unit tests and debugging.
        /// </summary>
        /// <param name="emitOptions">The optional emit options.</param>
        /// <returns>The node emitted to a string stream.</returns>
        string EmitAsString(EmitOptions? emitOptions = null);

        ///// <summary>
        ///// Creates a copy of this node with the specified leading trivia.
        ///// </summary>
        //ITsAstNode WithLeadingTrivia(ImmutableArray<ITsAstTriviaNode> value);

        ///// <summary>
        ///// Creates a copy of this node with the specified trailing trivia.
        ///// </summary>
        //ITsAstNode WithTrailingTrivia(ImmutableArray<ITsAstTriviaNode> value);

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
