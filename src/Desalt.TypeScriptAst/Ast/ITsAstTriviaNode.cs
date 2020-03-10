// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITsAstTriviaNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Root interface for all abstract syntax tree (AST) trivia node types. A trivia node is a
    /// comment or whitespace.
    /// </summary>
    public interface ITsAstTriviaNode : IEquatable<ITsAstTriviaNode>
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Indicates whether to preserve the leading and trailing spacing and not add spaces around
        /// the beginning and ending markers.
        /// </summary>
        bool PreserveSpacing { get; }

        /// <summary>
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        string CodeDisplay { get; }

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
        string EmitAsString(EmitOptions emitOptions = null);
    }
}
