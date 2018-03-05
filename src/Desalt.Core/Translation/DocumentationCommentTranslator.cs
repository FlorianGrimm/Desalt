// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationCommentTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        private static readonly Regex s_seeCrefMemberRegex = new Regex(
            @"<see\s+cref\s*=\s*""M:(?<typeName>(\w+\.)+)(?<memberName>\w+).*""\s*/>",
            InlineElementOptions);

        private static readonly Regex s_seeLangwordRegex = new Regex(
            @"<see\s+langword\s*=\s*""(?<langword>[^""]+)""\s*/>",
            InlineElementOptions);

        private static readonly Regex s_ctagRegex = new Regex(@"<c>(?<content>[^>]*)</c>", InlineElementOptions);

        /// <summary>
        /// Used for <see cref="TranslateElementText"/> method, to prevent new allocation of string
        /// </summary>
        private static readonly string[] s_newLineAsStringArray = { Environment.NewLine };

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsMultiLineComment Translate(DocumentationComment documentationComment)
        {
            var lines = new List<string>();

            // translate the <summary> first
            lines.AddRange(TranslateElementText(documentationComment.SummaryText));

            // translate each <param> tag
            foreach (string parameterName in documentationComment.ParameterNames)
            {
                string parameterText = documentationComment.GetParameterText(parameterName);
                lines.AddRange(TranslateParam(parameterName, parameterText));
            }

            ITsMultiLineComment jsDocComment = Factory.MultiLineComment(isJsDoc: true, lines: lines.ToArray());

            return jsDocComment;
        }

        /// <summary>
        /// Converts a &lt;param name="name"&gt; tag to a JsDoc @param tag.
        /// </summary>
        private static IEnumerable<string> TranslateParam(string parameterName, string parameterText)
        {
            string[] parameterTextLines = TranslateElementText(parameterText);

            yield return $"@param {parameterName} {parameterTextLines[0]}";
            foreach (string parameterLine in parameterTextLines.Skip(1))
            {
                yield return parameterLine;
            }
        }

        private static string[] TranslateElementText(string text)
        {
            string[] translated = TranslateKnownXmlTags(text)
                .Trim()
                .Split(s_newLineAsStringArray, StringSplitOptions.RemoveEmptyEntries);

            return translated;
        }

        /// <summary>
        /// Converts known XML documentation tags (see, seealso, etc.) to the TypeScript equivalent.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string TranslateKnownXmlTags(string text)
        {
            string translated = text;

            // translate <see langword="x"/> to `x`.
            translated = s_seeLangwordRegex.Replace(translated, match => $"`{match.Groups["langword"]}`");

            // translate <c>x</c> to `x`.
            translated = s_ctagRegex.Replace(translated, match => $"`{match.Groups["content"]}`");

            // translate <see cref="M:Type.Member" /> to [[Type.Member]]
            translated = s_seeCrefMemberRegex.Replace(
                translated,
                match => $"[[{RemoveNamespace(match.Groups["typeName"].Value)}.{match.Groups["memberName"]}]]");

            return translated;
        }

        private static string RemoveNamespace(string fullTypeName)
        {
            return fullTypeName.TrimEnd('.').Split('.').Last();
        }
    }
}
