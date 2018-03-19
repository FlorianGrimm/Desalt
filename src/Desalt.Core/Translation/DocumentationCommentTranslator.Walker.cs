// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationCommentTranslator.Walker.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Extensions;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;
    using XmlNames = DocumentationCommentXmlNames;

    internal partial class DocumentationCommentTranslator
    {
        /// <summary>
        /// Contains a syntax walker that parses a C# XML documentation comment.
        /// </summary>
        private sealed class Walker : CSharpSyntaxWalker
        {
            //// =======================================================================================================
            //// Member Variables
            //// =======================================================================================================

            private readonly Stack<XmlElementInfo> _elementTexts = new Stack<XmlElementInfo>();
            private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

            private ITsJsDocCommentBuilder _builder;

            //// =======================================================================================================
            //// Constructors
            //// =======================================================================================================

            public Walker()
                : base(SyntaxWalkerDepth.StructuredTrivia)
            {
            }

            //// =======================================================================================================
            //// Properties
            //// =======================================================================================================

            public ITsJsDocComment TranslatedComment => _builder?.Build() ?? Factory.JsDocComment();

            public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();

            //// =======================================================================================================
            //// Methods
            //// =======================================================================================================

            /// <summary>
            /// Called when the visitor visits a DocumentationCommentTriviaSyntax node.
            /// </summary>
            public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
            {
                _diagnostics.Clear();
                _elementTexts.Clear();
                _builder = Factory.JsDocCommentBuilder();
                base.VisitDocumentationCommentTrivia(node);
            }

            /// <summary>
            /// Called when the visitor visits a XmlElementStartTagSyntax node.
            /// </summary>
            public override void VisitXmlElementStartTag(XmlElementStartTagSyntax node)
            {
                var elementInfo = new XmlElementInfo(node.Name.LocalName.Text);
                if (node.Name.LocalName.Text.IsOneOf(XmlNames.SeeElementName, XmlNames.SeeAlsoElementName))
                {
                    elementInfo.NameOrCrefAttributeValue = GetSeeReferenceValue(node.Attributes).value;
                }

                _elementTexts.Push(elementInfo);
                base.VisitXmlElementStartTag(node);
            }

            /// <summary>
            /// Called when the visitor visits a XmlEmptyElementSyntax node.
            /// </summary>
            public override void VisitXmlEmptyElement(XmlEmptyElementSyntax node)
            {
                // for <see/> or <seealso/> where there is no text, VisitXmlElementStartTag/EndTag
                // is not called, so we'll translate the link tag here
                if (node.Name.LocalName.Text.IsOneOf(XmlNames.SeeElementName, XmlNames.SeeAlsoElementName))
                {
                    var (tagName, value) = GetSeeReferenceValue(node.Attributes);
                    ITsJsDocInlineContent text = tagName == XmlNames.LangwordAttributeName
                        ? (ITsJsDocInlineContent)Factory.JsDocInlineText($"`{value}`")
                        : Factory.JsDocLinkTag(value);
                    AppendTextOnCurrentElement(text, node);
                }

                base.VisitXmlEmptyElement(node);
            }

            /// <summary>
            /// Called when the visitor visits a XmlCrefAttributeSyntax node.
            /// </summary>
            public override void VisitXmlCrefAttribute(XmlCrefAttributeSyntax node)
            {
                _elementTexts.Peek().NameOrCrefAttributeValue = node.Cref.ToString();
                base.VisitXmlCrefAttribute(node);
            }

            /// <summary>
            /// Called when the visitor visits a XmlNameAttributeSyntax node.
            /// </summary>
            public override void VisitXmlNameAttribute(XmlNameAttributeSyntax node)
            {
                _elementTexts.Peek().NameOrCrefAttributeValue = node.Identifier.Identifier.Text;
                base.VisitXmlNameAttribute(node);
            }

            /// <summary>
            /// Called when the visitor visits a XmlTextSyntax node.
            /// </summary>
            public override void VisitXmlText(XmlTextSyntax node)
            {
                string text = node.TextTokens.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    AppendTextOnCurrentElement(Factory.JsDocInlineText(text), node);
                }

                base.VisitXmlText(node);
            }

            /// <summary>
            /// Called when the visitor visits a XmlElementEndTagSyntax node.
            /// </summary>
            public override void VisitXmlElementEndTag(XmlElementEndTagSyntax node)
            {
                XmlElementInfo elementInfo = _elementTexts.Pop();
                string elementName = elementInfo.Name;
                ITsJsDocBlock text = elementInfo.Text;

                switch (elementName)
                {
                    case XmlNames.SummaryElementName:
                        _builder.PrependDescription(text, separateWithBlankLine: true);
                        break;

                    case XmlNames.ParameterElementName:
                        _builder.AddParamTag(elementInfo.NameOrCrefAttributeValue, text);
                        break;

                    case XmlNames.ReturnsElementName:
                        _builder.SetReturnsTag(text);
                        break;

                    case XmlNames.RemarksElementName:
                        _builder.AppendDescription(text, separateWithBlankLine: true);
                        break;

                    case XmlNames.ExampleElementName:
                        _builder.AddExampleTag(text);
                        break;

                    case XmlNames.ExceptionElementName:
                        _builder.AddThrowsTag(elementInfo.NameOrCrefAttributeValue, text);
                        break;

                    case XmlNames.TypeParameterElementName:
                        _builder.AddTypeParamTag(elementInfo.NameOrCrefAttributeValue, text);
                        break;

                    case XmlNames.SeeElementName:
                    case XmlNames.SeeAlsoElementName:
                        AppendTextOnCurrentElement(
                            Factory.JsDocLinkTag(elementInfo.NameOrCrefAttributeValue, text.EmitAsString()),
                            node);
                        break;

                    case XmlNames.CElementName:
                        AppendTextOnCurrentElement(Factory.JsDocInlineText($"`{text.EmitAsString()}`"), node);
                        break;
                }

                base.VisitXmlElementEndTag(node);
            }

            private static (string tagName, string value) GetSeeReferenceValue(SyntaxList<XmlAttributeSyntax> attributes)
            {
                var attributeInfo = (from attribute in attributes
                                     let attributeName = attribute.Name.LocalName.Text
                                     where attributeName.IsOneOf(
                                         XmlNames.CrefAttributeName,
                                         XmlNames.HrefAttributeName,
                                         XmlNames.LangwordAttributeName)
                                     let relStart = attribute.StartQuoteToken.SpanStart + 1 - attribute.SpanStart + 1
                                     let relEnd = attribute.EndQuoteToken.SpanStart - attribute.SpanStart + 1
                                     select (tagName: attributeName,
                                         value: attribute.GetText()
                                             .GetSubText(TextSpan.FromBounds(relStart, relEnd))
                                             .ToString())).Single();

                return attributeInfo;
            }

            private void AppendTextOnCurrentElement(ITsJsDocInlineContent text, CSharpSyntaxNode node)
            {
                if (text == null)
                {
                    return;
                }

                if (_elementTexts.Count > 0)
                {
                    XmlElementInfo elementInfo = _elementTexts.Pop();
                    elementInfo.Text = elementInfo.Text?.WithAppendedContent(text) ??
                        Factory.JsDocBlock(text);
                    _elementTexts.Push(elementInfo);
                }
                else
                {
                    _diagnostics.Add(
                        DiagnosticFactory.UnstructuredXmlTextNotSupported(text.EmitAsString(), node.GetLocation()));
                }
            }

            private sealed class XmlElementInfo
            {
                public XmlElementInfo(string name)
                {
                    Name = name;
                }

                public string Name { get; }

                public string NameOrCrefAttributeValue { get; set; }

                public ITsJsDocBlock Text { get; set; }
            }
        }
    }
}
