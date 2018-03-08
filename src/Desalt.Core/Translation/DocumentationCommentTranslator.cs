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
    using System.Collections.Immutable;
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
            @"<see(also)?\s+cref\s*=\s*""(T|M|\!):(?<fullName>(\w+\.?)+).*""\s*/>",
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
        //// Constructors
        //// ===========================================================================================================

        private DocumentationCommentTranslator(DocumentationComment comment)
        {
            Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public DocumentationComment Comment { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsMultiLineComment Translate(DocumentationComment documentationComment)
        {
            var translator = new DocumentationCommentTranslator(documentationComment);
            return translator.TranslateInternal();
        }

        private ITsMultiLineComment TranslateInternal()
        {
            var lines = new List<string>();

            void AddSection(string text, string jsdocPrefix = "")
            {
                if (text != null)
                {
                    lines.AddRange(TranslateElementText(jsdocPrefix + text));
                }
            }

            // translate the <summary> first
            AddSection(Comment.SummaryText);

            // then any <remarks>
            AddSection(Comment.RemarksText);

            // then <example>
            AddSection(Comment.ExampleText, "@example ");

            // translate each <typeparam> tags, even though there is no JSDoc equivalent
            foreach (string typeParameterName in Comment.TypeParameterNames)
            {
                string parameterText = Comment.GetTypeParameterText(typeParameterName);
                lines.AddRange(TranslateParam(typeParameterName, parameterText, isTypeParam: true));
            }

            // translate each <param> tag
            foreach (string parameterName in Comment.ParameterNames)
            {
                string parameterText = Comment.GetParameterText(parameterName);
                lines.AddRange(TranslateParam(parameterName, parameterText));
            }

            // <returns> after the params
            AddSection(Comment.ReturnsText, "@returns ");

            // translate each <exception> tag
            foreach (string exceptionType in Comment.ExceptionTypes)
            {
                string shortExceptionType = RemoveNamespace(exceptionType);

                ImmutableArray<string> exceptionTexts = Comment.GetExceptionTexts(exceptionType);
                foreach (string exceptionText in exceptionTexts)
                {
                    AddSection(exceptionText, $"@throws {{{shortExceptionType}}} ");
                }
            }

            ITsMultiLineComment jsDocComment = Factory.MultiLineComment(isJsDoc: true, lines: lines.ToArray());

            return jsDocComment;
        }

        /// <summary>
        /// Converts a &lt;param name="name"&gt; tag to a JsDoc @param tag.
        /// </summary>
        private IEnumerable<string> TranslateParam(string parameterName, string parameterText, bool isTypeParam = false)
        {
            string[] parameterTextLines = TranslateElementText(parameterText);
            string prefix = isTypeParam ? "typeparam" : "@param";

            yield return $"{prefix} {parameterName} - {parameterTextLines[0]}";
            foreach (string parameterLine in parameterTextLines.Skip(1))
            {
                yield return parameterLine;
            }
        }

        private string[] TranslateElementText(string text)
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
        private string TranslateKnownXmlTags(string text)
        {
            string translated = text;

            // translate <see langword="x"/> to `x`.
            translated = s_seeLangwordRegex.Replace(translated, match => $"`{match.Groups["langword"]}`");

            // translate <c>x</c> to `x`.
            translated = s_ctagRegex.Replace(translated, match => $"`{match.Groups["content"]}`");

            // translate <see/seealso cref="M:Type.Member" /> to '@see Type.Member'
            translated = s_seeCrefMemberRegex.Replace(
                translated,
                match => $"@see {RemoveNamespace(match.Groups["fullName"].Value)}");

            return translated;
        }

        private static string RemoveNamespace(string fullTypeName)
        {
            return fullTypeName.TrimEnd('.').Split('.').Last();
        }
    }
}
