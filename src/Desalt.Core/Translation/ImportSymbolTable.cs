// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Extensions;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains mappings of defined types in a C# project to the files in which they're defined.
    /// This is needed in order to correctly generate <c>import</c> statements at the top of each
    /// translated TypeScript file.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal class ImportSymbolTable : SymbolTable<ImportSymbolInfo>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private ImportSymbolTable(IEnumerable<KeyValuePair<string, ImportSymbolInfo>> values)
            : base(values)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static Task<ImportSymbolTable> CreateAsync(
            DocumentTranslationContext context,
            bool excludeExternalReferenceTypes = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return CreateAsync(context.ToSafeArray(), excludeExternalReferenceTypes, cancellationToken);
        }

        public static async Task<ImportSymbolTable> CreateAsync(
            IEnumerable<DocumentTranslationContext> documentsContexts,
            bool excludeExternalReferenceTypes = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var allSymbols = await DiscoverSymbolsAsync(
                documentsContexts,
                DiscoverSymbolsInDocument,
                excludeExternalReferenceTypes
                    ? (ProcessExternallyReferencedTypeFunc<ImportSymbolInfo>)null
                    : ProcessExternallyReferencedType,
                cancellationToken);

            return new ImportSymbolTable(allSymbols);
        }

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        private static IEnumerable<KeyValuePair<ISymbol, ImportSymbolInfo>> DiscoverSymbolsInDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            ImportSymbolInfo symbolInfo = ImportSymbolInfo.CreateInternalReference(context.TypeScriptFilePath);

            return context.RootSyntax.GetAllDeclaredTypes(context.SemanticModel, cancellationToken)
                .Select(typeSymbol => new KeyValuePair<ISymbol, ImportSymbolInfo>(typeSymbol, symbolInfo));
        }

        /// <summary>
        /// Adds all of the defined types in external assembly references to the mapping.
        /// </summary>
        private static IEnumerable<KeyValuePair<ISymbol, ImportSymbolInfo>> ProcessExternallyReferencedType(
            ITypeSymbol typeSymbol,
            CompilerOptions options,
            CancellationToken cancellationToken)
        {
            var containingAssembly = typeSymbol.ContainingAssembly;
            Debug.Assert(containingAssembly != null, $"{typeSymbol.Name}.ContainingAssembly is null");

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (containingAssembly != null)
            {
                string moduleName = containingAssembly.Name;
                var symbolInfo = ImportSymbolInfo.CreateExternalReference(moduleName);
                yield return new KeyValuePair<ISymbol, ImportSymbolInfo>(typeSymbol, symbolInfo);
            }
        }
    }

    /// <summary>
    /// Represents an imported symbol that is contained in an <see cref="ImportSymbolTable"/>.
    /// </summary>
    internal class ImportSymbolInfo
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private ImportSymbolInfo(string relativeTypeScriptFilePathOrModuleName, bool isInternalReference)
        {
            RelativeTypeScriptFilePathOrModuleName = relativeTypeScriptFilePathOrModuleName;
            IsInternalReference = isInternalReference;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string RelativeTypeScriptFilePathOrModuleName { get; }

        /// <summary>
        /// Returns a value indicating whether this is an internal reference, meaning that the type
        /// is defined within this project. An external reference is something from another assembly.
        /// </summary>
        public bool IsInternalReference { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ImportSymbolInfo CreateInternalReference(string typeScriptFilePath)
        {
            return new ImportSymbolInfo(typeScriptFilePath, isInternalReference: true);
        }

        public static ImportSymbolInfo CreateExternalReference(string moduleName)
        {
            return new ImportSymbolInfo(moduleName, isInternalReference: false);
        }
    }
}
