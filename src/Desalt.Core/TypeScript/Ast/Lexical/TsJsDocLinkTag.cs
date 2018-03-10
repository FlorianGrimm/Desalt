// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsJsDocLinkTag.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Lexical
{
    using System;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a JSDoc inline @link tag of the format '{@link NamespaceOrUrl}' or '[Text]{@link NamespaceOrUrl}'.
    /// </summary>
    internal class TsJsDocLinkTag : AstTriviaNode, ITsJsDocLinkTag
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsJsDocLinkTag(string namepathOrUrl, string text = null) : base(preserveSpacing: true)
        {
            NamepathOrUrl = namepathOrUrl?.Replace("{", string.Empty).Replace("}", string.Empty) ??
                throw new ArgumentNullException(nameof(namepathOrUrl));
            Text = text?.Replace("[", string.Empty).Replace("]", string.Empty);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string NamepathOrUrl { get; }
        public string Text { get; }

        /// <summary>
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        public override string CodeDisplay =>
            string.IsNullOrEmpty(Text) ? $"{{@link {NamepathOrUrl}}}" : $"[{Text}]{{@link {NamepathOrUrl}}}";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public override void Emit(Emitter emitter)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                emitter.Write($"[{Text}]");
            }

            emitter.Write($"{{@link {NamepathOrUrl}}}");
        }
    }
}
