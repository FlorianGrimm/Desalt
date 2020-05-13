// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsJsDocLinkTag.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Lexical
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a JSDoc inline @link tag of the format '{@link NamespaceOrUrl}' or '[Text]{@link NamespaceOrUrl}'.
    /// </summary>
    internal class TsJsDocLinkTag : TsAstTriviaNode, ITsJsDocLinkTag
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsJsDocLinkTag(string namespaceOrUrl, string? text = null)
            : base(preserveSpacing: true)
        {
            NamespaceOrUrl = namespaceOrUrl?.Replace("{", string.Empty).Replace("}", string.Empty) ??
                throw new ArgumentNullException(nameof(namespaceOrUrl));
            Text = text?.Replace("[", string.Empty).Replace("]", string.Empty);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value indicating whether this content node is empty.
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(NamespaceOrUrl) && string.IsNullOrEmpty(Text);

        public string NamespaceOrUrl { get; }
        public string? Text { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public override void Emit(Emitter emitter)
        {
            emitter.Write(
                string.IsNullOrEmpty(Text) ? $"{{@link {NamespaceOrUrl}}}" : $"[{Text}]{{@link {NamespaceOrUrl}}}");
        }
    }
}
