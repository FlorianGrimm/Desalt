// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportSymbolTable.cs" company="Justin Rockwood">
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
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains mappings of defined types in a C# project to the files in which they're defined.
    /// This is needed in order to correctly generate <c>import</c> statements at the top of each
    /// translated TypeScript file.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal class ImportSymbolTable : SymbolTable<ImportSymbolInfo>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private ImportSymbolTable(
            ImmutableArray<KeyValuePair<string, ImportSymbolInfo>> documentSymbols,
            ImmutableArray<KeyValuePair<string, ImportSymbolInfo>> directlyReferencedExternalSymbols)
            : base(
                documentSymbols,
                directlyReferencedExternalSymbols,
                ImmutableArray<KeyValuePair<string, Lazy<ImportSymbolInfo>>>.Empty)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ImportSymbolTable Create(
            IEnumerable<DocumentTranslationContext> documentsContexts,
            SymbolTableDiscoveryKind discoveryKind,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ImmutableArray<DocumentTranslationContext> contexts = documentsContexts.ToImmutableArray();

            // process the types defined in the documents
            ImmutableArray<KeyValuePair<string, ImportSymbolInfo>> documentSymbols = contexts.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(context => ProcessSymbolsInDocument(context, cancellationToken))
                .ToImmutableArray();

            if (discoveryKind == SymbolTableDiscoveryKind.OnlyDocumentTypes)
            {
                return new ImportSymbolTable(
                    documentSymbols,
                    ImmutableArray<KeyValuePair<string, ImportSymbolInfo>>.Empty);
            }

            ImmutableArray<KeyValuePair<string, ImportSymbolInfo>> directlyReferencedSymbols = contexts
                .SelectMany(
                    context => SymbolTableUtils.DiscoverDirectlyReferencedExternalTypes(context, cancellationToken))
                .Distinct()
                .Select(ProcessExternallyReferencedType)
                .ToImmutableArray();

            return new ImportSymbolTable(documentSymbols, directlyReferencedSymbols);
        }

        /// <summary>
        /// Processes all of the defined types in the document to the mapping.
        /// </summary>
        private static IEnumerable<KeyValuePair<string, ImportSymbolInfo>> ProcessSymbolsInDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            ImportSymbolInfo symbolInfo = ImportSymbolInfo.CreateInternalReference(context.TypeScriptFilePath);

            return context.RootSyntax.GetAllDeclaredTypes(context.SemanticModel, cancellationToken)
                .Select(
                    typeSymbol => new KeyValuePair<string, ImportSymbolInfo>(
                        SymbolTableUtils.KeyFromSymbol(typeSymbol),
                        symbolInfo));
        }

        /// <summary>
        /// Processes an externally-referenced type.
        /// </summary>
        private static KeyValuePair<string, ImportSymbolInfo> ProcessExternallyReferencedType(ISymbol symbol)
        {
            var containingAssembly = symbol.ContainingAssembly;
            string moduleName = containingAssembly.Name;
            var symbolInfo = ImportSymbolInfo.CreateExternalReference(moduleName);
            return new KeyValuePair<string, ImportSymbolInfo>(SymbolTableUtils.KeyFromSymbol(symbol), symbolInfo);
        }
    }

    /// <summary>
    /// Represents an imported symbol that is contained in an <see cref="ImportSymbolTable"/>.
    /// </summary>
    internal class ImportSymbolInfo
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private ImportSymbolInfo(string relativeTypeScriptFilePathOrModuleName, bool isInternalReference)
        {
            RelativeTypeScriptFilePathOrModuleName = relativeTypeScriptFilePathOrModuleName;
            IsInternalReference = isInternalReference;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string RelativeTypeScriptFilePathOrModuleName { get; }

        /// <summary>
        /// Returns a value indicating whether this is an internal reference, meaning that the type
        /// is defined within this project. An external reference is something from another assembly.
        /// </summary>
        public bool IsInternalReference { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ImportSymbolInfo CreateInternalReference(string typeScriptFilePath)
        {
            return new ImportSymbolInfo(typeScriptFilePath, isInternalReference: true);
        }

        public static ImportSymbolInfo CreateExternalReference(string moduleName)
        {
            return new ImportSymbolInfo(moduleName, isInternalReference: false);
        }
    }
}
