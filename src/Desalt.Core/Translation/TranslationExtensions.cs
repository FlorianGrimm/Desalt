// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Provides extension methods to help with translation so that we can have a fluent API.
    /// </summary>
    internal static class TranslationExtensions
    {
        /// <summary>
        /// Translates the C# XML documentation comment into a JSDoc comment if there is a
        /// documentation comment on the specified node.
        /// </summary>
        /// <typeparam name="T">The type of the translated node.</typeparam>
        /// <param name="translatedNode">The already-translated TypeScript AST node.</param>
        /// <param name="semanticModel">The C# semantic model.</param>
        /// <param name="syntaxNode">The C# syntax node to get documentation comments from.</param>
        /// <returns>
        /// If there are documentation comments, a new TypeScript AST node with the translated JsDoc
        /// comments prepended. If there are no documentation comments, the same node is returned.
        /// </returns>
        public static T WithDocumentationComment<T>(
            this T translatedNode,
            SemanticModel semanticModel,
            SyntaxNode syntaxNode) where T : IAstNode
        {
            if (!syntaxNode.HasStructuredTrivia)
            {
                return translatedNode;
            }

            ISymbol symbol = semanticModel.GetDeclaredSymbol(syntaxNode);
            if (symbol == null)
            {
                return translatedNode;
            }

            DocumentationComment documentationComment = symbol.GetDocumentationComment();
            var jsDocComment = DocumentationCommentTranslator.Translate(documentationComment);

            return translatedNode.WithLeadingTrivia(jsDocComment);
        }

        /// <summary>
        /// Converts the translated declaration to an exported declaration if the C# declaration is public.
        /// </summary>
        /// <param name="translatedDeclaration">The TypeScript declaration to conditionally export.</param>
        /// <param name="semanticModel">The C# semantic model.</param>
        /// <param name="node">The C# syntax node to inspect.</param>
        /// <returns>
        /// If the type does not need to be exported, <paramref name="translatedDeclaration"/> is
        /// returned; otherwise a wrapped exported <see cref="ITsExportImplementationElement"/> is returned.
        /// </returns>
        public static ITsImplementationModuleElement AndExportIfNeeded(
            this ITsImplementationElement translatedDeclaration,
            SemanticModel semanticModel,
            BaseTypeDeclarationSyntax node)
        {
            // determine if this declaration should be exported
            INamedTypeSymbol symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol.DeclaredAccessibility != Accessibility.Public)
            {
                return translatedDeclaration;
            }

            ITsExportImplementationElement exportedInterfaceDeclaration =
                TsAstFactory.ExportImplementationElement(translatedDeclaration);

            return exportedInterfaceDeclaration.WithDocumentationComment(semanticModel, node);
        }

        /// <summary>
        /// Calls <see cref="AndExportIfNeeded"/> followed by <see cref="WithDocumentationComment{T}"/>.
        /// </summary>
        /// <param name="translatedDeclaration">The TypeScript declaration to conditionally export.</param>
        /// <param name="semanticModel">The C# semantic model.</param>
        /// <param name="node">The C# syntax node to inspect.</param>
        /// <returns></returns>
        public static ITsImplementationModuleElement AndExportIfNeededWithDocumentationComment(
            this ITsImplementationElement translatedDeclaration,
            SemanticModel semanticModel,
            BaseTypeDeclarationSyntax node)
        {
            ITsImplementationModuleElement exported = translatedDeclaration.AndExportIfNeeded(semanticModel, node);
            return exported.WithDocumentationComment(semanticModel, node);
        }
    }
}
