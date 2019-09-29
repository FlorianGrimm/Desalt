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
    using System.Linq;
    using System.Threading;
    using CompilerUtilities.Extensions;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;

    internal sealed partial class ScriptSymbolTable
    {
        /// <summary>
        /// Creates a new <see cref="ScriptSymbolTable"/> for the specified translation contexts.
        /// </summary>
        /// <param name="contexts">The contexts from which to retrieve symbols.</param>
        /// <param name="scriptNamer">names for the generated script given C# symbols.</param>
        /// <param name="discoveryKind">The kind of discovery to use (mainly for unit tests).</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A new <see cref="ScriptSymbolTable"/>.</returns>
        public static ScriptSymbolTable Create(
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
                .SelectMany(DiscoverTypeAndMembers)

                // Distinct is needed because of nested types, where we might directly reference a class and a nested class
                .Distinct()
                .Select(
                    typeSymbol => new KeyValuePair<ISymbol, IScriptSymbol>(
                        typeSymbol,
                        CreateScriptSymbol(typeSymbol, scriptNamer)))
                .Where(pair => pair.Value != null)
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

            return new ScriptSymbolTable(
                overrides,
                documentSymbols,
                directlyReferencedExternalSymbols,
                indirectlyReferencedSymbols);
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

                        case TypeKind.Enum:
                            return new ScriptEnumSymbol(typeSymbol, computedScriptName);
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
