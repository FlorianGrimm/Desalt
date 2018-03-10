﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsJsDocBlock.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Lexical
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a JSDoc block tag, for example @see, @example, and description.
    /// </summary>
    internal class TsJsDocBlock : AstTriviaNode, ITsJsDocBlock
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsJsDocBlock(IEnumerable<ITsJsDocInlineText> content) : base(preserveSpacing: true)
        {
            Content = content?.ToImmutableArray() ?? ImmutableArray<ITsJsDocInlineText>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsJsDocInlineText> Content { get; }

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
            foreach (ITsJsDocInlineText content in Content)
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
    }
}
