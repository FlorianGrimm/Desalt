// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITsNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Root interface for all abstract syntax tree (AST) nodes that can contain whitespace and be emitted
    /// (<see cref="ITsAstNode"/> and <see cref="TsTokenNode"/>. Implementations should be immutable.
    /// </summary>
    public interface ITsNode
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
