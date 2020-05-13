// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsJsDocInlineText.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Lexical
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents plain text within a JSDoc block tag.
    /// </summary>
    internal class TsJsDocInlineText : TsAstTriviaNode, ITsJsDocInlineText
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

        /// <summary>
        /// Returns a value indicating whether this content node is empty.
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Text);

        public string Text { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public override void Emit(Emitter emitter)
        {
            emitter.Write(Text);
        }

        /// <summary>
        /// Emits a node using a string stream. Useful for unit tests and debugging.
        /// </summary>
        /// <param name="emitOptions">The optional emit options.</param>
        /// <returns>The node emitted to a string stream.</returns>
        public override string EmitAsString(EmitOptions? emitOptions = null)
        {
            return Text;
        }
    }
}
