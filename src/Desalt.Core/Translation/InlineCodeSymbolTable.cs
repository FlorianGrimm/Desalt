// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="InlineCodeSymbolTable.cs" company="Justin Rockwood">
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
    using System.Threading;
    using Desalt.Core.Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Represents a symbol table containing [InlineCode] attribute content, which is to be used in
    /// translating calls to the method or property.
    /// </summary>
    internal class InlineCodeSymbolTable : SymbolTable<string>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private InlineCodeSymbolTable(
            ImmutableArray<KeyValuePair<string, string>> documentSymbols,
            ImmutableArray<KeyValuePair<string, string>> directlyReferencedExternalSymbols,
            ImmutableArray<KeyValuePair<string, Lazy<string>>> indirectlyReferencedExternalSymbols)
            : base(documentSymbols, directlyReferencedExternalSymbols, indirectlyReferencedExternalSymbols)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new <see cref="InlineCodeSymbolTable"/> for the specified translation contexts.
        /// </summary>
        /// <param name="contexts">The contexts from which to retrieve symbols.</param>
        /// <param name="directlyReferencedExternalTypeSymbols">
        /// An array of symbols that are directly referenced in the documents, but are defined in
        /// external assemblies.
        /// </param>
        /// <param name="indirectlyReferencedExternalTypeSymbols">
        /// An array of symbols that are not directly referenced in the documents and are defined in
        /// external assemblies.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A new <see cref="InlineCodeSymbolTable"/>.</returns>
        public static InlineCodeSymbolTable Create(
            ImmutableArray<DocumentTranslationContext> contexts,
            ImmutableArray<ITypeSymbol> directlyReferencedExternalTypeSymbols,
            ImmutableArray<INamedTypeSymbol> indirectlyReferencedExternalTypeSymbols,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // process the types defined in the documents
            var documentSymbols = contexts.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(ProcessSymbolsInDocument)
                .ToImmutableArray();

            // process the externally referenced types
            var directlyReferencedExternalSymbols = directlyReferencedExternalTypeSymbols
                .SelectMany(ProcessExternalType)
                .ToImmutableArray();

            // process all of the types and members in referenced assemblies
            var indirectlyReferencedExternalSymbols = indirectlyReferencedExternalTypeSymbols
                .SelectMany(symbol => symbol.ToSingleEnumerable().Concat(DiscoverMembersOfTypeSymbol(symbol)))
                .Select(
                    symbol => new KeyValuePair<string, Lazy<string>>(
                        SymbolTableUtils.KeyFromSymbol(symbol),
                        new Lazy<string>(
                            () => SymbolTableUtils.GetSaltarelleAttributeValueOrDefault(symbol, "InlineCode", null),
                            isThreadSafe: true)))
                .ToImmutableArray();

            return new InlineCodeSymbolTable(
                documentSymbols,
                directlyReferencedExternalSymbols,
                indirectlyReferencedExternalSymbols);
        }

        private static IEnumerable<KeyValuePair<string, string>> ProcessSymbolsInDocument(
            DocumentTranslationContext context)
        {
            // [InlineCode] is only valid on constructors and methods
            return from node in context.RootSyntax.DescendantNodes()
                   where node.Kind()
                       .IsOneOf(
                           SyntaxKind.ConstructorDeclaration,
                           SyntaxKind.MethodDeclaration,
                           SyntaxKind.GetAccessorDeclaration,
                           SyntaxKind.SetAccessorDeclaration)
                   let symbol = context.SemanticModel.GetDeclaredSymbol(node)
                   let inlineCode = SymbolTableUtils.GetSaltarelleAttributeValueOrDefault(symbol, "InlineCode", null)
                   where inlineCode != null
                   select new KeyValuePair<string, string>(SymbolTableUtils.KeyFromSymbol(symbol), inlineCode);
        }

        private static IEnumerable<ISymbol> DiscoverMembersOfTypeSymbol(INamespaceOrTypeSymbol typeSymbol) =>
            from methodSymbol in typeSymbol.GetMembers().OfType<IMethodSymbol>()
            where methodSymbol.MethodKind.IsOneOf(
                MethodKind.Constructor,
                MethodKind.Ordinary,
                MethodKind.PropertyGet,
                MethodKind.PropertySet)
            select methodSymbol;

        private static IEnumerable<KeyValuePair<string, string>> ProcessExternalType(ITypeSymbol typeSymbol)
        {
            return from methodSymbol in DiscoverMembersOfTypeSymbol(typeSymbol)
                   let inlineCode =
                       SymbolTableUtils.GetSaltarelleAttributeValueOrDefault(methodSymbol, "InlineCode", null)
                   where inlineCode != null
                   select new KeyValuePair<string, string>(SymbolTableUtils.KeyFromSymbol(methodSymbol), inlineCode);
        }
    }
}
