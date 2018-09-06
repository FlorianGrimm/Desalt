// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains mappings of defined types in a C# project to the files in which they're defined.
    /// This is needed in order to correctly generate <c>import</c> statements at the top of each
    /// translated TypeScript file.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal class ImportSymbolTable : SymbolTableBase<ImportSymbolInfo>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private ImportSymbolTable(
            ImmutableArray<KeyValuePair<ISymbol, ImportSymbolInfo>> documentSymbols,
            ImmutableArray<KeyValuePair<ISymbol, ImportSymbolInfo>> directlyReferencedExternalSymbols)
            : base(
                ImmutableArray<KeyValuePair<string, ImportSymbolInfo>>.Empty,
                documentSymbols,
                directlyReferencedExternalSymbols,
                ImmutableArray<KeyValuePair<ISymbol, Lazy<ImportSymbolInfo>>>.Empty)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new <see cref="ImportSymbolTable"/> for the specified translation contexts.
        /// </summary>
        /// <param name="contexts">The contexts from which to retrieve symbols.</param>
        /// <param name="directlyReferencedExternalTypeSymbols">
        /// An array of symbols that are directly referenced in the documents, but are defined in
        /// external assemblies.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A new <see cref="ImportSymbolTable"/>.</returns>
        public static ImportSymbolTable Create(
            ImmutableArray<DocumentTranslationContext> contexts,
            ImmutableArray<ITypeSymbol> directlyReferencedExternalTypeSymbols,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // process the types defined in the documents
            var documentSymbols = contexts.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(context => ProcessSymbolsInDocument(context, cancellationToken))
                .ToImmutableArray();

            // process the externally referenced types
            var directlyReferencedExternalSymbols =
                directlyReferencedExternalTypeSymbols.Select(ProcessExternalType).ToImmutableArray();

            return new ImportSymbolTable(documentSymbols, directlyReferencedExternalSymbols);
        }

        /// <summary>
        /// Processes all of the defined types in the document to the mapping.
        /// </summary>
        private static IEnumerable<KeyValuePair<ISymbol, ImportSymbolInfo>> ProcessSymbolsInDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            var symbolInfo = ImportSymbolInfo.CreateInternalReference(context.TypeScriptFilePath);

            return context.RootSyntax.GetAllDeclaredTypes(context.SemanticModel, cancellationToken)
                .Select(typeSymbol => new KeyValuePair<ISymbol, ImportSymbolInfo>(typeSymbol, symbolInfo));
        }

        /// <summary>
        /// Processes an externally-referenced type.
        /// </summary>
        private static KeyValuePair<ISymbol, ImportSymbolInfo> ProcessExternalType(ITypeSymbol symbol)
        {
            var containingAssembly = symbol.ContainingAssembly;
            string moduleName = containingAssembly.Name;
            var symbolInfo = ImportSymbolInfo.CreateExternalReference(moduleName);
            return new KeyValuePair<ISymbol, ImportSymbolInfo>(symbol, symbolInfo);
        }
    }
}
