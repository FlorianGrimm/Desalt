// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTableUtils.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using CompilerUtilities.Extensions;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains static utility methods for using a symbol table. Pulled out into a separate class
    /// because statics aren't shared between generic instances.
    /// </summary>
    internal static class SymbolTableUtils
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// Used to cache externally-referenced assembly types so we only have to populate them up
        /// once since it can be expensive.
        /// </summary>
        private static ImmutableDictionary<IAssemblySymbol, ImmutableArray<INamedTypeSymbol>> s_assemblySymbols =
            ImmutableDictionary<IAssemblySymbol, ImmutableArray<INamedTypeSymbol>>.Empty;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Finds a Saltarelle attribute attached to a specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeNameMinusSuffix">
        /// The name of the attribute to find, minus the "Attribute" suffix. For example,
        /// "InlineCode", which represents the <c>System.Runtime.CompilerServices.InlineCodeAttribute</c>.
        /// </param>
        /// <returns>
        /// The found attribute or null if the symbol does not have an attached attribute of the
        /// given name.
        /// </returns>
        public static AttributeData FindSaltarelleAttribute(ISymbol symbol, string attributeNameMinusSuffix)
        {
            var format = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            string fullAttributeName = $"System.Runtime.CompilerServices.{attributeNameMinusSuffix}Attribute";
            AttributeData attributeData = symbol?.GetAttributes()
                .FirstOrDefault(x => x.AttributeClass.ToDisplayString(format) == fullAttributeName);

            return attributeData;
        }

        /// <summary>
        /// Gets the value of the Saltarelle attribute attached to a specified symbol or a default
        /// value if the attribute is not present.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeNameMinusSuffix">
        /// The name of the attribute to find, minus the "Attribute" suffix. For example,
        /// "InlineCode", which represents the <c>System.Runtime.CompilerServices.InlineCodeAttribute</c>.
        /// </param>
        /// <param name="defaultValue">
        /// The value to use if the attribute is not present on the symbol.
        /// </param>
        /// <returns>
        /// Either the value of the attribute or the default value if the attribute is not present on
        /// the symbol.
        /// </returns>
        public static string GetSaltarelleAttributeValueOrDefault(
            ISymbol symbol,
            string attributeNameMinusSuffix,
            string defaultValue)
        {
            AttributeData attributeData = FindSaltarelleAttribute(symbol, attributeNameMinusSuffix);
            return attributeData?.ConstructorArguments[0].Value.ToString() ?? defaultValue;
        }

        /// <summary>
        /// Gets all of the types defined in the assembly that are scriptable, meaning that they
        /// aren't decorated with a [NonScriptable] attribute. The results are cached between calls
        /// for efficiency and are retrieved in a thread-safe manner.
        /// </summary>
        /// <param name="assemblySymbol">The assembly to use for type lookup.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
        /// <returns>An immutable array of all the scriptable types in the assembly.</returns>
        public static ImmutableArray<INamedTypeSymbol> GetScriptableTypesInAssembly(
            IAssemblySymbol assemblySymbol,
            CancellationToken cancellationToken)
        {
            ImmutableArray<INamedTypeSymbol> FetchScriptableTypes(IAssemblySymbol _)
            {
                var visitor = new ScriptableTypesSymbolVisitor(cancellationToken);
                visitor.Visit(assemblySymbol);
                return visitor.ScriptableTypeSymbols;
            }

            return ImmutableInterlocked.GetOrAdd(ref s_assemblySymbols, assemblySymbol, FetchScriptableTypes);
        }

        /// <summary>
        /// Discovers all of the types that are directly referenced in the document, but live in
        /// external assemblies.
        /// </summary>
        /// <param name="contexts">The contexts from which to retrieve symbols.</param>
        /// <param name="discoveryKind">The kind of discovery to use (mainly for unit tests).</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
        /// <returns>
        /// All of the externally-referenced type symbols. If <paramref name="discoveryKind"/> is
        /// <see cref="SymbolTableDiscoveryKind.OnlyDocumentTypes"/>, then an empty array is returned
        /// and no work is done.
        /// </returns>
        public static ImmutableArray<ITypeSymbol> DiscoverDirectlyReferencedExternalTypes(
            ImmutableArray<DocumentTranslationContext> contexts,
            SymbolTableDiscoveryKind discoveryKind,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return discoveryKind == SymbolTableDiscoveryKind.OnlyDocumentTypes
                ? ImmutableArray<ITypeSymbol>.Empty
                : contexts.SelectMany(context => DiscoverDirectlyReferencedExternalTypes(context, cancellationToken))
                    .Distinct()
                    .ToImmutableArray();
        }

        /// <summary>
        /// Discovers all of the types that are directly referenced in the document, but live in
        /// external assemblies.
        /// </summary>
        /// <param name="context">The <see cref="DocumentTranslationContext"/> to discover.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
        /// <returns>All of the externally-referenced type symbols.</returns>
        public static ImmutableArray<ITypeSymbol> DiscoverDirectlyReferencedExternalTypes(
            DocumentTranslationContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // find all of the external type references in the document
            var walker = new ExternalTypeWalker(context.SemanticModel, cancellationToken);
            walker.Visit(context.RootSyntax);
            return walker.ExternalTypeSymbols.ToImmutableArray();
        }

        /// <summary>
        /// Discovers all of the types defined in assemblies directly referenced by the documents.
        /// Mscorlib is always discovered.
        /// </summary>
        /// <param name="externalSymbols">All of the externally referenced type symbols.</param>
        /// <param name="compilation">The compilation to use for looking up mscorlib.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
        /// <param name="discoveryKind">The kind of discovery to use (mainly for unit tests).</param>
        /// <returns>
        /// All of the type symbols defined in external assemblies. If <paramref
        /// name="discoveryKind"/> is anything other than <see
        /// cref="SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes"/>, an empty array is returned
        /// and no work is done.
        /// </returns>
        public static ImmutableArray<INamedTypeSymbol> DiscoverTypesInReferencedAssemblies(
            IEnumerable<ITypeSymbol> externalSymbols,
            Compilation compilation,
            CancellationToken cancellationToken = default(CancellationToken),
            SymbolTableDiscoveryKind discoveryKind = SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes)
        {
            if (discoveryKind != SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes)
            {
                return ImmutableArray<INamedTypeSymbol>.Empty;
            }

            // get mscorlib
            IAssemblySymbol mscorlib = compilation.GetSpecialType(SpecialType.System_Boolean).ContainingAssembly;

            IEnumerable<IAssemblySymbol> referencedAssemblySymbols = mscorlib.ToSingleEnumerable()
                .Concat(externalSymbols.Select(symbol => symbol.ContainingAssembly))
                .Distinct();

            // get all of the assembly types
            return referencedAssemblySymbols.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(assemblySymbol => GetScriptableTypesInAssembly(assemblySymbol, cancellationToken))
                .ToImmutableArray();
        }
    }
}
