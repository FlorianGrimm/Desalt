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

        private readonly ConcurrentDictionary<string, string> _typeToFileMap =
            new ConcurrentDictionary<string, string>();

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        public string this[string symbolName]
        {
            get
            {
                if (!_typeToFileMap.TryGetValue(symbolName, out string fileName))
                {
                    throw new KeyNotFoundException();
                }

                return fileName;
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
            string filePath = context.TypeScriptFilePath;

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
                _typeToFileMap.AddOrUpdate(typeName, _ => filePath, (_, __) => filePath);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the symbol table contains a definition for the
        /// specified symbol name.
        /// </summary>
        public bool HasSymbol(string symbolName)
        {
            return _typeToFileMap.TryGetValue(symbolName, out string _);
        }
    }
}
