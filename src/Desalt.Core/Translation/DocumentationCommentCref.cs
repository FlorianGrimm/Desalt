// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationCommentCref.cs" company="Justin Rockwood">
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
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;
    using CrefKind = DocumentationCommentCrefKind;

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
        //// Member Variables
        //// ===========================================================================================================

        private static readonly ImmutableDictionary<char, CrefKind> s_charToKindMap =
            new Dictionary<char, CrefKind>
            {
                ['T'] = CrefKind.Type,
                ['F'] = CrefKind.Field,
                ['M'] = CrefKind.Method,
                ['P'] = CrefKind.Property,
                ['E'] = CrefKind.Event,
            }.ToImmutableDictionary();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private DocumentationCommentCref(
            CrefKind kind,
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
        public CrefKind Kind { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static DocumentationCommentCref Parse(string cref)
        {
            string fullTypeName;
            string memberName = null;
            CrefKind kind;

            using (var reader = new PeekingTextReader(cref))
            {
                char prefix = reader.Read(2).FirstOrDefault();

                switch (prefix)
                {
                    case 'T':
                        kind = CrefKind.Type;
                        fullTypeName = reader.ReadToEnd();
                        break;

                    case 'M':
                    case 'P':
                    case 'E':
                    case 'F':
                        kind = s_charToKindMap[prefix];
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
        public override string ToString() => Kind == CrefKind.Type ? TypeName : $"{TypeName}.{MemberName}";

        public string ToFullString() => Kind == CrefKind.Type ? FullTypeName : $"{FullTypeName}.{MemberName}";

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
        Field,
    }
}
