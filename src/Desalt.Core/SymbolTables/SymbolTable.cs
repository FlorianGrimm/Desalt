// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Abstract base class for a symbol table that holds different information.
    /// </summary>
    /// <typeparam name="T">The type of information that the symbol table holds.</typeparam>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal abstract class SymbolTable<T> where T : class
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Initializes a new <see cref="SymbolTable{T}"/> with the specified values.
        /// </summary>
        /// <param name="overrideSymbols">
        /// An array of overrides that takes precedence over any of the other symbols. This is to
        /// allow creating exceptions without changing the Saltarelle assembly source code. The key
        /// is what is returned from <see cref="SymbolTableUtils.KeyFromSymbol"/>.
        /// </param>
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
            ImmutableArray<KeyValuePair<string, T>> overrideSymbols,
            ImmutableArray<KeyValuePair<ISymbol, T>> documentSymbols,
            ImmutableArray<KeyValuePair<ISymbol, T>> directlyReferencedExternalSymbols,
            ImmutableArray<KeyValuePair<ISymbol, Lazy<T>>> indirectlyReferencedExternalSymbols)
        {
            OverrideSymbols = overrideSymbols.ToImmutableDictionary();
            DocumentSymbols = documentSymbols.ToImmutableDictionary();
            DirectlyReferencedExternalSymbols = directlyReferencedExternalSymbols.ToImmutableDictionary();
            IndirectlyReferencedExternalSymbols = indirectlyReferencedExternalSymbols.ToImmutableDictionary();
        }

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        public T this[ISymbol symbol]
        {
            get
            {
                if (!TryGetValue(symbol, out T value))
                {
                    throw new KeyNotFoundException($"There is no symbol '{symbol}' defined in the symbol table");
                }

                return value;
            }
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the overrides that takes precedence over any of the other symbols. This is to allow
        /// creating exceptions without changing the Saltarelle assembly source code. The key is what
        /// is returned from <see cref="SymbolTableUtils.KeyFromSymbol"/>.
        /// </summary>
        public ImmutableDictionary<string, T> OverrideSymbols { get; }

        /// <summary>
        /// Gets the symbols defined in the documents that were used to initialize this symbol table.
        /// </summary>
        public ImmutableDictionary<ISymbol, T> DocumentSymbols { get; }

        /// <summary>
        /// Gets the symbols directly referenced in the documents that were used to initialize this
        /// symbol table.
        /// </summary>
        public ImmutableDictionary<ISymbol, T> DirectlyReferencedExternalSymbols { get; }

        /// <summary>
        /// Gets the symbols defined in externally-referenced assemblies, where their values are
        /// created on demand and then cached.
        /// </summary>
        public ImmutableDictionary<ISymbol, Lazy<T>> IndirectlyReferencedExternalSymbols { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value indicating whether the symbol table contains a definition for the
        /// specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to look up.</param>
        public bool HasSymbol(ISymbol symbol) => TryGetValue(symbol, out _);

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
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            // look in the overrides first
            string key = SymbolTableUtils.KeyFromSymbol(symbol);
            if (OverrideSymbols.TryGetValue(key, out value))
            {
                return true;
            }

            // then in the document symbols
            if (DocumentSymbols.TryGetValue(symbol, out value))
            {
                return true;
            }

            // then in the directly-referenced symbols
            if (DirectlyReferencedExternalSymbols.TryGetValue(symbol, out value))
            {
                return true;
            }

            // then in the indirectly-referenced symbols
            if (IndirectlyReferencedExternalSymbols.TryGetValue(symbol, out Lazy<T> lazyValue) &&
                lazyValue.Value != null)
            {
                value = lazyValue.Value;
                return true;
            }

            // then try the original definition, which is the generic version of a symbol (for
            // example, if the symbol is a method `Value<int>(int x)`, then the original definition
            // is `Value<T>(T x)`
            if (symbol.OriginalDefinition != null && !ReferenceEquals(symbol.OriginalDefinition, symbol))
            {
                // ReSharper disable once TailRecursiveCall
                return TryGetValue(symbol.OriginalDefinition, out value);
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
