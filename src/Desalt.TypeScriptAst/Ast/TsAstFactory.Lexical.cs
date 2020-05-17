// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Lexical.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.TypeScriptAst.Ast.Lexical;

    public static partial class TsAstFactory
    {
        /// <summary>
        /// Represents a newline whitespace trivia node.
        /// </summary>
        public static readonly ITsWhitespaceTrivia Newline = new TsWhitespaceTrivia(
            "\n",
            isNewline: true,
            preserveSpacing: true);

        /// <summary>
        /// Creates whitespace that can appear before or after another <see cref="ITsAstNode"/>.
        /// </summary>
        public static ITsWhitespaceTrivia Whitespace(string text)
        {
            return new TsWhitespaceTrivia(text, isNewline: false, preserveSpacing: true);
        }

        /// <summary>
        /// Creates a TypeScript single-line comment of the form '// comment'.
        /// </summary>
        public static ITsSingleLineComment SingleLineComment(
            string text,
            bool preserveSpacing = false,
            bool omitNewLineAtEnd = false)
        {
            return new TsSingleLineComment(text, preserveSpacing, omitNewLineAtEnd);
        }

        /// <summary>
        /// Creates a TypeScript multi-line comment of the form '/* lines */'.
        /// </summary>
        public static ITsMultiLineComment MultiLineComment(params string[] lines)
        {
            return new TsMultiLineComment(isJsDoc: false, lines.ToImmutableArray(), preserveSpacing: false);
        }

        /// <summary>
        /// Creates a TypeScript multi-line comment of the form '/* lines */'.
        /// </summary>
        public static ITsMultiLineComment MultiLineComment(
            bool isJsDoc,
            bool preserveSpacing = false,
            params string[] lines)
        {
            return new TsMultiLineComment(isJsDoc, lines.ToImmutableArray(), preserveSpacing);
        }

        /// <summary>
        /// Creates a builder for <see cref="ITsJsDocComment"/> objects.
        /// </summary>
        public static ITsJsDocCommentBuilder JsDocCommentBuilder()
        {
            return new TsJsDocCommentBuilder();
        }

        /// <summary>
        /// Creates a structured JSDoc comment before a declaration.
        /// </summary>
        public static ITsJsDocComment JsDocComment(string description)
        {
            return new TsJsDocComment(description: new TsJsDocBlock(new[] { new TsJsDocInlineText(description) }));
        }

        /// <summary>
        /// Creates a structured JSDoc comment before a declaration.
        /// </summary>
        public static ITsJsDocComment JsDocComment(
            ITsJsDocBlock? fileTag = null,
            ITsJsDocBlock? copyrightTag = null,
            bool isPackagePrivate = false,
            IEnumerable<(string paramName, ITsJsDocBlock text)>? paramsTags = null,
            ITsJsDocBlock? returnsTag = null,
            IEnumerable<(string typeName, ITsJsDocBlock text)>? throwsTags = null,
            IEnumerable<ITsJsDocBlock>? exampleTags = null,
            ITsJsDocBlock? description = null,
            ITsJsDocBlock? summaryTag = null,
            IEnumerable<ITsJsDocBlock>? seeTags = null)
        {
            return new TsJsDocComment(
                fileTag: fileTag,
                copyrightTag: copyrightTag,
                isPackagePrivate: isPackagePrivate,
                paramsTags: paramsTags,
                returnsTag: returnsTag,
                throwsTags: throwsTags,
                exampleTags: exampleTags,
                description: description,
                summaryTag: summaryTag,
                seeTags: seeTags);
        }

        /// <summary>
        /// Creates a JSDoc block tag, for example @see, @example, and description.
        /// </summary>
        public static ITsJsDocBlock JsDocBlock(params ITsJsDocInlineContent[] content)
        {
            return new TsJsDocBlock(content);
        }

        /// <summary>
        /// Creates a JSDoc block tag, for example @see, @example, and description.
        /// </summary>
        public static ITsJsDocBlock JsDocBlock(params string[] content)
        {
            return new TsJsDocBlock(content.Select(x => new TsJsDocInlineText(x)));
        }

        /// <summary>
        /// Creates plain text within a JSDoc block tag.
        /// </summary>
        public static ITsJsDocInlineText JsDocInlineText(string text)
        {
            return new TsJsDocInlineText(text);
        }

        /// <summary>
        /// Creates a JSDoc inline @link tag of the format '{@link NamespaceOrUrl}' or '[Text]{@link NamespaceOrUrl}'.
        /// </summary>
        public static ITsJsDocLinkTag JsDocLinkTag(string namespaceOrUrl, string? text = null)
        {
            return new TsJsDocLinkTag(namespaceOrUrl, text);
        }
    }
}
