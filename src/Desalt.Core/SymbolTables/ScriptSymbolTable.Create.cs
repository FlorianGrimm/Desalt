// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptSymbolTable.Create.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Options;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;

    internal sealed partial class ScriptSymbolTable
    {
        /// <summary>
        /// Creates a new <see cref="ScriptSymbolTable"/> for the specified translation contexts.
        /// </summary>
        /// <param name="contexts">The contexts from which to retrieve symbols.</param>
        /// <param name="scriptNamer">Determines names for the generated script given C# symbols.</param>
        /// <param name="discoveryKind">The kind of discovery to use (mainly for unit tests).</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A new <see cref="ScriptSymbolTable"/>.</returns>
        public static ScriptSymbolTable Create(
            ImmutableArray<DocumentTranslationContext> contexts,
            IScriptNamer scriptNamer,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.DocumentAndAllAssemblyTypes,
            CancellationToken cancellationToken = default)
        {
            // get the symbol table overrides using the options from the first context - they should all be identical
            CompilerOptions options = contexts.First().Options;
            Debug.Assert(contexts.All(context => ReferenceEquals(context.Options, options)));
            ImmutableDictionary<string, SymbolTableOverride> overrides = options.SymbolTableOverrides.Overrides;

            // discover the externally referenced types
            ImmutableArray<ITypeSymbol> directlyReferencedExternalTypeSymbols = SymbolDiscoverer.DiscoverDirectlyReferencedExternalTypes(
                contexts,
                discoveryKind,
                cancellationToken);

            ImmutableArray<INamedTypeSymbol> indirectlyReferencedExternalTypeSymbols = SymbolDiscoverer.DiscoverTypesInReferencedAssemblies(
                directlyReferencedExternalTypeSymbols,
                contexts.First().SemanticModel.Compilation,
                cancellationToken,
                discoveryKind);

            // create all of the import information for the type symbols
            ImmutableDictionary<ITypeSymbol, ImportSymbolInfo> importSymbols = DiscoverImportSymbols(
                contexts,
                directlyReferencedExternalTypeSymbols,
                indirectlyReferencedExternalTypeSymbols.CastArray<ITypeSymbol>(),
                cancellationToken);

            // process the types defined in the documents
            var documentSymbols = contexts.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(context => ProcessSymbolsInDocument(context, scriptNamer, importSymbols, cancellationToken))
                .ToImmutableDictionary();

            // We need to use Distinct() in the queries below because of nested types. For example,
            // we might directly reference `MyClass` and `MyClass.Nested` so when
            // DiscoverTypeAndMembers is invoked, `MyClass.Nested` will be duplicated.

            // process symbols that are directly referenced by code in the documents
            var directlyReferencedExternalSymbols = directlyReferencedExternalTypeSymbols
                .SelectMany(DiscoverTypeAndMembers)
                .Distinct()
                .Select(
                    typeSymbol => new KeyValuePair<ISymbol, IScriptSymbol?>(
                        typeSymbol,
                        CreateScriptSymbol(typeSymbol, scriptNamer, importSymbols)!))
                .WhereValueNotNull()
                .ToImmutableDictionary();

            // process all of the rest of the symbols in all of the referenced assemblies, but
            // calculate their properties lazily on first access since the vast majority of them
            // won't be needed
            var indirectlyReferencedSymbols = indirectlyReferencedExternalTypeSymbols
                .SelectMany(DiscoverTypeAndMembers)
                .Distinct()
                .Select(
                    typeSymbol => new KeyValuePair<ISymbol, Lazy<IScriptSymbol?>>(
                        typeSymbol,
                        new Lazy<IScriptSymbol?>(
                            () => CreateScriptSymbol(typeSymbol, scriptNamer, importSymbols),
                            isThreadSafe: true)))
                .ToImmutableDictionary();

            return new ScriptSymbolTable(
                overrides,
                documentSymbols,
                directlyReferencedExternalSymbols,
                indirectlyReferencedSymbols);
        }

        /// <summary>
        /// Discovers all of the import information for each symbol used in the document or directly
        /// referenced from the document. This information will be handy when translating the
        /// 'import' statements at the top of the compiled TypeScript files.
        /// </summary>
        private static ImmutableDictionary<ITypeSymbol, ImportSymbolInfo> DiscoverImportSymbols(
            ImmutableArray<DocumentTranslationContext> contexts,
            ImmutableArray<ITypeSymbol> directlyReferencedExternalTypeSymbols,
            ImmutableArray<ITypeSymbol> indirectlyReferencedSymbols,
            CancellationToken cancellationToken)
        {
            var documentImports = contexts.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(context => CreateDocumentImportMappings(context, cancellationToken))
                .ToImmutableDictionary();

            IEnumerable<KeyValuePair<ITypeSymbol, ImportSymbolInfo>> externalImports = CreateExternalTypesImportMappings(
                directlyReferencedExternalTypeSymbols.Concat(indirectlyReferencedSymbols).Distinct());

            var importSymbols = ImmutableDictionary.CreateRange(documentImports.Concat(externalImports));
            return importSymbols;
        }

        /// <summary>
        /// Creates associated <see cref="ImportSymbolInfo"/> for all of the defined types in the document.
        /// </summary>
        private static IEnumerable<KeyValuePair<ITypeSymbol, ImportSymbolInfo>> CreateDocumentImportMappings(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            var symbolInfo = ImportSymbolInfo.CreateInternalReference(context.TypeScriptFilePath);

            return context.RootSyntax.GetAllDeclaredTypes(context.SemanticModel, cancellationToken)
                .Select(typeSymbol => new KeyValuePair<ITypeSymbol, ImportSymbolInfo>(typeSymbol, symbolInfo));
        }

        /// <summary>
        /// Creates an associated <see cref="ImportSymbolInfo"/> for the specified externally-referenced types.
        /// </summary>
        private static IEnumerable<KeyValuePair<ITypeSymbol, ImportSymbolInfo>> CreateExternalTypesImportMappings(
            IEnumerable<ITypeSymbol> externallyReferencedTypes)
        {
            return externallyReferencedTypes.Select(
                typeSymbol => new KeyValuePair<ITypeSymbol, ImportSymbolInfo>(
                    typeSymbol,
                    CreateExternalTypeImport(typeSymbol)));
        }

        /// <summary>
        /// Creates an associated <see cref="ImportSymbolInfo"/> for the specified externally-referenced types.
        /// </summary>
        private static ImportSymbolInfo CreateExternalTypeImport(ITypeSymbol symbol)
        {
            IAssemblySymbol containingAssembly = symbol.ContainingAssembly;
            string moduleName = containingAssembly.Name;
            var symbolInfo = ImportSymbolInfo.CreateExternalReference(moduleName);
            return symbolInfo;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scriptNamer"></param>
        /// <param name="importSymbols"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static ImmutableDictionary<ISymbol, IScriptSymbol> ProcessSymbolsInDocument(
            DocumentTranslationContext context,
            IScriptNamer scriptNamer,
            IReadOnlyDictionary<ITypeSymbol, ImportSymbolInfo> importSymbols,
            CancellationToken cancellationToken)
        {
            var dictionary = context.RootSyntax.GetAllDeclaredTypes(context.SemanticModel, cancellationToken)
                .SelectMany(typeSymbol => ProcessTypeAndMembers(typeSymbol, scriptNamer, importSymbols))
                .ToImmutableDictionary();

            return dictionary;
        }

        /// <summary>
        /// Returns an enumerable of the specified type and all of its members that should be processed.
        /// </summary>
        private static IEnumerable<ISymbol> DiscoverTypeAndMembers(INamespaceOrTypeSymbol typeSymbol)
        {
            return typeSymbol.ToSingleEnumerable().Concat(typeSymbol.GetMembers().Where(ShouldProcessMember));
        }

        private static IEnumerable<KeyValuePair<ISymbol, IScriptSymbol>> ProcessTypeAndMembers(
            ITypeSymbol typeSymbol,
            IScriptNamer scriptNamer,
            IReadOnlyDictionary<ITypeSymbol, ImportSymbolInfo> importSymbols)
        {
            return from symbol in DiscoverTypeAndMembers(typeSymbol)
                   let scriptSymbol = CreateScriptSymbol(symbol, scriptNamer, importSymbols)
                   where scriptSymbol != null
                   select new KeyValuePair<ISymbol, IScriptSymbol>(symbol, scriptSymbol);
        }

        /// <summary>
        /// Factory method for creating an instance if a <see cref="IScriptSymbol"/> from the
        /// specified Roslyn <see cref="ISymbol"/>.
        /// </summary>
        private static IScriptSymbol? CreateScriptSymbol(
            ISymbol symbol,
            IScriptNamer scriptNamer,
            IReadOnlyDictionary<ITypeSymbol, ImportSymbolInfo> importSymbols)
        {
            string computedScriptName = scriptNamer.DetermineScriptNameForSymbol(symbol);

            switch (symbol)
            {
                case INamedTypeSymbol typeSymbol:
                    if (!importSymbols.TryGetValue(typeSymbol, out ImportSymbolInfo importInfo))
                    {
                        throw new InvalidOperationException(
                            $"Could not find an instance of ImportSymbolInfo for symbol '{typeSymbol.ToHashDisplay()}'");
                    }

                    switch (typeSymbol.TypeKind)
                    {
                        case TypeKind.Class:
                        case TypeKind.Interface:

                            return new ScriptTypeSymbol(typeSymbol, computedScriptName, importInfo);

                        case TypeKind.Struct:
                            return new ScriptStructSymbol(typeSymbol, computedScriptName, importInfo);

                        case TypeKind.Delegate:
                            return new ScriptDelegateSymbol(typeSymbol, computedScriptName);

                        case TypeKind.Enum:
                            return new ScriptEnumSymbol(typeSymbol, computedScriptName, importInfo);
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
            // Skip over compiler-generated stuff like auto-property backing fields, event add/remove
            // functions, and property get/set methods.
            if (member.IsImplicitlyDeclared)
            {
                return false;
            }

            // Include all methods except static constructors, property get/set methods, and event
            // add/remove methods.
            if (member is IMethodSymbol methodSymbol)
            {
                return !methodSymbol.MethodKind.IsOneOf(
                    MethodKind.StaticConstructor,
                    MethodKind.PropertyGet,
                    MethodKind.PropertySet,
                    MethodKind.EventAdd,
                    MethodKind.EventRemove);
            }

            // There's an apparent corruption in the NativeTypeDefs.TypeUtil assembly and this
            // compiler-generated symbol is not valid - just skip it.
            if (member.ToDisplayString() == "System.TypeUtil.<>o__3")
            {
                return false;
            }

            return true;
        }
    }
}
