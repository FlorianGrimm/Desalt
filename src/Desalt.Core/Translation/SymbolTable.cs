// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains static utility methods for using a symbol table.
    /// </summary>
    internal static class SymbolTable
    {
        public static readonly IEqualityComparer<ISymbol> KeyComparer = new KeyEqualityComparer();

        public static string KeyFromSymbol(ISymbol symbol) => symbol.MetadataName;

        private sealed class KeyEqualityComparer : IEqualityComparer<ISymbol>
        {
            public bool Equals(ISymbol x, ISymbol y) => KeyFromSymbol(x).Equals(KeyFromSymbol(y));

            public int GetHashCode(ISymbol obj) => KeyFromSymbol(obj).GetHashCode();
        }
    }

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

        private readonly ConcurrentDictionary<string, T> _symbolMap = new ConcurrentDictionary<string, T>();

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        public T this[ISymbol symbol]
        {
            get
            {
                string symbolName = SymbolTable.KeyFromSymbol(symbol);
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
        /// Returns a value indicating whether the symbol table contains a definition for the
        /// specified symbol name.
        /// </summary>
        /// <param name="symbol">The symbol to look up.</param>
        public bool HasSymbol(ISymbol symbol)
        {
            string symbolName = SymbolTable.KeyFromSymbol(symbol);
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
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        public abstract void AddDefinedTypesInDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken);

        protected T AddOrUpdate(ISymbol symbol, T value)
        {
            string key = SymbolTable.KeyFromSymbol(symbol);
            return _symbolMap.AddOrUpdate(key, _ => value, (_, __) => value);
        }
    }
}
