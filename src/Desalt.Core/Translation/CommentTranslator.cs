// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CommentTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Translates single- and multi-line comments attached to a node.
    /// </summary>
    internal static class CommentTranslator
    {
        /// <summary>
        /// Adds all comments to the translated node, including single and multiple line comments and structured
        /// documentation comments.
        /// </summary>
        /// <typeparam name="T">The type of translated node.</typeparam>
        /// <param name="translatedNode">The node that has already been translated.</param>
        /// <param name="context">The <see cref="TranslationContext"/> to use for looking up symbols and logging errors.</param>
        /// <param name="node">The original syntax node that was the source of the translation.</param>
        /// <returns>The same translated node with leading and trailing comments.</returns>
        public static T AddAllCommentsFrom<T>(this T translatedNode, TranslationContext context, SyntaxNode node)
            where T : ITsAstNode
        {
            ISymbol symbol = context.GetExpectedDeclaredSymbol<ISymbol>(node);
            var leadingTranslatedTrivia = TranslateTriviaList(node.GetLeadingTrivia(), context, symbol);
            var trailingTranslatedTrivia = TranslateTriviaList(node.GetTrailingTrivia(), context, symbol);

            translatedNode = AddTranslatedTrivia(translatedNode, leadingTranslatedTrivia, addAsLeadingTrivia: true);
            translatedNode = AddTranslatedTrivia(translatedNode, trailingTranslatedTrivia, addAsLeadingTrivia: false);

            return translatedNode;
        }

        /// <summary>
        /// Adds only "simple" comments, including single and multiple line comments. Structured documentation comments
        /// will be included, but will not be translated to JsDoc comments. Use <see cref="AddAllCommentsFrom{T}"/> for this.
        /// </summary>
        /// <typeparam name="T">The type of translated node.</typeparam>
        /// <param name="translatedNode">The node that has already been translated.</param>
        /// <param name="nodeOrToken">The original syntax node or token that was the source of the translation.</param>
        /// <returns>The same translated node with leading and trailing comments.</returns>
        public static T AddCommentsFrom<T>(this T translatedNode, SyntaxNodeOrToken nodeOrToken)
            where T : ITsAstNode
        {
            translatedNode = translatedNode.AddCommentsFrom(nodeOrToken.GetLeadingTrivia(), addAsLeadingTrivia: true);
            translatedNode = translatedNode.AddCommentsFrom(nodeOrToken.GetTrailingTrivia(), addAsLeadingTrivia: false);

            return translatedNode;
        }

        /// <summary>
        /// Adds only "simple" comments, including single and multiple line comments. Structured documentation comments
        /// will be included, but will not be translated to JsDoc comments. Use <see cref="AddAllCommentsFrom{T}"/> for this.
        /// </summary>
        /// <typeparam name="T">The type of translated node.</typeparam>
        /// <param name="translatedNode">The node that has already been translated.</param>
        /// <param name="triviaList">The node's leading or trailing trivia list.</param>
        /// <param name="addAsLeadingTrivia">
        /// Indicates if <paramref name="triviaList"/> should be used as the leading or trailing trivia list.
        /// </param>
        /// <returns>The same translated node with leading and trailing comments.</returns>
        public static T AddCommentsFrom<T>(this T translatedNode, SyntaxTriviaList triviaList, bool addAsLeadingTrivia)
            where T : ITsAstNode
        {
            if (!triviaList.HasComment(includingDocumentationComment: true))
            {
                return translatedNode;
            }

            var translatedTrivia = TranslateTriviaList(triviaList);
            translatedNode = AddTranslatedTrivia(translatedNode, translatedTrivia, addAsLeadingTrivia);

            return translatedNode;
        }

        public static T AppendLeadingCommentsFrom<T>(this T translatedNode, SyntaxNodeOrToken nodeOrToken, bool toLeadingTrivia)
            where T : ITsAstNode
        {
            if (!nodeOrToken.HasLeadingTrivia)
            {
                return translatedNode;
            }

            var translatedTrivia = TranslateTriviaList(nodeOrToken.GetLeadingTrivia());
            translatedNode = toLeadingTrivia
                ? translatedNode.AppendLeadingTrivia(translatedTrivia)
                : translatedNode.AppendTrailingTrivia(translatedTrivia);
            return translatedNode;
        }

        public static T AppendTrailingCommentsFrom<T>(this T translatedNode, SyntaxNodeOrToken nodeOrToken, bool toLeadingTrivia)
            where T : ITsAstNode
        {
            if (!nodeOrToken.HasTrailingTrivia)
            {
                return translatedNode;
            }

            var translatedTrivia = TranslateTriviaList(nodeOrToken.GetTrailingTrivia());
            translatedNode = toLeadingTrivia
                ? translatedNode.AppendLeadingTrivia(translatedTrivia)
                : translatedNode.AppendTrailingTrivia(translatedTrivia);
            return translatedNode;
        }

        public static T PrependTrailingCommentsFrom<T>(this T translatedNode, SyntaxNodeOrToken nodeOrToken, bool toLeadingTrivia)
            where T : ITsAstNode
        {
            if (!nodeOrToken.HasTrailingTrivia)
            {
                return translatedNode;
            }

            var translatedTrivia = TranslateTriviaList(nodeOrToken.GetTrailingTrivia());
            translatedNode = toLeadingTrivia
                ? translatedNode.PrependLeadingTrivia(translatedTrivia)
                : translatedNode.PrependTrailingTrivia(translatedTrivia);
            return translatedNode;
        }

        ///// <summary>
        ///// Adds only "simple" comments, including single and multiple line comments. Structured documentation comments
        ///// will be included, but will not be translated to JsDoc comments. Use <see cref="AddAllCommentsFrom{T}"/> for this.
        ///// </summary>
        ///// <typeparam name="T">The type of translated node.</typeparam>
        ///// <param name="translatedNode">The node that has already been translated.</param>
        ///// <param name="tokenList">
        ///// A list of syntax tokens, where each nodeOrToken's leading and trailing trivia will be added to the
        ///// leading/trailing trivia of the translated node. This is most useful for a <c>Modifiers</c> collection
        ///// (public, static, etc.).
        ///// </param>
        ///// <param name="addAsLeadingTrivia">
        ///// Indicates if the translated trivia should be added to the translated node's leading or trailing trivia.
        ///// </param>
        ///// <returns>The same translated node with leading and trailing comments.</returns>
        //public static T AddCommentsFrom<T>(this T translatedNode, SyntaxTokenList tokenList, bool addAsLeadingTrivia)
        //    where T : ITsAstNode
        //{
        //    var translatedTrivia = new List<ITsAstTriviaNode>();

        //    foreach (SyntaxToken nodeOrToken in tokenList)
        //    {
        //        ITsAstTriviaNode[] translatedLeadingTrivia = TranslateTriviaList(nodeOrToken.LeadingTrivia);
        //        ITsAstTriviaNode[] translatedTrailingTrivia = TranslateTriviaList(nodeOrToken.TrailingTrivia);
        //        translatedTrivia.AddRange(translatedLeadingTrivia);
        //        translatedTrivia.AddRange(translatedTrailingTrivia);
        //    }

        //    translatedNode = AddTranslatedTrivia(translatedNode, translatedTrivia.ToArray(), addAsLeadingTrivia);
        //    return translatedNode;
        //}

        private static T AddTranslatedTrivia<T>(
            T translatedNode,
            ITsAstTriviaNode[] translatedTrivia,
            bool addAsLeadingTrivia)
            where T : ITsAstNode
        {
            translatedNode = addAsLeadingTrivia
                ? translatedNode.AppendLeadingTrivia(translatedTrivia)
                : translatedNode.AppendTrailingTrivia(translatedTrivia);

            return translatedNode;
        }

        /// <summary>
        /// Translates all of the trivia in the specified <see cref="SyntaxTriviaList"/> into corresponding TypeScript
        /// whitespace and comments.
        /// </summary>
        /// <param name="triviaList">The node's trivia list to translate.</param>
        /// <param name="context">
        /// If provided, documentation comments are syntactically parsed and converted into JsDoc comments.
        /// </param>
        /// <param name="symbol">
        /// If <paramref name="context"/> is provided, this must be a valid <see cref="ISymbol"/> corresponding to the node.
        /// </param>
        /// <returns></returns>
        private static ITsAstTriviaNode[] TranslateTriviaList(
            SyntaxTriviaList triviaList,
            TranslationContext? context = null,
            [NotNullIfNotNull("context")] ISymbol? symbol = null)
        {
            var translatedTriviaNodes = new List<ITsAstTriviaNode>();
            bool shouldTranslateDocumentationComment = context != null && symbol != null;

            foreach (SyntaxTrivia trivia in triviaList)
            {
                // Treat documentation comments as a single line comment if we're not supposed to translate them as
                // JsDoc comments.
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                    (!shouldTranslateDocumentationComment && trivia.HasStructure))
                {
                    var singleLineComment = TranslateSingleLineComment(trivia);
                    translatedTriviaNodes.Add(singleLineComment);
                }
                else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    var multiLineComment = TranslateMultiLineComment(trivia);
                    translatedTriviaNodes.Add(multiLineComment);
                }
                else if (context != null && symbol != null && trivia.HasStructure)
                {
                    DocumentationComment? documentationComment = symbol.GetDocumentationComment(
                        preferredCulture: CultureInfo.InvariantCulture,
                        expandIncludes: true,
                        cancellationToken: context.CancellationToken);

                    if (documentationComment == null)
                    {
                        continue;
                    }

                    var result = DocumentationCommentTranslator.Translate(documentationComment);
                    context.Diagnostics.AddRange(result.Diagnostics);
                    translatedTriviaNodes.Add(result.Result);
                }
            }

            return translatedTriviaNodes.ToArray();
        }

        private static ITsSingleLineComment TranslateSingleLineComment(SyntaxTrivia trivia)
        {
            string comment = trivia.ToString().Trim();

            // Remove the leading comment marker, since the translated node will automatically add them.
            if (comment.StartsWith("//", StringComparison.Ordinal))
            {
                comment = comment.Substring(2);
            }

            var singleLineComment = Factory.SingleLineComment(comment, preserveSpacing: true);
            return singleLineComment;
        }

        private static ITsMultiLineComment TranslateMultiLineComment(SyntaxTrivia trivia)
        {
            string[] lines = trivia.ToFullString()
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split('\n')
                .Select(line => line.Trim())
                .Select(line => line.StartsWith("/*", StringComparison.Ordinal) ? line.Substring(2) : line)
                .Select(
                    line => line.EndsWith("*/", StringComparison.Ordinal)
                        ? line.Substring(0, line.Length - 2)
                        : line)
                .ToArray();

            var multiLineComment = Factory.MultiLineComment(isJsDoc: false, preserveSpacing: true, lines);
            return multiLineComment;
        }
    }
}
