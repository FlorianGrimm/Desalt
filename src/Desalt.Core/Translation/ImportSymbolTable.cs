// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Contains mappings of defined types in a C# project to the files in which they're defined.
    /// This is needed in order to correctly generate <c>import</c> statements at the top of each
    /// translated TypeScript file.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal class ImportSymbolTable : SymbolTable<ImportSymbolInfo>
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        public override void AddDefinedTypesInDocument(DocumentTranslationContext context)
        {
            ImportSymbolInfo symbolInfo = ImportSymbolInfo.CreateInternalReference(context.TypeScriptFilePath);

            IEnumerable<INamedTypeSymbol> allTypeDeclarationSymbols = context.RootSyntax.DescendantNodes()
                .Select(
                    node =>
                    {
                        switch (node)
                        {
                            case BaseTypeDeclarationSyntax typeDeclaration:
                                return context.SemanticModel.GetDeclaredSymbol(typeDeclaration);

                            case DelegateDeclarationSyntax delegateDeclaration:
                                return context.SemanticModel.GetDeclaredSymbol(delegateDeclaration);

                            default:
                                return null;
                        }
                    })
                .Where(symbol => symbol != null);

            foreach (INamedTypeSymbol typeSymbol in allTypeDeclarationSymbols)
            {
                AddOrUpdate(typeSymbol, symbolInfo);
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
    }
}
