// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RoslynExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Globalization;
    using System.Threading;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    /// <summary>
    /// Contains extension methods for working with Roslyn data types.
    /// </summary>
    internal static class RoslynExtensions
    {
        /// <summary>
        /// Extracts the semanic type symbol from the specified type syntax node.
        /// </summary>
        public static ITypeSymbol GetTypeSymbol(this TypeSyntax typeSyntax, SemanticModel semanticModel)
        {
            return semanticModel.GetTypeInfo(typeSyntax).Type;
        }

        public static bool IsInterfaceType(this ITypeSymbol symbol)
        {
            return symbol?.TypeKind == TypeKind.Interface;
        }

        public static ITsIdentifier GetScriptName(
            this ISymbol symbol,
            ScriptNameSymbolTable scriptNameSymbolTable,
            string defaultName)
        {
            return Factory.Identifier(scriptNameSymbolTable.GetValueOrDefault(symbol, defaultName));
        }

        /// <summary>
        /// Parses an XML documentation comment belonging to the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol containing a potential XML documentation comment.</param>
        /// <param name="preferredCulture">Preferred culture or null for the default.</param>
        /// <param name="expandIncludes">
        /// Optionally, expand &lt;include&gt; elements. No impact on non-source documentation comments.
        /// </param>
        /// <param name="cancellationToken">Token allowing cancellation of request.</param>
        /// <returns>
        /// The parsed <see cref="DocumentationComment"/> or <see cref="DocumentationComment.Empty"/>
        /// if there is no XML documentation comment for the specified symbol.
        /// </returns>
        public static DocumentationComment GetDocumentationComment(
            this ISymbol symbol,
            CultureInfo preferredCulture = null,
            bool expandIncludes = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string xmlText = symbol.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
            return string.IsNullOrEmpty(xmlText)
                ? DocumentationComment.Empty
                : DocumentationComment.FromXmlFragment(xmlText, symbol);
        }
    }
}
