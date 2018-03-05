// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationCommentTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Text.RegularExpressions;
    using Desalt.Core.TypeScript.Ast;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    /// <summary>
    /// Converts a CSharp XML documentation comment into a TypeScript JsDoc comment.
    /// </summary>
    internal class DocumentationCommentTranslator
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private const RegexOptions InlineElementOptions = RegexOptions.CultureInvariant |
            RegexOptions.IgnoreCase |
            RegexOptions.ExplicitCapture |
            RegexOptions.Singleline;

        private static readonly Regex s_seeLangwordRegex = new Regex(
            @"<see\s+langword\s*=\s*""(?<langword>[^""]+)""\s*/>",
            InlineElementOptions);

        private static readonly Regex s_ctagRegex = new Regex(@"<c>(?<content>[^>]*)</c>", InlineElementOptions);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsMultiLineComment Translate(DocumentationComment documentationComment)
        {
            ITsMultiLineComment jsDocComment = Factory.MultiLineComment(
                isJsDoc: true,
                lines: new[] { TranslateElementText(documentationComment.SummaryText) });

            return jsDocComment;
        }

        private static string TranslateElementText(string text)
        {
            string translated = TranslateKnownXmlTags(text).Trim();
            return translated;
        }

        private static string TranslateKnownXmlTags(string text)
        {
            string translated = text;

            // translate <see langword="x"/> to `x`.
            translated = s_seeLangwordRegex.Replace(translated, match => $"`{match.Groups["langword"]}`");

            // translate <c>x</c> to `x`.
            translated = s_ctagRegex.Replace(translated, match => $"`{match.Groups["content"]}`");

            return translated;
        }
    }
}
