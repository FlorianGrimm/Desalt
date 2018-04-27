// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Abstract base class for a symbol table that holds different information.
    /// </summary>
    /// <typeparam name="T">The type of information that the symbol table holds.</typeparam>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal abstract class SymbolTable<T>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly ImmutableDictionary<string, T> _documentSymbols;
        private readonly ImmutableDictionary<string, T> _directlyReferencedExternalSymbols;
        private readonly ImmutableDictionary<string, Lazy<T>> _indirectlyReferencedExternalSymbols;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Initializes a new <see cref="SymbolTable{T}"/> with the specified values.
        /// </summary>
        /// <param name="documentSymbols">The symbols defined in the documents.</param>
        /// <param name="directlyReferencedExternalSymbols">
        /// The symbols directly referenced in the documents but residing in external assemblies.
        /// </param>
        /// <param name="indirectlyReferencedExternalSymbols">
        /// Values that will be computed on demand and then cached. This is mostly useful for
        /// externally-referenced types in an assembly that may never be accessed. There is a
        /// performance hit for processing potentially hundreds of values when they may not be used.
        /// </param>
        protected SymbolTable(
            IEnumerable<KeyValuePair<string, T>> documentSymbols,
            IEnumerable<KeyValuePair<string, T>> directlyReferencedExternalSymbols,
            IEnumerable<KeyValuePair<string, Lazy<T>>> indirectlyReferencedExternalSymbols)
        {
            _documentSymbols = documentSymbols?.ToImmutableDictionary() ??
                throw new ArgumentNullException(nameof(documentSymbols));

            _directlyReferencedExternalSymbols = directlyReferencedExternalSymbols?.ToImmutableDictionary() ??
                ImmutableDictionary<string, T>.Empty;

            _indirectlyReferencedExternalSymbols = indirectlyReferencedExternalSymbols?.ToImmutableDictionary() ??
                ImmutableDictionary<string, Lazy<T>>.Empty;
        }

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        public T this[ISymbol symbol]
        {
            get
            {
                string symbolName = SymbolTableUtils.KeyFromSymbol(symbol);

                // look in the document symbols first
                if (!_documentSymbols.TryGetValue(symbolName, out T value))
                {
                    // then in the directly-referenced symbols
                    if (!_directlyReferencedExternalSymbols.TryGetValue(symbolName, out value))
                    {
                        // then in the indirectly-referenced symbols
                        if (!_indirectlyReferencedExternalSymbols.TryGetValue(symbolName, out Lazy<T> lazyValue))
                        {
                            throw new KeyNotFoundException(
                                $"There is no symbol '{symbolName}' defined in the symbol table");
                        }

                        value = lazyValue.Value;
                    }
                }

                return value;
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Gets the symbols defined in the documents that were used to initialize this symbol table.
        /// </summary>
        public IEnumerable<KeyValuePair<string, T>> DocumentSymbols => _documentSymbols;

        /// <summary>
        /// Gets the symbols directly referenced in the documents that were used to initialize this
        /// symbol table.
        /// </summary>
        public IEnumerable<KeyValuePair<string, T>> DirectlyReferencedExternalSymbols => _directlyReferencedExternalSymbols;

        /// <summary>
        /// Gets the symbols defined in externally-referenced assemblies, where their values are
        /// created on demand and then cached.
        /// </summary>
        public IEnumerable<KeyValuePair<string, Lazy<T>>> IndirectlyReferencedExternalSymbols => _indirectlyReferencedExternalSymbols;

        /// <summary>
        /// Returns a value indicating whether the symbol table contains a definition for the
        /// specified symbol name.
        /// </summary>
        /// <param name="symbol">The symbol to look up.</param>
        public bool HasSymbol(ISymbol symbol)
        {
            string symbolName = SymbolTableUtils.KeyFromSymbol(symbol);
            return symbolName != null &&
                (_documentSymbols.TryGetValue(symbolName, out _) ||
                    _directlyReferencedExternalSymbols.TryGetValue(symbolName, out _) ||
                    _indirectlyReferencedExternalSymbols.TryGetValue(symbolName, out _));
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
    }
}
