// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="NewSymbolTable.cs" company="Justin Rockwood">
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
    using CompilerUtilities.Extensions;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a symbol table holding information about how to translate a C# symbol into TypeScript.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal sealed class NewSymbolTable
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Initializes a new <see cref="NewSymbolTable"/> with the specified values.
        /// </summary>
        ///// <param name="overrideSymbols">
        ///// An array of overrides that takes precedence over any of the other symbols. This is to
        ///// allow creating exceptions without changing the Saltarelle assembly source code.
        ///// </param>
        ///// <param name="documentSymbols">The symbols defined in the documents.</param>
        ///// <param name="directlyReferencedExternalSymbols">
        ///// The symbols directly referenced in the documents but residing in external assemblies.
        ///// </param>
        ///// <param name="indirectlyReferencedExternalSymbols">
        ///// Values that will be computed on demand and then cached. This is mostly useful for
        ///// externally-referenced types in an assembly that may never be accessed. There is a
        ///// performance hit for processing potentially hundreds of values when they may not be used.
        ///// </param>
        private NewSymbolTable(
            ImmutableDictionary<string, SymbolTableOverride> overrideSymbols,
            ImmutableDictionary<ISymbol, IScriptSymbol> documentSymbols,
            ImmutableDictionary<ISymbol, IScriptSymbol> directlyReferencedExternalSymbols,
            ImmutableDictionary<ISymbol, Lazy<IScriptSymbol>> indirectlyReferencedExternalSymbols)
        {
            OverrideSymbols = overrideSymbols;
            DocumentSymbols = documentSymbols;
            DirectlyReferencedExternalSymbols = directlyReferencedExternalSymbols;
            IndirectlyReferencedExternalSymbols = indirectlyReferencedExternalSymbols;
        }

        /// <summary>
        /// Creates a new <see cref="NewSymbolTable"/> for the specified translation contexts.
        /// </summary>
        /// <param name="contexts">The contexts from which to retrieve symbols.</param>
        /// <param name="scriptNamer">names for the generated script given C# symbols.</param>
        /// <param name="discoveryKind">The kind of discovery to use (mainly for unit tests).</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A new <see cref="NewSymbolTable"/>.</returns>
        public static NewSymbolTable Create(
            ImmutableArray<DocumentTranslationContext> contexts,
            IScriptNamer scriptNamer,
            SymbolTableDiscoveryKind discoveryKind = SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // get the symbol table overrides
            ImmutableDictionary<string, SymbolTableOverride> overrides =
                contexts.First().Options.SymbolTableOverrides.Overrides;

            // process the types defined in the documents
            var documentSymbols = contexts.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(context => ProcessSymbolsInDocument(context, scriptNamer, cancellationToken))
                .ToImmutableDictionary();

            // process the externally referenced types
            var directlyReferencedExternalTypeSymbols = SymbolTableUtils.DiscoverDirectlyReferencedExternalTypes(
                contexts,
                discoveryKind,
                cancellationToken);

            var directlyReferencedExternalSymbols = directlyReferencedExternalTypeSymbols
                .SelectMany(typeSymbol => ProcessTypeAndMembers(typeSymbol, scriptNamer))
                .ToImmutableDictionary();

            var indirectlyReferencedExternalTypeSymbols = SymbolTableUtils.DiscoverTypesInReferencedAssemblies(
                directlyReferencedExternalTypeSymbols,
                contexts.FirstOrDefault()?.SemanticModel.Compilation,
                cancellationToken,
                discoveryKind);

            var indirectlyReferencedSymbols = indirectlyReferencedExternalTypeSymbols.Select(
                    typeSymbol => new KeyValuePair<ISymbol, Lazy<IScriptSymbol>>(
                        typeSymbol,
                        new Lazy<IScriptSymbol>(() => CreateScriptSymbol(typeSymbol, scriptNamer), isThreadSafe: true)))
                .ToImmutableDictionary();

            return new NewSymbolTable(
                overrides,
                documentSymbols,
                directlyReferencedExternalSymbols,
                indirectlyReferencedSymbols);
        }

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        public IScriptSymbol this[ISymbol symbol]
        {
            get
            {
                if (!TryGetValue(symbol, out IScriptSymbol value))
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
        public ImmutableDictionary<ISymbol, Lazy<IScriptSymbol>> IndirectlyReferencedExternalSymbols { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value indicating whether the symbol table contains a definition for the
        /// specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to look up.</param>
        public bool HasSymbol(ISymbol symbol) => TryGetValue(symbol, out IScriptSymbol _);

        /// <summary>
        /// Gets the type-specific symbol from the symbol table.
        /// </summary>
        /// <typeparam name="TScriptSymbol">
        /// One of the inherited script symbol types (from <see cref="IScriptSymbol"/>).
        /// </typeparam>
        /// <param name="key">The C# symbol to lookup.</param>
        /// <returns>A <see cref="IScriptSymbol"/> corresponding to the specified key.</returns>
        public TScriptSymbol Get<TScriptSymbol>(ISymbol key)
            where TScriptSymbol : class, IScriptSymbol
        {
            if (!TryGetValue(key, out TScriptSymbol scriptSymbol))
            {
                throw new KeyNotFoundException();
            }

            return scriptSymbol;
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <typeparam name="TScriptSymbol">
        /// One of the inherited script symbol types (from <see cref="IScriptSymbol"/>).
        /// </typeparam>
        /// <param name="symbol">The symbol to look up.</param>
        /// <param name="value">
        /// The value associated with the symbol if found; otherwise null.
        /// </param>
        /// <returns>true if the symbol was found; otherwise, false.</returns>
        public bool TryGetValue<TScriptSymbol>(ISymbol symbol, out TScriptSymbol value)
            where TScriptSymbol : class, IScriptSymbol
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            // initialize the result to null so we can detect if we found the symbol

            // detect if there is a generic version of a symbol (for example, if the symbol is a
            // method `Value<int>(int x)`, then the original definition is `Value<T>(T x)`
            bool hasGenericVersion =
                symbol.OriginalDefinition != null && !ReferenceEquals(symbol.OriginalDefinition, symbol);

            // local function to search the specified dictionary for the symbol first, then the
            // generic version if there is one
            bool TryFindSymbolOrGenericVersion<TValue>(
                ImmutableDictionary<ISymbol, TValue> dictionary,
                out TValue foundValue)
            {
                if (dictionary.TryGetValue(symbol, out foundValue))
                {
                    return true;
                }

                if (hasGenericVersion && dictionary.TryGetValue(symbol.OriginalDefinition, out foundValue))
                {
                    return true;
                }

                return false;
            }

            // look in the document symbols first
            if (!TryFindSymbolOrGenericVersion(DocumentSymbols, out IScriptSymbol scriptSymbol))
            {
                // then in the directly-referenced symbols
                if (!TryFindSymbolOrGenericVersion(DirectlyReferencedExternalSymbols, out scriptSymbol))
                {
                    // then in the indirectly-referenced symbols
                    if (TryFindSymbolOrGenericVersion(
                        IndirectlyReferencedExternalSymbols,
                        out Lazy<IScriptSymbol> lazyValue))
                    {
                        scriptSymbol = lazyValue.Value;
                    }
                }
            }

            // check the overrides to see if there's a defined value
            if (scriptSymbol != null &&
                OverrideSymbols.TryGetValue(symbol.ToHashDisplay(), out SymbolTableOverride @override) ||
                (hasGenericVersion &&
                    OverrideSymbols.TryGetValue(symbol.OriginalDefinition.ToHashDisplay(), out @override)))
            {
                if (@override.InlineCode != null && scriptSymbol is IScriptMethodSymbol scriptMethodSymbol)
                {
                    scriptSymbol = scriptMethodSymbol.WithInlineCode(@override.InlineCode);
                }
            }

            value = scriptSymbol as TScriptSymbol;
            return value != null;
        }

        private static ImmutableDictionary<ISymbol, IScriptSymbol> ProcessSymbolsInDocument(
            DocumentTranslationContext context,
            IScriptNamer scriptNamer,
            CancellationToken cancellationToken)
        {
            var dictionary = context.RootSyntax.GetAllDeclaredTypes(context.SemanticModel, cancellationToken)
                .SelectMany(typeSymbol => ProcessTypeAndMembers(typeSymbol, scriptNamer))
                .ToImmutableDictionary();

            return dictionary;
        }

        private static IEnumerable<ISymbol> DiscoverTypeAndMembers(INamespaceOrTypeSymbol typeSymbol) =>
            typeSymbol.ToSingleEnumerable().Concat(typeSymbol.GetMembers().Where(ShouldProcessMember));

        private static IEnumerable<KeyValuePair<ISymbol, IScriptSymbol>> ProcessTypeAndMembers(
            ITypeSymbol typeSymbol,
            IScriptNamer scriptNamer)
        {
            return from symbol in DiscoverTypeAndMembers(typeSymbol)
                   let scriptSymbol = CreateScriptSymbol(symbol, scriptNamer)
                   where scriptSymbol != null
                   select new KeyValuePair<ISymbol, IScriptSymbol>(symbol, scriptSymbol);
        }

        private static IScriptSymbol CreateScriptSymbol(ISymbol symbol, IScriptNamer scriptNamer)
        {
            string computedScriptName = scriptNamer.DetermineScriptNameForSymbol(symbol);

            switch (symbol)
            {
                case INamedTypeSymbol typeSymbol:
                    switch (typeSymbol.TypeKind)
                    {
                        case TypeKind.Class:
                        case TypeKind.Interface:
                            return new ScriptTypeSymbol(typeSymbol, computedScriptName);

                        case TypeKind.Struct:
                            return new ScriptStructSymbol(typeSymbol, computedScriptName);

                        case TypeKind.Delegate:
                            return new ScriptDelegateSymbol(typeSymbol, computedScriptName);
                    }

                    break;

                case IFieldSymbol fieldSymbol:
                    return new ScriptFieldSymbol(fieldSymbol, computedScriptName);

                case IPropertySymbol propertySymbol:
                    return new ScriptPropertySymbol(propertySymbol, computedScriptName);

                case IMethodSymbol methodSymbol:
                    return new ScriptMethodSymbol(methodSymbol, computedScriptName);
            }

            return null;
        }

        private static bool ShouldProcessMember(ISymbol member)
        {
            // skip over compiler-generated stuff like auto-property backing fields, event add/remove
            // functions, and property get/set methods
            if (member.IsImplicitlyDeclared)
            {
                return false;
            }

            // don't skip non-method members
            if (member.Kind != SymbolKind.Method || !(member is IMethodSymbol methodSymbol))
            {
                return true;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (methodSymbol.MethodKind)
            {
                // skip static constructors
                case MethodKind.StaticConstructor:
                    return false;

                // skip property get/set methods
                case MethodKind.PropertyGet:
                case MethodKind.PropertySet:
                    return false;

                // skip event add/remove
                case MethodKind.EventAdd:
                case MethodKind.EventRemove:
                    return false;
            }

            return true;
        }
    }
}
