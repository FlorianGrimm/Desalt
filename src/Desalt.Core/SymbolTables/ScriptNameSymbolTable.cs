// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNameSymbolTable.cs" company="Justin Rockwood">
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
    /// Contains mappings of defined types in a C# project to the translated names (what they will be
    /// called in the TypeScript file). By default, Saltarelle converts type members to `camelCase`
    /// names. It can also be changed using the [PreserveName] and [ScriptName] attributes.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal class ScriptNameSymbolTable : SymbolTableBase<string>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private ScriptNameSymbolTable(
            ImmutableArray<KeyValuePair<string, string>> overrideSymbols,
            ImmutableArray<KeyValuePair<ISymbol, string>> documentSymbols,
            ImmutableArray<KeyValuePair<ISymbol, string>> directlyReferencedExternalSymbols,
            ImmutableArray<KeyValuePair<ISymbol, Lazy<string>>> indirectlyReferencedExternalSymbols)
            : base(
                overrideSymbols,
                documentSymbols,
                directlyReferencedExternalSymbols,
                indirectlyReferencedExternalSymbols)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new <see cref="ScriptNameSymbolTable"/> for the specified translation contexts.
        /// </summary>
        /// <param name="contexts">The contexts from which to retrieve symbols.</param>
        /// <param name="directlyReferencedExternalTypeSymbols">
        /// An array of symbols that are directly referenced in the documents, but are defined in
        /// external assemblies.
        /// </param>
        /// <param name="indirectlyReferencedExternalTypeSymbols">
        /// An array of symbols that are not directly referenced in the documents and are defined in
        /// external assemblies.
        /// </param>
        /// <param name="overrideSymbols">
        /// An array of overrides that takes precedence over any of the other symbols. This is to
        /// allow creating exceptions without changing the Saltarelle assembly source code. The key
        /// is what is returned from <see cref="RoslynExtensions.ToHashDisplay"/>.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A new <see cref="ScriptNameSymbolTable"/>.</returns>
        public static ScriptNameSymbolTable Create(
            ImmutableArray<DocumentTranslationContext> contexts,
            ImmutableArray<ITypeSymbol> directlyReferencedExternalTypeSymbols,
            ImmutableArray<INamedTypeSymbol> indirectlyReferencedExternalTypeSymbols,
            IEnumerable<KeyValuePair<string, string>> overrideSymbols = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IAssemblySymbol mscorlibAssemblySymbol =
                SymbolTableUtils.GetMscorlibAssemblySymbol(contexts.First().SemanticModel.Compilation);

            RenameRules renameRules = contexts.FirstOrDefault()?.Options.RenameRules ?? RenameRules.Default;
            var scriptNamer = new ScriptNamer(mscorlibAssemblySymbol, renameRules);

            // process the types defined in the documents
            var documentSymbols = contexts.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(context => ProcessSymbolsInDocument(context, scriptNamer, cancellationToken))
                .ToImmutableArray();

            // process the externally referenced types
            var directlyReferencedExternalSymbols = directlyReferencedExternalTypeSymbols
                .SelectMany(symbol => DiscoverScriptNameOnTypeAndMembers(symbol, scriptNamer))
                .ToImmutableArray();

            // process all of the types and members in referenced assemblies
            var indirectlyReferencedExternalSymbols = indirectlyReferencedExternalTypeSymbols
                .SelectMany(DiscoverTypeAndMembers)
                .Select(
                    symbol => new KeyValuePair<ISymbol, Lazy<string>>(
                        symbol,
                        new Lazy<string>(
                            () => scriptNamer.DetermineScriptNameForSymbol(symbol),
                            isThreadSafe: true)))
                .ToImmutableArray();

            return new ScriptNameSymbolTable(
                overrideSymbols?.ToImmutableArray() ?? ImmutableArray<KeyValuePair<string, string>>.Empty,
                documentSymbols,
                directlyReferencedExternalSymbols,
                indirectlyReferencedExternalSymbols);
        }

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        private static IEnumerable<KeyValuePair<ISymbol, string>> ProcessSymbolsInDocument(
            DocumentTranslationContext context,
            ScriptNamer scriptNamer,
            CancellationToken cancellationToken)
        {
            return context.RootSyntax

                // get all of the declared types
                .GetAllDeclaredTypes(context.SemanticModel, cancellationToken)

                // except delegates
                .Where(symbol => symbol.TypeKind != TypeKind.Delegate)

                // and get all of the members of the type that can have a script name
                .SelectMany(symbol => DiscoverScriptNameOnTypeAndMembers(symbol, scriptNamer));
        }

        private static IEnumerable<ISymbol> DiscoverTypeAndMembers(INamespaceOrTypeSymbol typeSymbol) =>
            typeSymbol.ToSingleEnumerable().Concat(typeSymbol.GetMembers().Where(ShouldProcessMember));

        private static IEnumerable<KeyValuePair<ISymbol, string>> DiscoverScriptNameOnTypeAndMembers(
            ITypeSymbol typeSymbol,
            ScriptNamer scriptNamer)
        {
            return DiscoverTypeAndMembers(typeSymbol)
                .Select(
                    symbol => new KeyValuePair<ISymbol, string>(
                        symbol,
                        scriptNamer.DetermineScriptNameForSymbol(symbol)));
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
                // skip constructors
                case MethodKind.Constructor:
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
