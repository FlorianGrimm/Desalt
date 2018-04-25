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
    using System.Threading.Tasks;
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
        protected static async Task<IEnumerable<KeyValuePair<string, T>>> DiscoverSymbolsAsync(
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

            DocumentTranslationContext[] contexts = documentsContexts.ToArray();
            var documentSymbols = new IEnumerable<KeyValuePair<ISymbol, T>>[contexts.Length];
            var externalSymbols = new IEnumerable<KeyValuePair<ISymbol, T>>[contexts.Length];
            var tasks = new Task[contexts.Length];

            // process each document in parallel
            for (int i = 0; i < contexts.Length; i++)
            {
                // define a local variable for the index so the closure works properly
                int i1 = i;
                tasks[i] = Task.Run(
                    () =>
                    {
                        var context = contexts[i1];
                        var symbols = DiscoverSymbolsInDocument(
                            context,
                            discoveryKind,
                            discoverSymbolsInDocumentFunc,
                            processExternallyReferencedTypeFunc,
                            cancellationToken);
                        documentSymbols[i1] = symbols.documentSymbols;
                        externalSymbols[i1] = symbols.externalSymbols;
                    },
                    cancellationToken);
            }

            await Task.WhenAll(tasks);

            var allSymbols = documentSymbols.Concat(externalSymbols.Where(x => x != null))
                .SelectMany(x => x)
                .Distinct(SymbolTableUtils.GetKeyValueComparer<T>())
                .Select(pair => new KeyValuePair<string, T>(SymbolTableUtils.KeyFromSymbol(pair.Key), pair.Value));

            return allSymbols;
        }

        private static (IEnumerable<KeyValuePair<ISymbol, T>> documentSymbols, IEnumerable<KeyValuePair<ISymbol, T>>
            externalSymbols) DiscoverSymbolsInDocument(
                DocumentTranslationContext context,
                SymbolTableDiscoveryKind discoveryKind,
                DiscoverSymbolsInDocumentFunc<T> discoverSymbolsInDocumentFunc,
                ProcessExternallyReferencedTypeFunc<T> processExternallyReferencedTypeFunc,
                CancellationToken cancellationToken)
        {
            IEnumerable<KeyValuePair<ISymbol, T>> documentSymbols =
                discoverSymbolsInDocumentFunc(context, cancellationToken);
            IEnumerable<KeyValuePair<ISymbol, T>> externalSymbols = Enumerable.Empty<KeyValuePair<ISymbol, T>>();

            // we're done if we just need the document types
            if (discoveryKind == SymbolTableDiscoveryKind.OnlyDocumentTypes)
            {
                return (documentSymbols, externalSymbols);
            }

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

            externalSymbols = processedExternalTypes.SelectMany(x => x);

            return (documentSymbols, externalSymbols);
        }
    }
}
