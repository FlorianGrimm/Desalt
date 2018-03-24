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
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains static utility methods for using a symbol table.
    /// </summary>
    internal static class SymbolTable
    {
        public static readonly IEqualityComparer<ISymbol> KeyComparer = new KeyEqualityComparer();

        private static readonly SymbolDisplayFormat
            s_symbolDisplayFormat = SymbolDisplayFormat.MinimallyQualifiedFormat;

        public static string KeyFromSymbol(ISymbol symbol) => symbol?.ToDisplayString(s_symbolDisplayFormat);

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
        public bool HasSymbol(ISymbol symbol)
        {
            string symbolName = SymbolTable.KeyFromSymbol(symbol);
            return _symbolMap.TryGetValue(symbolName, out T _);
        }

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        public abstract void AddDefinedTypesInDocument(DocumentTranslationContext context);

        protected T AddOrUpdate(ISymbol symbol, T value)
        {
            string key = SymbolTable.KeyFromSymbol(symbol);
            return _symbolMap.AddOrUpdate(key, _ => value, (_, __) => value);
        }
    }
}
