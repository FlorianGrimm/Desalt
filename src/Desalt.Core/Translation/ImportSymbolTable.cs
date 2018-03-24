// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Contains mappings of defined types in a C# project to the files in which they're defined.
    /// This is needed in order to correctly generate <c>import</c> statements at the top of each
    /// translated TypeScript file.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal class ImportSymbolTable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly ConcurrentDictionary<string, ImportSymbolInfo> _typeToFileMap =
            new ConcurrentDictionary<string, ImportSymbolInfo>();

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        public ImportSymbolInfo this[string symbolName]
        {
            get
            {
                if (!_typeToFileMap.TryGetValue(symbolName, out ImportSymbolInfo symbolInfo))
                {
                    throw new KeyNotFoundException();
                }

                return symbolInfo;
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        public void AddDefinedTypesInDocument(DocumentTranslationContext context)
        {
            ImportSymbolInfo symbolInfo = ImportSymbolInfo.CreateInternalReference(context.TypeScriptFilePath);

            var allTypeDeclarations = context.RootSyntax.DescendantNodes()
                .Select(
                    node =>
                    {
                        switch (node)
                        {
                            case BaseTypeDeclarationSyntax typeDeclaration:
                                return typeDeclaration.Identifier.Text;

                            case DelegateDeclarationSyntax delegateDeclaration:
                                return delegateDeclaration.Identifier.Text;

                            default:
                                return null;
                        }
                    })
                .Where(name => !string.IsNullOrWhiteSpace(name));

            foreach (string typeName in allTypeDeclarations)
            {
                _typeToFileMap.AddOrUpdate(typeName, _ => symbolInfo, (_, __) => symbolInfo);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the symbol table contains a definition for the
        /// specified symbol name.
        /// </summary>
        public bool HasSymbol(string symbolName)
        {
            return _typeToFileMap.TryGetValue(symbolName, out ImportSymbolInfo _);
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
