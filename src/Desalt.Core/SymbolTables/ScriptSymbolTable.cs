// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using Desalt.Core.Options;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a symbol table holding information about how to translate a C# symbol into TypeScript.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal sealed partial class ScriptSymbolTable
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Initializes a new <see cref="ScriptSymbolTable"/> with the specified values.
        /// </summary>
        /// <param name="overrideSymbols">
        /// An array of overrides that takes precedence over any of the other symbols. This is to
        /// allow creating exceptions without changing the Saltarelle assembly source code.
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
        private ScriptSymbolTable(
            ImmutableDictionary<string, SymbolTableOverride> overrideSymbols,
            ImmutableDictionary<ISymbol, IScriptSymbol> documentSymbols,
            ImmutableDictionary<ISymbol, IScriptSymbol> directlyReferencedExternalSymbols,
            ImmutableDictionary<ISymbol, Lazy<IScriptSymbol?>> indirectlyReferencedExternalSymbols)
        {
            OverrideSymbols = overrideSymbols;
            DocumentSymbols = documentSymbols;
            DirectlyReferencedExternalSymbols = directlyReferencedExternalSymbols;
            IndirectlyReferencedExternalSymbols = indirectlyReferencedExternalSymbols;
        }

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        public IScriptSymbol this[ISymbol symbol]
        {
            get
            {
                if (!TryGetValue(symbol, out IScriptSymbol? value))
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
        /// creating exceptions without changing the Saltarelle assembly source code.
        /// </summary>
        public ImmutableDictionary<string, SymbolTableOverride> OverrideSymbols { get; }

        /// <summary>
        /// Gets the symbols defined in the documents that were used to initialize this symbol table.
        /// </summary>
        public ImmutableDictionary<ISymbol, IScriptSymbol> DocumentSymbols { get; }

        /// <summary>
        /// Gets the symbols directly referenced in the documents that were used to initialize this
        /// symbol table.
        /// </summary>
        public ImmutableDictionary<ISymbol, IScriptSymbol> DirectlyReferencedExternalSymbols { get; }

        /// <summary>
        /// Gets the symbols defined in externally-referenced assemblies, where their values are
        /// created on demand and then cached.
        /// </summary>
        public ImmutableDictionary<ISymbol, Lazy<IScriptSymbol?>> IndirectlyReferencedExternalSymbols { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value indicating whether the symbol table contains a definition for the
        /// specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to look up.</param>
        public bool HasSymbol(ISymbol symbol)
        {
            return TryGetValue(symbol, out IScriptSymbol _);
        }

        /// <summary>
        /// Gets the computed script name corresponding to the specified symbol, or the default value
        /// if the symbol is not in the symbol table.
        /// </summary>
        /// <param name="symbol">The symbol to look up.</param>
        /// <param name="defaultValue">The default value to use if the symbol is not present.</param>
        /// <returns>Either the symbol's computed script name or the default value.</returns>
        public string GetComputedScriptNameOrDefault(ISymbol symbol, string defaultValue)
        {
            return TryGetValue(symbol, out IScriptSymbol? scriptSymbol) ? scriptSymbol.ComputedScriptName : defaultValue;
        }

        /// <summary>
        /// Gets the type-specific symbol from the symbol table.
        /// </summary>
        /// <typeparam name="TScriptSymbol">
        /// One of the inherited script symbol types (from <see cref="IScriptSymbol"/>).
        /// </typeparam>
        /// <param name="symbol">The C# symbol to lookup.</param>
        /// <returns>A <see cref="IScriptSymbol"/> corresponding to the specified symbol.</returns>
        public TScriptSymbol Get<TScriptSymbol>(ISymbol symbol)
            where TScriptSymbol : class, IScriptSymbol
        {
            if (!TryGetValue(symbol, out TScriptSymbol? scriptSymbol))
            {
                throw new KeyNotFoundException();
            }

            return scriptSymbol;
        }

        /// <summary>
        /// Gets the <see cref="IScriptMethodSymbol"/> corresponding to the specified <see cref="IMethodSymbol"/>.
        /// </summary>
        /// <param name="symbol">The C# symbol to lookup.</param>
        /// <returns>A <see cref="IScriptMethodSymbol"/> corresponding to the specified symbol.</returns>
        public IScriptMethodSymbol Get(IMethodSymbol symbol)
        {
            return Get<IScriptMethodSymbol>(symbol);
        }

        /// <summary>
        /// Attempts to get the value associated with the specified symbol.
        /// </summary>
        /// <typeparam name="TScriptSymbol">
        /// One of the inherited script symbol types (from <see cref="IScriptSymbol"/>).
        /// </typeparam>
        /// <param name="symbol">The symbol to look up.</param>
        /// <param name="value">
        /// The value associated with the symbol if found; otherwise null.
        /// </param>
        /// <returns>true if the symbol was found; otherwise, false.</returns>
        public bool TryGetValue<TScriptSymbol>(ISymbol symbol, [NotNullWhen(true)] out TScriptSymbol? value)
            where TScriptSymbol : class, IScriptSymbol
        {
            _ = symbol ?? throw new ArgumentNullException(nameof(symbol));

            // Look in the document symbols first.
            if (!TryFindSymbol(
                symbol,
                DocumentSymbols,
                out ISymbol symbolUsedForLookup,
                out IScriptSymbol? scriptSymbol))
            {
                // Then in the directly-referenced symbols.
                if (!TryFindSymbol(
                    symbol,
                    DirectlyReferencedExternalSymbols,
                    out symbolUsedForLookup,
                    out scriptSymbol))
                {
                    // And finally in the indirectly-referenced symbols.
                    if (TryFindSymbol(
                        symbol,
                        IndirectlyReferencedExternalSymbols,
                        out symbolUsedForLookup,
                        out Lazy<IScriptSymbol?> lazyValue))
                    {
                        scriptSymbol = lazyValue.Value;
                    }
                }
            }

            // Check the overrides to see if there's a defined value.
            if (scriptSymbol != null &&
                OverrideSymbols.TryGetValue(
                    symbolUsedForLookup.ToHashDisplay(),
                    out SymbolTableOverride? symbolTableOverride))
            {
                if (symbolTableOverride.InlineCode != null && scriptSymbol is IScriptMethodSymbol scriptMethodSymbol)
                {
                    scriptSymbol = scriptMethodSymbol.WithInlineCode(symbolTableOverride.InlineCode);
                }
            }

            value = scriptSymbol as TScriptSymbol;
            return value != null;
        }

        /// <summary>
        /// Checks the specified dictionary for the symbol, by checking the symbol, its original definition (if it's
        /// generic), or its non-reduced form (for extension methods).
        /// </summary>
        /// <typeparam name="TValue">
        /// The value of the item in the dictionary (usually a <see cref="IScriptSymbol"/> or <see cref="Lazy{IScriptSymbol}"/>).
        /// </typeparam>
        /// <param name="symbol">The <see cref="ISymbol"/> to query.</param>
        /// <param name="dictionary">The <see cref="ImmutableDictionary{ISymbol,TValue}"/> to search.</param>
        /// <param name="symbolUsedForLookup">
        /// Contains the symbol that was used for the lookup, which will either be the specified symbol, its original
        /// definition, or its non-reduced form, depending on which was found in the symbol table.
        /// </param>
        /// <param name="foundValue">
        /// Contains the found value if the method returns true. If the method returns false, this is undefined.
        /// </param>
        /// <returns>True if the symbol (or one of its derivatives) was found; otherwise, false.</returns>
        private static bool TryFindSymbol<TValue>(
            ISymbol symbol,
            ImmutableDictionary<ISymbol, TValue> dictionary,
            out ISymbol symbolUsedForLookup,
            out TValue foundValue)
        {
            if (dictionary.TryGetValue(symbol, out foundValue))
            {
                symbolUsedForLookup = symbol;
                return true;
            }

            // Detect if there is a generic version of a symbol (for example, if the symbol is a
            // method `Value<int>(int x)`, then the original definition is `Value<T>(T x)`.
            if (HasGenericVersion(symbol, out ISymbol? originalDefinition) &&
                dictionary.TryGetValue(originalDefinition, out foundValue))
            {
                symbolUsedForLookup = originalDefinition;
                return true;
            }

            // Extension methods can be reduced during translation. For example, `ExtensionMethod(this object x)`
            // invoked as `x.ExtensionMethod()` will have the reduced symbol `System.Object.ExtensionMethod()`. We need
            // to detect this case and look for the non-reduced form in the symbol table.
            if (IsReducedMethod(symbol, out IMethodSymbol? nonReducedMethodSymbol) &&
                dictionary.TryGetValue(nonReducedMethodSymbol, out foundValue))
            {
                symbolUsedForLookup = nonReducedMethodSymbol;
                return true;
            }

            symbolUsedForLookup = symbol;
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether there is a generic version of a symbol. For example, if the symbol is a
        /// method `Value{int}(int x)`, then the original definition is `Value{T}(T x)`.
        /// </summary>
        /// <param name="symbol">The <see cref="ISymbol"/> to query.</param>
        /// <param name="originalDefinition">
        /// If the method returns true, contains the original definition of the symbol. If the method returns false,
        /// this is null.
        /// </param>
        /// <returns>True if the symbol has a generic version; otherwise, false.</returns>
        private static bool HasGenericVersion(ISymbol symbol, [NotNullWhen(true)] out ISymbol? originalDefinition)
        {
            originalDefinition = symbol.OriginalDefinition;
            return originalDefinition != null &&
                !SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition, symbol);
        }

        /// <summary>
        /// Returns a value indicating whether the specified symbol is a reduced method. Extension methods can be
        /// reduced during translation. For example, `ExtensionMethod(this object x)` invoked as `x.ExtensionMethod()`
        /// will have the reduced symbol `System.Object.ExtensionMethod()`. We need to detect this case and look for the
        /// non-reduced form in the symbol table.
        /// </summary>
        /// <param name="symbol">The <see cref="ISymbol"/> to query.</param>
        /// <param name="nonReducedMethodSymbol">
        /// If the method returns true, contains the non-reduced method symbol. If the method returns false, this is null.
        /// </param>
        /// <returns>True if the symbol is a reduced method symbol; otherwise, false.</returns>
        private static bool IsReducedMethod(
            ISymbol symbol,
            [NotNullWhen(true)] out IMethodSymbol? nonReducedMethodSymbol)
        {
            nonReducedMethodSymbol = (symbol as IMethodSymbol)?.ReducedFrom;
            return nonReducedMethodSymbol != null;
        }
    }
}
