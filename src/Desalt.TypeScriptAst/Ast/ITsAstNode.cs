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
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        string CodeDisplay { get; }

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
    }
}
