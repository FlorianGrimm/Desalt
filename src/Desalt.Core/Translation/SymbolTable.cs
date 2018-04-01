// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains static utility methods for using a symbol table. Pulled out into a separate class
    /// because statics aren't shared between generic instances.
    /// </summary>
    internal static class SymbolTable
    {
        public static readonly IEqualityComparer<ISymbol> KeyComparer = new KeyEqualityComparer();

        private static readonly SymbolDisplayFormat s_symbolDisplayFormat = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            SymbolDisplayGenericsOptions.IncludeTypeParameters,
            SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters,
            SymbolDisplayDelegateStyle.NameOnly,
            SymbolDisplayExtensionMethodStyle.StaticMethod,
            SymbolDisplayParameterOptions.IncludeType,
            SymbolDisplayPropertyStyle.NameOnly,
            SymbolDisplayLocalOptions.IncludeType,
            SymbolDisplayKindOptions.IncludeNamespaceKeyword |
            SymbolDisplayKindOptions.IncludeTypeKeyword |
            SymbolDisplayKindOptions.IncludeMemberKeyword,
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        public static string KeyFromSymbol(ISymbol symbol) => symbol.ToDisplayString(s_symbolDisplayFormat);

        private sealed class KeyEqualityComparer : IEqualityComparer<ISymbol>
        {
            public bool Equals(ISymbol x, ISymbol y) => KeyFromSymbol(x).Equals(KeyFromSymbol(y));

            public int GetHashCode(ISymbol obj) => KeyFromSymbol(obj).GetHashCode();
        }
    }

    /// <summary>
    /// Interface for a symbol table that holds different information.
    /// </summary>
    internal interface IConcurrentSymbolTable
    {
        /// <summary>
        /// Returns a value indicating whether the symbol table contains a definition for the
        /// specified symbol name.
        /// </summary>
        /// <param name="symbol">The symbol to look up.</param>
        bool HasSymbol(ISymbol symbol);

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        void AddDefinedTypesInDocument(DocumentTranslationContext context, CancellationToken cancellationToken);

        /// <summary>
        /// Adds all of the defined types in external assembly references to the mapping.
        /// </summary>
        void AddExternallyReferencedTypes(DocumentTranslationContext context, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Abstract base class for a symbol table that holds different information.
    /// </summary>
    /// <typeparam name="T">The type of information that the symbol table holds.</typeparam>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal abstract partial class SymbolTable<T> : IConcurrentSymbolTable, IEnumerable<KeyValuePair<string, T>>
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

        /// <summary>
        /// Adds all of the defined types in external assembly references to the mapping.
        /// </summary>
        public void AddExternallyReferencedTypes(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            ExternalTypeWalker walker = new ExternalTypeWalker(context.SemanticModel, cancellationToken);
            walker.Visit(context.RootSyntax);
            IEnumerable<ITypeSymbol> externalTypeSymbols = walker.ExternalTypeSymbols;

            foreach (ITypeSymbol externalTypeSymbol in externalTypeSymbols)
            {
                AddExternallyReferencedType(externalTypeSymbol, context, cancellationToken);
            }
        }

        /// <summary>
        /// Adds a single type defined in an external assembly.
        /// </summary>
        protected abstract void AddExternallyReferencedType(
            ITypeSymbol typeSymbol,
            DocumentTranslationContext context,
            CancellationToken cancellationToken);

        protected T AddOrUpdate(ISymbol symbol, T value)
        {
            string key = SymbolTable.KeyFromSymbol(symbol);
            return _symbolMap.AddOrUpdate(key, _ => value, (_, __) => value);
        }
    }
}
