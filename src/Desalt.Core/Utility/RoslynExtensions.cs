// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RoslynExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Utility
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Translation;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Contains extension methods for working with Roslyn data types.
    /// </summary>
    internal static class RoslynExtensions
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly SymbolDisplayFormat s_symbolDisplayFormat = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions:
            SymbolDisplayMemberOptions.IncludeContainingType |
            SymbolDisplayMemberOptions.IncludeParameters |
            SymbolDisplayMemberOptions.IncludeExplicitInterface,
            delegateStyle: SymbolDisplayDelegateStyle.NameOnly,
            extensionMethodStyle: SymbolDisplayExtensionMethodStyle.StaticMethod,
            parameterOptions:
            SymbolDisplayParameterOptions.IncludeName |
            SymbolDisplayParameterOptions.IncludeType |
            SymbolDisplayParameterOptions.IncludeParamsRefOut,
            propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
            localOptions: SymbolDisplayLocalOptions.IncludeType,
            kindOptions: SymbolDisplayKindOptions.None,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a string representation of the specified symbol that is suitable for use in a
        /// hash table or for using in unit tests.
        /// </summary>
        /// <param name="symbol">The symbol to display.</param>
        public static string ToHashDisplay(this ISymbol symbol)
        {
            return symbol.ToDisplayString(s_symbolDisplayFormat);
        }

        /// <summary>
        /// Extracts the semantic type symbol from the specified type syntax node.
        /// </summary>
        public static ITypeSymbol GetTypeSymbol(this TypeSyntax typeSyntax, SemanticModel semanticModel)
        {
            return semanticModel.GetTypeInfo(typeSyntax).Type!;
        }

        public static bool IsInterfaceType(this ITypeSymbol symbol)
        {
            return symbol?.TypeKind == TypeKind.Interface;
        }

        public static ITsIdentifier GetScriptName(
            this ISymbol symbol,
            ScriptSymbolTable scriptSymbolTable,
            string defaultName)
        {
            return Factory.Identifier(scriptSymbolTable.GetComputedScriptNameOrDefault(symbol, defaultName));
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
        /// The parsed <see cref="DocumentationComment"/> or null if there is no XML documentation comment for the
        /// specified symbol.
        /// </returns>
        public static DocumentationComment? GetDocumentationComment(
            this ISymbol symbol,
            CultureInfo? preferredCulture = null,
            bool expandIncludes = false,
            CancellationToken cancellationToken = default)
        {
            string? xmlText = symbol.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
            return string.IsNullOrEmpty(xmlText)
                ? null
                : DocumentationComment.FromXmlFragment(xmlText, symbol);
        }

        /// <summary>
        /// Returns all of the declared types in the <see cref="SyntaxNode"/> children.
        /// </summary>
        /// <param name="rootSyntax">The root <see cref="CompilationUnitSyntax"/> node.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> to use.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to use for canceling the requests.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{INamedTypeSymbol}"/> of all of the declared type symbols in the
        /// syntax tree.
        /// </returns>
        public static IEnumerable<INamedTypeSymbol> GetAllDeclaredTypes(
            this CompilationUnitSyntax rootSyntax,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            INamedTypeSymbol? GetDeclaredSymbol(SyntaxNode node)
            {
                switch (node)
                {
                    case BaseTypeDeclarationSyntax typeDeclarationSyntax:
                        return semanticModel.GetDeclaredSymbol(typeDeclarationSyntax, cancellationToken);

                    case DelegateDeclarationSyntax delegateDeclarationSyntax:
                        return semanticModel.GetDeclaredSymbol(delegateDeclarationSyntax, cancellationToken);

                    default:
                        return null;
                }
            }

            var query = rootSyntax.DescendantNodes().Select(GetDeclaredSymbol).WhereNotNull();
            return query;
        }
    }
}
