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
    using System.Text;
    using CompilerUtilities;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;
    using TypeScriptAst.TypeScript.Ast;
    using Factory = TypeScriptAst.TypeScript.Ast.TsAstFactory;
    using XmlNames = DocumentationCommentXmlNames;

    /// <summary>
    /// Converts a CSharp XML documentation comment into a TypeScript JsDoc comment.
    /// </summary>
    internal class DocumentationCommentTranslator
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static IExtendedResult<ITsJsDocComment> Translate(DocumentationComment documentationComment)
        {
            var comment = documentationComment ?? throw new ArgumentNullException(nameof(documentationComment));

            var diagnostics = new List<Diagnostic>();
            ITsJsDocCommentBuilder builder = Factory.JsDocCommentBuilder();

            // if there is <summary> and <remarks>, then append the remarks after the summary,
            // otherwise just use one or the other as the description
            string description = comment.SummaryText;
            if (!string.IsNullOrEmpty(comment.RemarksText))
            {
                if (description == null)
                {
                    description = comment.RemarksText;
                }
                else
                {
                    description += "\n" + comment.RemarksText;
                }
            }

            builder.SetDescription(TranslateElementText(description, diagnostics));

            // <example>
            builder.AddExampleTag(TranslateElementText(comment.ExampleText, diagnostics));

            // translate each <param> tag
            foreach (string parameterName in comment.ParameterNames)
            {
                string parameterText = comment.GetParameterText(parameterName);
                builder.AddParamTag(parameterName, TranslateElementText(parameterText, diagnostics));
            }

            // translate each <typeparam> tags, even though there is no JSDoc equivalent
            foreach (string typeParameterName in comment.TypeParameterNames)
            {
                string parameterText = comment.GetTypeParameterText(typeParameterName);
                builder.AddTypeParamTag(typeParameterName, TranslateElementText(parameterText, diagnostics));
            }

            // <returns>
            builder.SetReturnsTag(TranslateElementText(comment.ReturnsText, diagnostics));

            // translate each <exception> tag
            foreach (string typeName in comment.ExceptionTypes)
            {
                foreach (string exceptionText in comment.GetExceptionTexts(typeName))
                {
                    builder.AddThrowsTag(RemoveNamespace(typeName), TranslateElementText(exceptionText, diagnostics));
                }
            }

            ITsJsDocComment translatedComment = builder.Build();
            return new ExtendedResult<ITsJsDocComment>(translatedComment, diagnostics);
        }

        private static string RemoveNamespace(string fullTypeName)
        {
            return fullTypeName.TrimEnd('.').Split('.').Last();
        }

        /// <summary>
        /// Translates any embedded XML blocks into the relevant JSDoc equivalent.
        /// </summary>
        /// <param name="text">The XML documentation comment text to translate.</param>
        /// <param name="diagnostics">The diagnosit lise to use for reporting errors.</param>
        /// <returns>The translated text in JSDoc format.</returns>
        private static ITsJsDocBlock TranslateElementText(string text, ICollection<Diagnostic> diagnostics)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var textItems = new List<ITsJsDocInlineContent>();
            var builder = new StringBuilder();

            // loop over all of the text items
            using (var reader = new PeekingTextReader(text))
            {
                while (!reader.IsAtEnd)
                {
                    builder.Append(reader.ReadUntil('<'));

                    if (reader.Peek() != '<')
                    {
                        break;
                    }

                    AddNextTextItem(reader);
                }
            }

            // ReSharper disable once ImplicitlyCapturedClosure
            void AddNextTextItem(PeekingTextReader reader)
            {
                DocumentationCommentXmlElement element = DocumentationCommentXmlElement.Parse(reader, diagnostics);
                string elementName = element.ElementName;

                // <c>x</c> and <code>x</code> translates to `x`
                if (XmlNames.ElementIsOneOf(elementName, XmlNames.CElementName, XmlNames.CodeElementName))
                {
                    if (!string.IsNullOrWhiteSpace(element.Content))
                    {
                        builder.Append("`").Append(element.Content).Append("`");
                    }
                }

                // <see langword="x"/> translates to `x`
                else if (XmlNames.ElementEquals(elementName, XmlNames.SeeElementName) &&
                    element.Attributes.ContainsKey(XmlNames.LangwordAttributeName))
                {
                    builder.Append("`").Append(element.Attributes[XmlNames.LangwordAttributeName]).Append("`");
                }

                // translate <see href="url">x</see> (or seealso or a) to [x]{@link url}
                else if (XmlNames.ElementIsOneOf(
                        elementName,
                        XmlNames.SeeElementName,
                        XmlNames.SeeAlsoElementName,
                        XmlNames.AElementName) &&
                    element.Attributes.ContainsKey(XmlNames.HrefAttributeName))
                {
                    ITsJsDocLinkTag linkTag = Factory.JsDocLinkTag(
                        element.Attributes[XmlNames.HrefAttributeName],
                        element.Content);
                    AddJsDocLink(linkTag);
                }

                // translate <see(also) cref="x">Text</see(also)> to [Text]{@link x}
                else if (XmlNames.ElementIsOneOf(elementName, XmlNames.SeeElementName, XmlNames.SeeAlsoElementName))
                {
                    if (!element.Attributes.ContainsKey(XmlNames.CrefAttributeName))
                    {
                        diagnostics.Add(
                            DiagnosticFactory.InternalError(
                                new Exception("TODO: Missing cref attribute in documentation comment: {0}")));
                    }

                    // parse cref type references
                    var cref = DocumentationCommentCref.Parse(element.Attributes[XmlNames.CrefAttributeName]);
                    string linkUrl = cref.ToString();

                    // create a new link
                    AddJsDocLink(Factory.JsDocLinkTag(linkUrl, element.Content));
                }
            }

            void AddJsDocLink(ITsJsDocLinkTag linkTag)
            {
                // add the current builder's contents to the text items
                textItems.Add(Factory.JsDocInlineText(builder.ToString()));

                // create a new link
                textItems.Add(linkTag);

                // clear out the builder for the next text run
                builder.Clear();
            }

            // add the last run to the text items
            if (builder.Length > 0)
            {
                textItems.Add(Factory.JsDocInlineText(builder.ToString().TrimEnd()));
            }

            return Factory.JsDocBlock(textItems.ToArray());
        }
    }
}
