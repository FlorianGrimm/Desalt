// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationCommentXmlElement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Desalt.CompilerUtilities;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents an XML element inside of a documentation comment.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    internal sealed class DocumentationCommentXmlElement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private DocumentationCommentXmlElement(
            string elementName,
            IEnumerable<KeyValuePair<string, string>>? attributes = null,
            string? content = null)
        {
            ElementName = !string.IsNullOrWhiteSpace(elementName)
                ? elementName
                : throw new ArgumentNullException(nameof(elementName));

            Attributes =
                attributes?.ToImmutableDictionary(keyComparer: DocumentationCommentXmlNames.AttributeComparer) ??
                ImmutableDictionary<string, string>.Empty;

            Content = content ?? string.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string ElementName { get; }
        public ImmutableDictionary<string, string> Attributes { get; }
        public string Content { get; }

        private string DebuggerDisplay => ToString();

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static DocumentationCommentXmlElement Create(
            string elementName,
            IEnumerable<KeyValuePair<string, string>>? attributes = null,
            string? content = null)
        {
            return new DocumentationCommentXmlElement(elementName, attributes, content);
        }

        /// <summary>
        /// Parses an XML element and returns the result.
        /// </summary>
        /// <param name="reader">The reader to use for parsing.</param>
        /// <param name="diagnostics">An optional diagnostic list to use for reporting errors.</param>
        /// <returns>The parsed XML element or null if there were errors.</returns>
        public static DocumentationCommentXmlElement? Parse(
            PeekingTextReader reader,
            ICollection<Diagnostic>? diagnostics = null)
        {
            if (reader.Peek() != '<')
            {
                throw new InvalidOperationException("Shouldn't be called unless at an XML start character.");
            }

            diagnostics ??= new List<Diagnostic>();

            // skip the <
            reader.Read();

            // get the element name
            string? elementName = reader.ReadUntil(c => char.IsWhiteSpace(c) || c.IsOneOf('/', '>'));
            reader.SkipWhitespace();

            if (string.IsNullOrWhiteSpace(elementName))
            {
                diagnostics.Add(
                    DiagnosticFactory.InternalError(
                        new Exception("TODO: Malformed XML in documentation comment: {0}")));
                return null;
            }

            // get the attributes
            var attributes = new Dictionary<string, string>(DocumentationCommentXmlNames.AttributeComparer);
            while (reader.Peek() != '>' && reader.Peek(2) != "/>")
            {
                string? attributeName = reader.ReadUntil('=')?.Trim();
                reader.ReadUntil('"');
                reader.Read();

                string? attributeValue = reader.ReadUntil('"');
                reader.Read();
                reader.SkipWhitespace();

                if (string.IsNullOrWhiteSpace(attributeName) || string.IsNullOrWhiteSpace(attributeValue))
                {
                    diagnostics.Add(
                        DiagnosticFactory.InternalError(
                            new Exception("TODO: Malformed XML in documentation comment: {0}")));
                }
                else
                {
                    attributes.Add(attributeName, attributeValue);
                }
            }

            // skip over the closing tag
            if (reader.Peek(2) == "/>")
            {
                reader.Read(2);
            }

            // get the content - embedded XML is not supported
            string? content = string.Empty;
            if (reader.Peek() == '>')
            {
                reader.Read();
                string closingTag = $"</{elementName}";
                content = reader.ReadUntil(closingTag);
                reader.Read(closingTag.Length);
                reader.SkipWhitespace();

                // read the >
                if (reader.Peek() != '>')
                {
                    diagnostics.Add(
                        DiagnosticFactory.InternalError(
                            new Exception("TODO: Malformed XML in documentation comment: {0}")));
                }

                reader.Read();
            }

            return new DocumentationCommentXmlElement(elementName, attributes, content);
        }

        public override string ToString()
        {
            var builder = new StringBuilder($"<{ElementName}");

            if (Attributes.Count > 0)
            {
                foreach (KeyValuePair<string, string> pair in Attributes.OrderBy(
                    pair => pair.Key,
                    DocumentationCommentXmlNames.AttributeComparer))
                {
                    builder.Append(" ").Append(pair.Key).Append("=\"").Append(pair.Value).Append("\"");
                }
            }

            if (string.IsNullOrWhiteSpace(Content))
            {
                builder.Append("/>");
            }
            else
            {
                builder.Append(">").Append(Content).Append("</").Append(ElementName).Append(">");
            }

            return builder.ToString();
        }
    }
}
