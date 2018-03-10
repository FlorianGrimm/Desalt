// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AstTriviaNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast
{
    using System.Diagnostics;
    using System.IO;
    using Desalt.Core.Emit;

    /// <summary>
    /// Abstract base class for all abstract syntax tree (AST) trivia node types. A trivia node is a
    /// comment or whitespace.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class AstTriviaNode : IAstTriviaNode
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        protected AstTriviaNode(bool preserveSpacing)
        {
            PreserveSpacing = preserveSpacing;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Indicates whether to preserve the leading and trailing spacing and not add spaces around
        /// the beginning and ending markers.
        /// </summary>
        public bool PreserveSpacing { get; }

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

        /// <summary>
        /// Returns either a space or an empty string depending on the value of <see cref="PreserveSpacing"/>.
        /// </summary>
        protected string Space => PreserveSpacing ? string.Empty : " ";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public abstract void Emit(Emitter emitter);

        /// <summary>
        /// Emits a node using a string stream. Useful for unit tests and debugging.
        /// </summary>
        /// <param name="emitOptions">The optional emit options.</param>
        /// <returns>The node emitted to a string stream.</returns>
        public virtual string EmitAsString(EmitOptions emitOptions = null)
        {
            using (var stream = new MemoryStream())
            using (var emitter = new Emitter(stream, options: emitOptions))
            {
                Emit(emitter);
                stream.Flush();
                stream.Position = 0;

                using (var reader = new StreamReader(stream, emitter.Encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
