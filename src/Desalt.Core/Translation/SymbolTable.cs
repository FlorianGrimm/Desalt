// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Desalt.Core.Extensions;
    using Microsoft.CodeAnalysis;

    internal delegate IEnumerable<KeyValuePair<ISymbol, T>> DiscoverSymbolsInDocumentFunc<T>(
        DocumentTranslationContext context,
        CancellationToken cancellationToken);

    internal delegate IEnumerable<KeyValuePair<ISymbol, T>> ProcessExternallyReferencedTypeFunc<T>(
        ITypeSymbol externalType,
        CompilerOptions options,
        CancellationToken cancellationToken);

    /// <summary>
    /// Abstract base class for a symbol table that holds different information.
    /// </summary>
    /// <typeparam name="T">The type of information that the symbol table holds.</typeparam>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal abstract class SymbolTable<T> : IEnumerable<KeyValuePair<string, T>>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly ImmutableDictionary<string, T> _symbolMap;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        protected SymbolTable(IEnumerable<KeyValuePair<string, T>> values)
        {
            _symbolMap = values.ToImmutableDictionary();
        }

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        public T this[ISymbol symbol]
        {
            get
            {
                string symbolName = SymbolTableUtils.KeyFromSymbol(symbol);
                if (!_symbolMap.TryGetValue(symbolName, out T scriptName))
                {
                    throw new KeyNotFoundException($"There is no symbol '{symbolName}' defined in the symbol table");
                }

                return scriptName;
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate
        /// through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => _symbolMap.GetEnumerator();

        /// <summary>
        /// Returns a value indicating whether the symbol table contains a definition for the
        /// specified symbol name.
        /// </summary>
        /// <param name="symbol">The symbol to look up.</param>
        public bool HasSymbol(ISymbol symbol)
        {
            string symbolName = SymbolTableUtils.KeyFromSymbol(symbol);
            return symbolName != null && _symbolMap.TryGetValue(symbolName, out T _);
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="symbol">The symbol to look up.</param>
        /// <param name="value">
        /// The value associated with the symbol if found; otherwise the default value for <c>T</c>.
        /// </param>
        /// <returns>true if the symbol was found; otherwise, false.</returns>
        public bool TryGetValue(ISymbol symbol, out T value)
        {
            if (HasSymbol(symbol))
            {
                value = this[symbol];
                return true;
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key, returning a default value if
        /// not found.
        /// </summary>
        /// <param name="symbol">The symbol to look up.</param>
        /// <param name="defaultIfMissing">The value to return if the symbol was not found.</param>
        /// <returns>Either the found value or the specified default value.</returns>
        public T GetValueOrDefault(ISymbol symbol, T defaultIfMissing)
        {
            if (!TryGetValue(symbol, out T value))
            {
                value = defaultIfMissing;
            }

            return value;
        }

        /// <summary>
        /// Discovers all of the symbols needed for this symbol table.
        /// </summary>
        /// <param name="documentsContexts">The documents to process.</param>
        /// <param name="discoveryKind">The kinds of types to add to the symbol table.</param>
        /// <param name="discoverSymbolsInDocumentFunc">
        /// A function to call for each document to discover the symbols defined in the document to
        /// add to the symbol table.
        /// </param>
        /// <param name="processExternallyReferencedTypeFunc">
        /// An optional function to call for each externally referenced type.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
        protected static IEnumerable<KeyValuePair<string, T>> DiscoverSymbols(
            IEnumerable<DocumentTranslationContext> documentsContexts,
            SymbolTableDiscoveryKind discoveryKind,
            DiscoverSymbolsInDocumentFunc<T> discoverSymbolsInDocumentFunc,
            ProcessExternallyReferencedTypeFunc<T> processExternallyReferencedTypeFunc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (documentsContexts == null)
            {
                throw new ArgumentNullException(nameof(documentsContexts));
            }

            if (discoverSymbolsInDocumentFunc == null)
            {
                throw new ArgumentNullException(nameof(discoverSymbolsInDocumentFunc));
            }

            if (processExternallyReferencedTypeFunc == null)
            {
                throw new ArgumentNullException(nameof(processExternallyReferencedTypeFunc));
            }

            ImmutableArray<DocumentTranslationContext> contexts = documentsContexts.ToImmutableArray();

            // discover all of the symbols in the document in parallel
            IEnumerable<KeyValuePair<ISymbol, T>> documentSymbols = contexts.AsParallel()
                .SelectMany(context => discoverSymbolsInDocumentFunc(context, cancellationToken));

            // discover all of the externally-referenced type symbols in parallel
            var externalReferenceQuery = contexts.AsParallel()
                .SelectMany(
                    context => DiscoverExternallyReferencedTypes(
                        context,
                        processExternallyReferencedTypeFunc,
                        cancellationToken));

            IEnumerable<KeyValuePair<ISymbol, T>> externalSymbols =
                discoveryKind.IsOneOf(
                    SymbolTableDiscoveryKind.DocumentAndReferencedTypes,
                    SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes)
                    ? externalReferenceQuery
                    : Enumerable.Empty<KeyValuePair<ISymbol, T>>();

            var allSymbols = documentSymbols.Concat(externalSymbols)
                .Distinct(SymbolTableUtils.GetKeyValueComparer<T>())
                .Select(pair => new KeyValuePair<string, T>(SymbolTableUtils.KeyFromSymbol(pair.Key), pair.Value));

            return allSymbols;
        }

        /// <summary>
        /// Discovers all of the types that are directly referenced in the document, but live in
        /// external assemblies.
        /// </summary>
        /// <param name="context">The <see cref="DocumentTranslationContext"/> to discover.</param>
        /// <param name="processExternallyReferencedTypeFunc">
        /// An optional function to call for each externally referenced type.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
        /// <returns></returns>
        private static IEnumerable<KeyValuePair<ISymbol, T>> DiscoverExternallyReferencedTypes(
            DocumentTranslationContext context,
            ProcessExternallyReferencedTypeFunc<T> processExternallyReferencedTypeFunc,
            CancellationToken cancellationToken)
        {
            // find all of the external type references in the document
            var walker = new ExternalTypeWalker(context.SemanticModel, cancellationToken);
            walker.Visit(context.RootSyntax);
            ISet<ITypeSymbol> externalTypeSymbols = walker.ExternalTypeSymbols;

            var processedExternalTypes = new List<IEnumerable<KeyValuePair<ISymbol, T>>>();
            foreach (ITypeSymbol externalTypeSymbol in externalTypeSymbols)
            {
                IEnumerable<KeyValuePair<ISymbol, T>> processedExternalType =
                    processExternallyReferencedTypeFunc(externalTypeSymbol, context.Options, cancellationToken);

                processedExternalTypes.Add(processedExternalType);
            }

            return processedExternalTypes.SelectMany(x => x);
        }

        private static ImmutableArray<INamedTypeSymbol> DiscoverAssemblyTypeSymbols(
            IEnumerable<ISymbol> externalSymbols,
            Compilation compilation,
            CancellationToken cancellationToken)
        {
            // get mscorlib
            IAssemblySymbol mscorlib = compilation.GetSpecialType(SpecialType.System_Boolean).ContainingAssembly;

            IEnumerable<IAssemblySymbol> referencedAssemblySymbols = mscorlib.ToSingleEnumerable()
                .Concat(externalSymbols)
                .Select(symbol => symbol.ContainingAssembly)
                .Distinct();

            return referencedAssemblySymbols.AsParallel()
                .SelectMany(
                    assemblySymbol => SymbolTableUtils.GetScriptableTypesInAssembly(assemblySymbol, cancellationToken))
                .WithCancellation(cancellationToken)
                .ToImmutableArray();
        }
    }
}
