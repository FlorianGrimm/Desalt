// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationCommentCref.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Linq;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a parsed cref attribute in an XML documentation comment. The fully-qualified form
    /// is accepted, which is the result of calling <see cref="ISymbol.GetDocumentationCommentXml"/>.
    /// The form is a single letter qualifier specifying the type of member (T for type, M for
    /// method, E for enum, etc.) followed by a colon and then a fully-qualified namespace name and
    /// optional method signature.
    /// </summary>
    internal sealed class DocumentationCommentCref
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private DocumentationCommentCref(
            DocumentationCommentCrefKind kind,
            string fullTypeName,
            string typeName,
            string memberName)
        {
            Kind = kind;
            FullTypeName = fullTypeName;
            TypeName = typeName;
            MemberName = memberName;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string FullTypeName { get; }
        public string TypeName { get; }
        public string MemberName { get; }
        public DocumentationCommentCrefKind Kind { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static DocumentationCommentCref Parse(string cref)
        {
            string fullTypeName;
            string memberName = null;
            DocumentationCommentCrefKind kind;

            using (var reader = new PeekingTextReader(cref))
            {
                char prefix = reader.Read(2).FirstOrDefault();

                switch (prefix)
                {
                    case 'T':
                        kind = DocumentationCommentCrefKind.Type;
                        fullTypeName = reader.ReadToEnd();
                        break;

                    case 'M':
                    case 'P':
                    case 'E':
                        kind = prefix == 'M' ? DocumentationCommentCrefKind.Method :
                            prefix == 'P' ? DocumentationCommentCrefKind.Property : DocumentationCommentCrefKind.Event;
                        string qualifiedName = reader.ReadUntil('(');
                        (fullTypeName, memberName) = SplitQualifiedName(qualifiedName);
                        break;

                    default:
                        throw new InvalidOperationException("Invalid cref attribute: " + cref);
                }
            }

            string typeName = fullTypeName.Split('.').Last();

            return new DocumentationCommentCref(kind, fullTypeName, typeName, memberName);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            switch (Kind)
            {
                case DocumentationCommentCrefKind.Type:
                    return TypeName;

                case DocumentationCommentCrefKind.Method:
                case DocumentationCommentCrefKind.Property:
                case DocumentationCommentCrefKind.Event:
                    return $"{TypeName}.{MemberName}";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string ToFullString()
        {
            return Kind == DocumentationCommentCrefKind.Type ? FullTypeName : $"{FullTypeName}.{MemberName}";
        }

        private static (string typeName, string memberName) SplitQualifiedName(string qualifiedName)
        {
            string[] pieces = qualifiedName.Split('.');
            return (string.Join(".", pieces.Take(pieces.Length - 1)), pieces.Last());
        }
    }

    internal enum DocumentationCommentCrefKind
    {
        Type,
        Method,
        Property,
        Event,
    }
}
