// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsJsDocBlock.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Lexical
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Desalt.TypeScriptAst.Emit;
    using Factory = TsAstFactory;

    /// <summary>
    /// Represents a JSDoc block tag, for example @see, @example, and description.
    /// </summary>
    internal class TsJsDocBlock : TsAstTriviaNode, ITsJsDocBlock
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsJsDocBlock(IEnumerable<ITsJsDocInlineContent> content) : base(preserveSpacing: true)
        {
            Content = content?.ToImmutableArray() ?? ImmutableArray<ITsJsDocInlineContent>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsJsDocInlineContent> Content { get; }

        /// <summary>
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        public override string CodeDisplay =>
            Content.Aggregate(
                new StringBuilder(),
                (builder, content) => builder.Append(content.CodeDisplay),
                builder => builder.ToString());

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public override void Emit(Emitter emitter)
        {
            foreach (ITsJsDocInlineContent content in Content)
            {
                content.Emit(emitter);
            }
        }

        /// <summary>
        /// Emits a node using a string stream. Useful for unit tests and debugging.
        /// </summary>
        /// <param name="emitOptions">The optional emit options.</param>
        /// <returns>The node emitted to a string stream.</returns>
        public override string EmitAsString(EmitOptions emitOptions = null) =>
            Content.Aggregate(
                new StringBuilder(),
                (builder, inlineContent) => builder.Append(inlineContent.EmitAsString(emitOptions)),
                builder => builder.ToString());

        public ITsJsDocBlock WithAppendedContent(string text, bool separateWithBlankLine = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return this;
            }

            if (separateWithBlankLine && !Content.IsEmpty)
            {
                text = "\n\n" + text;
            }

            return new TsJsDocBlock(Content.Add(Factory.JsDocInlineText(text)));
        }

        public ITsJsDocBlock WithAppendedContent(ITsJsDocInlineContent text, bool separateWithBlankLine = false)
        {
            if (text == null)
            {
                return this;
            }

            ImmutableArray<ITsJsDocInlineContent> newContent;
            if (separateWithBlankLine && !Content.IsEmpty)
            {
                newContent = Content.AddRange(new[] { Factory.JsDocInlineText("\n\n"), text });
            }
            else
            {
                newContent = Content.Add(text);
            }

            return new TsJsDocBlock(newContent);
        }

        public ITsJsDocBlock WithAppendedContent(ITsJsDocBlock block, bool separateWithBlankLine = false)
        {
            if (block == null || block.Content.IsEmpty)
            {
                return this;
            }

            var newContentBuilder = Content.ToBuilder();
            if (separateWithBlankLine && !Content.IsEmpty)
            {
                newContentBuilder.Add(Factory.JsDocInlineText("\n\n"));
            }

            newContentBuilder.AddRange(block.Content);

            return new TsJsDocBlock(newContentBuilder);
        }
    }
}
