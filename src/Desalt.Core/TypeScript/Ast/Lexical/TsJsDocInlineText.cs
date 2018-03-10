// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsJsDocInlineText.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Lexical
{
    using System;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents plain text within a JSDoc block tag.
    /// </summary>
    internal class TsJsDocInlineText : AstTriviaNode, ITsJsDocInlineText
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsJsDocInlineText(string text) : base(preserveSpacing: true)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string Text { get; }

        /// <summary>
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        public override string CodeDisplay => Text;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public override void Emit(Emitter emitter) => emitter.Write(Text);

        /// <summary>
        /// Emits a node using a string stream. Useful for unit tests and debugging.
        /// </summary>
        /// <param name="emitOptions">The optional emit options.</param>
        /// <returns>The node emitted to a string stream.</returns>
        public override string EmitAsString(EmitOptions emitOptions = null) => Text;
    }
}
