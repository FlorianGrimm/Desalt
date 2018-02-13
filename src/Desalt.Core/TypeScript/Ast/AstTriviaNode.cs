// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AstTriviaNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast
{
    using System.Diagnostics;
    using Desalt.Core.Emit;

    /// <summary>
    /// Abstract base class for all abstract syntax tree (AST) trivia node types. A trivia node is a
    /// comment or whitespace.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class AstTriviaNode : IAstTriviaNode
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        public abstract string CodeDisplay { get; }

        /// <summary>
        /// Gets a consise string representing the current AST node to show in the debugger
        /// variable window.
        /// </summary>
        protected virtual string DebuggerDisplay => $"{GetType().Name}: {CodeDisplay}";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public abstract void Emit(Emitter emitter);
    }
}
